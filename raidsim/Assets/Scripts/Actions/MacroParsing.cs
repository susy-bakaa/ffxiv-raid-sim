// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Globalization;

namespace dev.susybaka.raidsim.Actions
{
    public static class MacroParsing
    {
        public static string NormalizeNewlines(string s) => (s ?? "").Replace("\r\n", "\n");

        public static string ClampTo15Lines(string body)
        {
            body = NormalizeNewlines(body);
            var lines = body.Split('\n');
            if (lines.Length <= 15)
                return body;
            return string.Join("\n", lines, 0, 15);
        }

        // Finds first /micon or /macroicon line and extracts ActionId token.
        // Accepts: /micon Sprint
        //          /micon "Sprint"
        //          /macroicon Sprint
        public static bool TryExtractMiconActionId(string body, out string actionId)
        {
            actionId = null;
            body = NormalizeNewlines(body);
            var lines = body.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.Length == 0)
                    continue;

                if (StartsWithCmd(line, "/micon") || StartsWithCmd(line, "/macroicon"))
                {
                    // Remove command word
                    var rest = line;
                    rest = RemoveLeadingCmd(rest, "/micon");
                    rest = RemoveLeadingCmd(rest, "/macroicon");
                    rest = rest.Trim();

                    if (rest.StartsWith("\""))
                    {
                        int end = rest.IndexOf('"', 1);
                        if (end > 1)
                            actionId = rest.Substring(1, end - 1).Trim();
                    }
                    else
                    {
                        // token until whitespace
                        int sp = rest.IndexOf(' ');
                        actionId = (sp >= 0 ? rest.Substring(0, sp) : rest).Trim();
                    }

                    return !string.IsNullOrWhiteSpace(actionId);
                }
            }

            return false;
        }

        public static bool IsMacroMetaLine(string line)
        {
            line = (line ?? "").Trim();
            if (line.Length == 0)
                return true;
            return StartsWithCmd(line, "/micon") || StartsWithCmd(line, "/macroicon");
        }

        private static bool StartsWithCmd(string line, string cmd)
            => line.StartsWith(cmd, true, CultureInfo.InvariantCulture);

        private static string RemoveLeadingCmd(string line, string cmd)
        {
            if (!StartsWithCmd(line, cmd))
                return line;
            return line.Substring(cmd.Length);
        }
    }
}