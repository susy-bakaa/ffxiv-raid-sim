// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
using static dev.susybaka.raidsim.Core.GlobalData;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace dev.susybaka.raidsim.Actions
{
    public static class MacroShareCode
    {
        // Prefix so we can detect + change formats later
        private const string Prefix = "MC1."; // MacroCode v1

        public static string Encode(MacroLibrarySnapshot snapshot)
        {
            var json = JsonUtility.ToJson(snapshot);
            var utf8 = Encoding.UTF8.GetBytes(json);

            byte[] compressed;
            using (var ms = new MemoryStream())
            {
                using (var brotli = new BrotliStream(ms, CompressionLevel.Optimal, leaveOpen: true))
                    brotli.Write(utf8, 0, utf8.Length);

                compressed = ms.ToArray();
            }

            // Base64URL (no + /, no padding)
            var b64 = Convert.ToBase64String(compressed);
            var b64url = b64.Replace('+', '-').Replace('/', '_').TrimEnd('=');

            return Prefix + b64url;
        }

        public static bool TryDecode(string code, out MacroLibrarySnapshot snapshot)
        {
            snapshot = null;
            if (string.IsNullOrWhiteSpace(code))
                return false;

            code = code.Trim();
            if (!code.StartsWith(Prefix, StringComparison.Ordinal))
                return false;

            var b64url = code.Substring(Prefix.Length);
            var b64 = b64url.Replace('-', '+').Replace('_', '/');

            // restore padding
            switch (b64.Length % 4)
            {
                case 2:
                    b64 += "==";
                    break;
                case 3:
                    b64 += "=";
                    break;
                case 0:
                    break;
                default:
                    return false; // invalid length
            }

            byte[] compressed;
            try
            { compressed = Convert.FromBase64String(b64); }
            catch (FormatException) { return false; }

            try
            {
                using var input = new MemoryStream(compressed);
                using var brotli = new BrotliStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();
                brotli.CopyTo(output);

                var json = Encoding.UTF8.GetString(output.ToArray());
                snapshot = JsonUtility.FromJson<MacroLibrarySnapshot>(json);
                return snapshot != null && snapshot.slots != null;
            }
            catch
            {
                return false;
            }
        }
    }
}