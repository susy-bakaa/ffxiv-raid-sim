using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace dev.susybaka.Shared.Editor
{
    public sealed class ObsWsClient : IDisposable
    {
        private ClientWebSocket _ws;
        private CancellationTokenSource _cts;
        private Task _receiveLoop;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<JObject>> _pending =
            new ConcurrentDictionary<string, TaskCompletionSource<JObject>>();

        public bool IsConnected => _ws != null && _ws.State == WebSocketState.Open;

        public async Task<bool> ConnectAsync(string host, int port, string password, int rpcVersion = 1)
        {
            if (IsConnected)
                return true;

            _ws = new ClientWebSocket();
            _ws.Options.AddSubProtocol("obswebsocket.json"); // default is JSON over text anyway :contentReference[oaicite:6]{index=6}
            _cts = new CancellationTokenSource();

            try
            {
                var uri = new Uri($"ws://{host}:{port}");
                await _ws.ConnectAsync(uri, _cts.Token).ConfigureAwait(false);

                // 1) Receive Hello (op=0) :contentReference[oaicite:7]{index=7}
                var hello = await ReceiveJsonAsync(_cts.Token).ConfigureAwait(false);
                if ((int)hello["op"] != 0)
                    throw new Exception("Expected OBS Hello (op=0).");

                var d = (JObject)hello["d"];
                var serverRpc = (int)d["rpcVersion"];
                var negotiatedRpc = Math.Min(rpcVersion, serverRpc);

                string auth = null;
                var authObj = d["authentication"] as JObject;
                if (authObj != null)
                {
                    var challenge = (string)authObj["challenge"];
                    var salt = (string)authObj["salt"];
                    auth = ComputeAuth(password, salt, challenge); // per protocol :contentReference[oaicite:8]{index=8}
                }

                // 2) Send Identify (op=1) :contentReference[oaicite:9]{index=9}
                var identifyD = new JObject
                {
                    ["rpcVersion"] = negotiatedRpc
                    // omit eventSubscriptions to keep defaults; not needed for start/stop
                };
                if (!string.IsNullOrEmpty(auth))
                    identifyD["authentication"] = auth;

                var identify = new JObject
                {
                    ["op"] = 1,
                    ["d"] = identifyD
                };
                await SendJsonAsync(identify, _cts.Token).ConfigureAwait(false);

                // 3) Receive Identified (op=2) :contentReference[oaicite:10]{index=10}
                var identified = await ReceiveJsonAsync(_cts.Token).ConfigureAwait(false);
                if ((int)identified["op"] != 2)
                    throw new Exception("Expected OBS Identified (op=2).");

                // Start receive loop for RequestResponse (op=7)
                _receiveLoop = Task.Run(() => ReceiveLoop(_cts.Token));

                return true;
            }
            catch
            {
                Dispose();
                return false;
            }
        }

        public async Task StartRecordAsync()
        {
            // StartRecord request :contentReference[oaicite:11]{index=11}
            await SendRequestAsync("StartRecord", null).ConfigureAwait(false);
        }

        public async Task<string> StopRecordAsync()
        {
            // StopRecord returns outputPath :contentReference[oaicite:12]{index=12}
            var resp = await SendRequestAsync("StopRecord", null).ConfigureAwait(false);
            var data = resp["d"]?["responseData"] as JObject;
            return (string)data?["outputPath"];
        }

        public async Task<JObject> SendRequestAsync(string requestType, JObject requestData)
        {
            if (!IsConnected)
                throw new InvalidOperationException("OBS websocket not connected.");

            var requestId = Guid.NewGuid().ToString("N");
            var tcs = new TaskCompletionSource<JObject>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[requestId] = tcs;

            var reqD = new JObject
            {
                ["requestType"] = requestType,
                ["requestId"] = requestId
            };
            if (requestData != null)
                reqD["requestData"] = requestData;

            var req = new JObject
            {
                ["op"] = 6,
                ["d"] = reqD
            };

            await SendJsonAsync(req, _cts.Token).ConfigureAwait(false);
            var msg = await tcs.Task.ConfigureAwait(false);

            // Check requestStatus.result
            var status = msg["d"]?["requestStatus"] as JObject;
            if (status != null && status.TryGetValue("result", out var resultTok) && resultTok.Type == JTokenType.Boolean)
            {
                if (!(bool)resultTok)
                {
                    var code = (int?)status["code"];
                    var comment = (string)status["comment"];
                    throw new Exception($"OBS request '{requestType}' failed (code {code}): {comment}");
                }
            }

            return msg;
        }

        private async Task ReceiveLoop(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && IsConnected)
                {
                    var msg = await ReceiveJsonAsync(ct).ConfigureAwait(false);
                    var op = (int?)msg["op"];

                    // RequestResponse (op=7) :contentReference[oaicite:13]{index=13}
                    if (op == 7)
                    {
                        var requestId = (string)msg["d"]?["requestId"];
                        if (!string.IsNullOrEmpty(requestId) && _pending.TryRemove(requestId, out var tcs))
                            tcs.TrySetResult(msg);
                    }
                    // You can also handle events here (op=5) if you want. :contentReference[oaicite:14]{index=14}
                }
            }
            catch
            {
                // swallow; connection is likely closed
            }
        }

        private async Task SendJsonAsync(JObject obj, CancellationToken ct)
        {
            var bytes = Encoding.UTF8.GetBytes(obj.ToString(Newtonsoft.Json.Formatting.None));
            await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, ct).ConfigureAwait(false);
        }

        private async Task<JObject> ReceiveJsonAsync(CancellationToken ct)
        {
            var sb = new StringBuilder();
            var buffer = new byte[8192];

            while (true)
            {
                var res = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct).ConfigureAwait(false);
                if (res.MessageType == WebSocketMessageType.Close)
                    throw new WebSocketException("OBS websocket closed.");

                sb.Append(Encoding.UTF8.GetString(buffer, 0, res.Count));
                if (res.EndOfMessage)
                    break;
            }

            return JObject.Parse(sb.ToString());
        }

        private static string ComputeAuth(string password, string salt, string challenge)
        {
            // per obs-websocket 5.x protocol :contentReference[oaicite:15]{index=15}
            var secret = Sha256Base64(password + salt);
            return Sha256Base64(secret + challenge);
        }

        private static string Sha256Base64(string input)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hash);
        }

        public void Dispose()
        {
            try
            { _cts?.Cancel(); }
            catch { }
            try
            { _ws?.Abort(); }
            catch { }
            try
            { _ws?.Dispose(); }
            catch { }
            _ws = null;

            try
            { _cts?.Dispose(); }
            catch { }
            _cts = null;

            _pending.Clear();
        }
    }
}