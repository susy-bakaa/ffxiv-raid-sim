using Lumina;
using Lumina.Data;
using Lumina.Excel.Sheets;

namespace xivAnim
{
    public static class Utility
    {
        /// <summary>
        /// motionNames: e.g. ["cbbm_id0", "cbbm_sp_b_1", "cbbm_sp_b_2lp" ...]
        /// Returns a dict mapping those motion names to IsLoop (true/false) based on the MotionTimeline sheet.
        /// </summary>
        public static Dictionary<string, bool> BuildLoopMapForMotions(GameData lumina, IEnumerable<string> motionNames)
        {
            var wanted = new HashSet<string>(motionNames, StringComparer.OrdinalIgnoreCase);
            var result = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            Console.WriteLine($"  Building loop map for {wanted.Count} motions...");

            var sheet = lumina.GetExcelSheet<MotionTimeline>(Language.English);
            if (sheet == null)
                throw new InvalidOperationException("MotionTimeline sheet not found via Lumina.");
            else
                Console.WriteLine($"    MotionTimeline excel sheet successfully loaded via Lumina.");

            foreach (var row in sheet)
            {
                string key = row.Filename.ExtractText();
                bool isLoop = row.IsLoop;

                if (string.IsNullOrWhiteSpace(key))
                    continue;

                if (!wanted.Contains(key))
                    continue;

                Console.WriteLine($"    Found motion '{key}': IsLoop = {isLoop}");

                result[key] = isLoop;
            }

            Console.WriteLine($"  Loop map successfully built with {result.Count} entries.");
            return result;
        }
    }

}
