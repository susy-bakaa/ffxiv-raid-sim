using System.Diagnostics;
using System.Text.RegularExpressions;
using Lumina;
using Lumina.Data;
using Lumina.Excel.Sheets;

namespace dev.susy_baka.xivAnim.Core
{
    public static class Utility
    {
        /// <summary>
        /// Replaces invalid characters in a file name with underscores.
        /// </summary>
        /// <remarks>This method uses <see cref="Path.GetInvalidFileNameChars"/> to determine the set of
        /// invalid characters.</remarks>
        /// <param name="name">The file name to sanitize. Cannot be <see langword="null"/>.</param>
        /// <returns>A sanitized version of the file name where all invalid characters are replaced with underscores.</returns>
        public static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }

        /// <summary>
        /// Returns a dictionary mapping motion names to their loop status (true/false) using the MotionTimeline sheet.
        /// </summary>
        /// <remarks>
        /// This method queries the <see cref="MotionTimeline"/> Excel sheet via Lumina and matches the provided motion names.
        /// </remarks>
        /// <param name="lumina">The Lumina <see cref="GameData"/> instance used to access Excel sheets. Cannot be <see langword="null"/>.</param>
        /// <param name="motionNames">A collection of motion names to look up. Cannot be <see langword="null"/>.</param>
        /// <returns>
        /// A dictionary where each key is a motion name and the value indicates whether the motion is a loop.
        /// </returns>
        public static Dictionary<string, bool> BuildLoopMapForMotions(GameData lumina, IEnumerable<string> motionNames)
        {
            var wanted = new HashSet<string>(motionNames, StringComparer.OrdinalIgnoreCase);
            var result = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            Log.Info($"  Building loop map for {wanted.Count} motions...");

            var sheet = lumina.GetExcelSheet<MotionTimeline>(Language.English);
            if (sheet == null)
            {
                Log.Error("MotionTimeline excel sheet not found via Lumina.");
                throw new InvalidOperationException("MotionTimeline sheet not found via Lumina.");
            }
            else
            {
                Log.Info("    MotionTimeline excel sheet successfully loaded via Lumina.");
            }

            foreach (var row in sheet)
            {
                string key = row.Filename.ExtractText();
                bool isLoop = row.IsLoop;

                if (string.IsNullOrWhiteSpace(key))
                    continue;

                if (!wanted.Contains(key))
                    continue;

                Log.Info($"    Found motion '{key}': IsLoop = {isLoop}");

                result[key] = isLoop;
            }

            Log.Info($"  Loop map successfully built with {result.Count} entries.");
            return result;
        }

        public sealed class ProcessResult
        {
            public int ExitCode { get; init; }
            public string StdOut { get; init; } = "";
            public string StdErr { get; init; } = "";
        }

        /// <summary>
        /// Executes an external process with the specified executable and arguments, capturing its standard output and error streams.
        /// </summary>
        /// <remarks>
        /// This method uses <see cref="ProcessStartInfo"/> to configure the process execution environment, disables shell execution,
        /// and redirects both standard output and error. The working directory is set to the executable's directory or the current environment directory.
        /// </remarks>
        /// <param name="executable">The path to the executable to run. Cannot be <see langword="null"/>.</param>
        /// <param name="args">A collection of arguments to pass to the process. Cannot be <see langword="null"/>.</param>
        /// <returns>
        /// A <see cref="ProcessResult"/> containing the process exit code, standard output, and standard error.
        /// </returns>
        public static ProcessResult RunProcess(string executable, IEnumerable<string> args, CancellationToken token)
        {
            var psi = new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(executable) ?? Environment.CurrentDirectory
            };
            foreach (var arg in args)
                psi.ArgumentList.Add(arg);

            // Debug logging of the executed command
            //Log.Info($"[{Path.GetFileName(executable)}] {executable} {string.Join(" ", psi.ArgumentList)}");

            return RunProcess(psi, token);
        }

        private static ProcessResult RunProcess(ProcessStartInfo psi, CancellationToken token)
        {
            using var proc = new Process { StartInfo = psi };

            proc.Start();

            using (token.Register(() =>
            {
                try
                {
                    if (!proc.HasExited)
                    {
                        Log.Info($"Cancelling process {proc.StartInfo.FileName}...");
                        proc.Kill(entireProcessTree: true);
                    }
                }
                catch { /* ignore */ }
            }))
            {
                var stdoutTask = proc.StandardOutput.ReadToEndAsync();
                var stderrTask = proc.StandardError.ReadToEndAsync();

                proc.WaitForExit();

                Task.WaitAll(stdoutTask, stderrTask);

                var stdout = stdoutTask.Result;
                var stderr = stderrTask.Result;

                if (token.IsCancellationRequested)
                    throw new OperationCanceledException(token);

                return new ProcessResult
                {
                    ExitCode = proc.ExitCode,
                    StdOut = stdout,
                    StdErr = stderr
                };
            }
        }

        public sealed class PapAnimInfo
        {
            public int AnimCount { get; init; }
            public List<string> Names { get; init; } = new();
        }

        /// <summary>
        /// Executes MultiAssist with the specified skeleton and PAP files, using the XML output mode,
        /// and parses the resulting console output to extract both the total animation count and the list of animation names.
        /// </summary>
        /// <remarks>
        /// This method runs MultiAssist with the <c>extract</c> command, passing the skeleton and PAP file paths,
        /// and requests XML output to a temporary file (which is deleted after execution). It then analyzes the combined
        /// standard output and error streams to locate the <c>anim_count</c> value and the <c>anim_infos</c> line,
        /// extracting all animation names found. The returned <see cref="PapAnimInfo"/> contains both the count and names.
        /// </remarks>
        /// <param name="multiAssistPath">The path to the MultiAssist executable. Cannot be <see langword="null"/>.</param>
        /// <param name="skeletonFile">The path to the skeleton file to use. Cannot be <see langword="null"/>.</param>
        /// <param name="papFile">The path to the PAP file to analyze. Cannot be <see langword="null"/>.</param>
        /// <returns>
        /// A <see cref="PapAnimInfo"/> containing the total number of animations and a list of their names,
        /// as parsed from MultiAssist's console output.
        /// </returns>
        public static PapAnimInfo GetPapAnimInfo(string multiAssistPath, string skeletonFile, string papFile, CancellationToken token)
        {
            // Dummy output path since we don't care about the XML file itself.
            var tmpOut = Path.GetTempFileName();
            
            List<string> args = new List<string>()
            {
                "extract",
                "-s", skeletonFile,
                "-p", papFile,
                "-i", "0",
                "-t", "xml",
                "-o", tmpOut
            };

            // The old way of building args as a single string (not used anymore):
            // var args = $"extract -s \"{skeletonFile}\" -p \"{papFile}\" -i 0 -t xml -o \"{tmpOut}\"";

            var result = RunProcess(multiAssistPath, args, token);

            // We don't actually need the file, only the logs.
            try
            { 
                File.Delete(tmpOut); 
            }
            catch { /* Ignore */ }

            var combined = (result.StdOut ?? "") + "\n" + (result.StdErr ?? "");

            // First try to find the anim_count
            var countMatch = Regex.Match(combined, @"anim_count\s*:\s*(\d+)");
            int animCount = 0;
            if (countMatch.Success && int.TryParse(countMatch.Groups[1].Value, out var parsed))
                animCount = parsed;

            // Secondly try to find the animation names from anim_infos
            // We search the *last* line that contains 'anim_infos' and pull every `'name': 'xxx'`.
            string? animLine = combined
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault(l => l.Contains("anim_infos"));

            var names = new List<string>();
            if (!string.IsNullOrEmpty(animLine))
            {
                var matches = Regex.Matches(animLine, @"'name':\s*'([^']+)'");
                foreach (Match m in matches)
                {
                    names.Add(m.Groups[1].Value);
                }
            }

            return new PapAnimInfo
            {
                AnimCount = animCount > 0 ? animCount : names.Count,
                Names = names
            };
        }

        /// <summary>
        /// Constructs an output file name based on the original file path, a target file name, and a set of path patterns.
        /// </summary>
        /// <remarks>
        /// This method sanitizes the provided <paramref name="fileName"/> by removing its extension and replacing invalid characters.
        /// It then checks if the normalized original file path matches any of the regular expressions in <paramref name="appendNamesForPaths"/>.
        /// If a match is found, the output file name is constructed by appending the sanitized file name to the original file name (without extension),
        /// separated by an underscore, and followed by the original extension. If no match is found, only the sanitized file name and extension are used.
        /// </remarks>
        /// <param name="originalFilePath">The path to the original file. Cannot be <see langword="null"/>.</param>
        /// <param name="fileName">The target file name to use. Cannot be <see langword="null"/>.</param>
        /// <param name="appendNamesForPaths">A list of regular expression patterns to match against the normalized original file path. Cannot be <see langword="null"/>.</param>
        /// <returns>
        /// A string representing the constructed output file name, formatted according to the matching rules.
        /// </returns>
        public static string BuildOutputFileName(string originalFilePath, string fileName, List<string> appendNamesForPaths)
        {
            string originalFileName = Path.GetFileNameWithoutExtension(originalFilePath);
            string extension = Path.GetExtension(fileName);

            fileName = SanitizeFileName(Path.GetFileNameWithoutExtension(fileName));

            // normalize *once*
            string normalizedOriginalPath = originalFilePath.Replace('\\', '/');

            for (int i = 0; i < appendNamesForPaths.Count; i++)
            {
                if (Regex.IsMatch(normalizedOriginalPath, appendNamesForPaths[i], RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    return $"{originalFileName}_{fileName}{extension}";
            }

            return $"{fileName}{extension}";
        }
    }
}
