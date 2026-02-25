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