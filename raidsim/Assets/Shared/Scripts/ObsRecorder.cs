using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace dev.susybaka.Shared.Editor
{
#if UNITY_EDITOR
    public class ObsRecorder : MonoBehaviour
    {
        public bool _enabled = true;

        [Header("OBS WebSocket")]
        public string host = "127.0.0.1";
        public int port = 4455;
        [Tooltip("OBS WebSocket password (Tools -> WebSocket Server Settings).")]
        public string password = "";

        [Header("Determinism")]
        public bool freezeTimeUntilRecording = true;
        [Tooltip("Extra real-time wait after OBS confirms StartRecord.")]
        public float prerollSeconds = 0.10f;

        [Header("Overwrite a stable filename (optional)")]
        public bool overwriteStableFile = true;
        public string stableFileName = "unity_latest.mp4";
        public string stableOutputFolder = ""; // empty = same folder as OBS output file

        [Header("Hooks")]
        public UnityEvent onBeginRecording; // optional
        public UnityEvent onRecordingArmed; // start your sequence here
        public UnityEvent onObsUnavailable; // optional
        public UnityEvent onEndRecording;

        private ObsWsClient _obs;
        private bool _isRecording;

        // Monitor play mode state to end recording if user stops play mode:
        private void OnApplicationQuit()
        {
            if (_isRecording)
            {
                EndRecording();
            }
        }

        private void Update()
        {
            // Optional: monitor for state changes
            if (_isRecording && !_enabled)
            {
                EndRecording();
            }
        }

        // Call this from a UnityEvent / button / hotkey:
        [ContextMenu("Begin Recording")]
        public void BeginRecording()
        {
            if (!_enabled)
                return;

            onBeginRecording?.Invoke();
            StartCoroutine(BeginRoutine());
        }

        // Call this from a UnityEvent when your run is done:
        [ContextMenu("End Recording")]
        public void EndRecording()
        {
            if (!_enabled)
                return;

            onEndRecording?.Invoke();
            StartCoroutine(EndRoutine());
        }

        private System.Collections.IEnumerator BeginRoutine()
        {
            float oldTimeScale = Time.timeScale;
            if (freezeTimeUntilRecording)
                Time.timeScale = 0f;

            var task = BeginAsync();
            while (!task.IsCompleted)
                yield return null;

            if (task.Result)
            {
                // preroll in real-time
                var end = Time.realtimeSinceStartup + prerollSeconds;
                while (Time.realtimeSinceStartup < end)
                    yield return null;

                if (freezeTimeUntilRecording)
                    Time.timeScale = oldTimeScale;
                onRecordingArmed?.Invoke();
            }
            else
            {
                if (freezeTimeUntilRecording)
                    Time.timeScale = oldTimeScale;
                onObsUnavailable?.Invoke();
            }
        }

        private async Task<bool> BeginAsync()
        {
            try
            {
                _obs ??= new ObsWsClient();
                var ok = await _obs.ConnectAsync(host, port, password);
                if (!ok)
                    return false;

                await _obs.StartRecordAsync(); // StartRecord :contentReference[oaicite:16]{index=16}
                _isRecording = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private System.Collections.IEnumerator EndRoutine()
        {
            var task = EndAsync();
            while (!task.IsCompleted)
                yield return null;
        }

        private async Task EndAsync()
        {
            if (_obs == null || !_isRecording)
                return;

            try
            {
                var outputPath = await _obs.StopRecordAsync(); // StopRecord returns outputPath :contentReference[oaicite:17]{index=17}
                _isRecording = false;

                if (overwriteStableFile && !string.IsNullOrEmpty(outputPath))
                    TryOverwriteStableFile(outputPath);
            }
            catch
            {
                // ignore
            }
        }

        private void TryOverwriteStableFile(string obsOutputPath)
        {
            // outputPath is the saved recording path per protocol :contentReference[oaicite:18]{index=18}
            if (!File.Exists(obsOutputPath))
                return;

            string folder = stableOutputFolder;
            if (string.IsNullOrWhiteSpace(folder))
                folder = Path.GetDirectoryName(obsOutputPath);

            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(obsOutputPath);
            var dest = Path.Combine(folder, Path.ChangeExtension(stableFileName, ext));

            try
            {
                if (File.Exists(dest))
                    File.Delete(dest);
                File.Move(obsOutputPath, dest);
            }
            catch
            {
                // if move fails (locked, permissions), you can fallback to File.Copy
            }
        }

        private void OnDisable()
        {
            _enabled = false;
        }
    }
#else
    public class ObsRecorder : MonoBehaviour
    {
        // empty stub for non-editor platforms
    }
#endif
}