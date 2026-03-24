// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace dev.susybaka.raidsim.Actions
{
    public static class MacroParsing
    {
        public enum WaitRoundingMode
        {
            Ceil,   // 2.5 -> 3
            Round,  // 2.5 -> 3
            Floor   // 2.5 -> 2
        }

        public static string NormalizeNewlines(string s) => (s ?? "").Replace("\r\n", "\n");

        public static string ClampTo15Lines(string body)
        {
            body = NormalizeNewlines(body);
            var lines = body.Split('\n');
            if (lines.Length <= 15)
                return body;
            return string.Join("\n", lines, 0, 15);
        }

        public static bool TryExtractMicon(string body, out string name, out MacroMiconType type)
        {
            name = null;
            type = MacroMiconType.None;

            body = NormalizeNewlines(body);
            var lines = body.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.Length == 0)
                    continue;

                if (!StartsWithCmd(line, "/micon") && !StartsWithCmd(line, "/macroicon"))
                    continue;

                var rest = line;
                rest = RemoveLeadingCmd(rest, "/micon");
                rest = RemoveLeadingCmd(rest, "/macroicon");
                rest = rest.Trim();

                // First arg: quoted or token
                if (rest.StartsWith("\""))
                {
                    int end = rest.IndexOf('"', 1);
                    if (end > 1)
                    {
                        name = rest.Substring(1, end - 1).Trim();
                        rest = rest.Substring(end + 1).Trim();
                    }
                }
                else
                {
                    int sp = rest.IndexOf(' ');
                    name = (sp >= 0 ? rest.Substring(0, sp) : rest).Trim();
                    rest = (sp >= 0 ? rest.Substring(sp + 1).Trim() : "");
                }

                // Second arg: type token (optional, default action)
                var typeTok = "";
                if (!string.IsNullOrWhiteSpace(rest))
                {
                    int sp = rest.IndexOf(' ');
                    typeTok = (sp >= 0 ? rest.Substring(0, sp) : rest).Trim();
                }

                type = ParseMiconType(typeTok);
                if (type == MacroMiconType.None)
                    type = MacroMiconType.Action; // default

                return !string.IsNullOrWhiteSpace(name);
            }

            return false;
        }

        private static MacroMiconType ParseMiconType(string tok)
        {
            tok = (tok ?? "").Trim().ToLowerInvariant();
            return tok switch
            {
                "action" => MacroMiconType.Action,
                "waymark" => MacroMiconType.Waymark,
                "enemysign" => MacroMiconType.Sign,
                "" => MacroMiconType.None,
                _ => MacroMiconType.None
            };
        }

        public static bool IsMacroMetaLine(string line)
        {
            line = (line ?? "").Trim();
            if (line.Length == 0)
                return true;
            return StartsWithCmd(line, "/micon") || StartsWithCmd(line, "/macroicon");
        }

        /// <summary>
        /// Standalone wait command. Returns true if line is /wait (even if invalid/missing value).
        /// </summary>
        public static bool TryParseWaitLine(string line, out float waitSeconds, bool integerOnly, WaitRoundingMode roundingMode)
        {
            waitSeconds = 0f;
            if (string.IsNullOrWhiteSpace(line))
                return false;

            var t = line.Trim();
            if (!StartsWithCmd(t, "/wait"))
                return false;

            var parts = t.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            // "/wait" alone => wait 0
            if (parts.Length < 2)
                return true;

            if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var s))
                s = 0f;

            waitSeconds = ApplyWaitRounding(s, integerOnly, roundingMode);
            waitSeconds = ClampWait(waitSeconds);
            return true;
        }

        /// <summary>
        /// Extracts the FIRST inline <wait.X> token (if any), removes it from the line, and returns wait seconds.
        /// Extra <wait.X> tokens are ignored (first wins).
        /// </summary>
        public static bool TryExtractInlineWait(ref string line, out float waitSeconds, bool integerOnly, WaitRoundingMode roundingMode)
        {
            waitSeconds = 0f;
            if (string.IsNullOrWhiteSpace(line))
                return false;

            var m = InlineWaitRegex.Match(line);
            if (!m.Success)
                return false;

            var raw = m.Groups["t"].Value;
            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var s))
            {
                waitSeconds = ApplyWaitRounding(s, integerOnly, roundingMode);
                waitSeconds = ClampWait(waitSeconds);
            }

            // Remove only the first match
            line = line.Remove(m.Index, m.Length);
            return waitSeconds > 0f;
        }

        /// <summary>
        /// Cheap detector so you can decide whether you need coroutine execution.
        /// </summary>
        public static bool LineContainsWaitDirective(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            var t = line.Trim();
            return StartsWithCmd(t, "/wait") || t.IndexOf("<wait.", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool StartsWithCmd(string line, string cmd)
            => line.StartsWith(cmd, true, CultureInfo.InvariantCulture);

        private static string RemoveLeadingCmd(string line, string cmd)
        {
            if (!StartsWithCmd(line, cmd))
                return line;
            return line.Substring(cmd.Length);
        }

        private static readonly Regex InlineWaitRegex =
            new Regex(@"<\s*wait\.(?<t>\d+(\.\d+)?)\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static float ClampWait(float seconds)
        {
            if (seconds < 0f)
                seconds = 0f;
            if (seconds > 60f)
                seconds = 60f;
            return seconds;
        }

        private static float ApplyWaitRounding(float seconds, bool integerOnly, WaitRoundingMode mode)
        {
            seconds = ClampWait(seconds);

            if (!integerOnly)
                return seconds;

            // Integer-only: parse float but round it
            return mode switch
            {
                WaitRoundingMode.Ceil => (float)Math.Ceiling(seconds),
                WaitRoundingMode.Floor => (float)Math.Floor(seconds),
                _ => (float)Math.Round(seconds, MidpointRounding.AwayFromZero),
            };
        }
    }
}