using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Lumina;
using Lumina.Data;

public class Config
{
    public string gamePath { get; set; } = "";
    public string multiAssistExe { get; set; } = "";
    public string blenderExe { get; set; } = "";
    public string outputRoot { get; set; } = "";
    public List<BossJob> jobs { get; set; } = new();
}

public class BossJob
{
    public string name { get; set; } = "";
    public string skeletonGamePath { get; set; } = "";
    public string modelPath { get; set; } = "";
    public List<string> papGamePaths { get; set; } = new();
    public List<string> appendFileNames { get; set; } = new();
}

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: xivAnim config.json");
            return 1;
        }

        var configPath = args[0];
        var cfg = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))
                  ?? throw new InvalidOperationException("Invalid config");

        var gameData = new GameData(cfg.gamePath); // Lumina

        foreach (var job in cfg.jobs)
        {
            Console.WriteLine($"=== {job.name} ===");

            var jobRoot = Path.Combine(cfg.outputRoot, job.name);
            Directory.CreateDirectory(jobRoot);

            // Create subdirs
            var skeletonDir = Path.Combine(jobRoot, "Skeleton");
            Directory.CreateDirectory(skeletonDir);
            var papDir = Path.Combine(jobRoot, "Animations");
            Directory.CreateDirectory(papDir);
            var outDir = Path.Combine(jobRoot, "Exported Animations");
            Directory.CreateDirectory(outDir);

            // Ensure skeleton + pap files exist locally
            var skeletonFile = ExtractRaw(gameData, job.skeletonGamePath, skeletonDir);

            foreach (var papGamePath in job.papGamePaths)
            {
                var papFile = ExtractRaw(gameData, papGamePath, papDir);
                var papBase = Path.GetFileNameWithoutExtension(papFile);

                Console.WriteLine($"  Processing {papBase}...");

                ExportAllAnimationsForPap(
                    cfg.multiAssistExe,
                    skeletonFile,
                    papFile,
                    outDir,
                    job.appendFileNames
                );
            }

            // Optionally run Blender automation
            if (!string.IsNullOrWhiteSpace(cfg.blenderExe))
            {
                Console.WriteLine("  Running Blender automation...");
                
                var baseFbx = Path.Combine(jobRoot, job.modelPath);

                RunBlenderAutomation(
                    cfg.blenderExe,
                    baseFbx,
                    outDir,
                    jobRoot
                );
            }
        }

        return 0;
    }

    /// <summary>
    /// Using Lumina: read a game path and dump it to disk.
    /// </summary>
    private static string ExtractRaw(GameData gameData, string gamePath, string rawRoot)
    {
        var file = gameData.GetFile(gamePath); // generic file
        var data = file?.Data; // byte[]
        var dest = Path.Combine(rawRoot, Path.GetFileName(gamePath));
        if (data != null)
        {
            Console.WriteLine($"  Extracting {gamePath} via Lumina...");
            File.WriteAllBytes(dest, data);
        }
        else
        {
            Console.WriteLine($"  WARN: Could not extract {gamePath} via Lumina, attempting local file copy...");

            Directory.CreateDirectory(rawRoot);
            if (!File.Exists(gamePath))
                throw new FileNotFoundException($"Raw file not found (for now expecting local path): {gamePath}");

            if (!File.Exists(dest))
                File.Copy(gamePath, dest);
        }

        return dest;
    }

    private static void ExportAllAnimationsForPap(string multiAssistExe, string skeletonFile, string papFile, string outputDir, List<string> appendNames)
    {
        // Normalize papFile and outputDir to use consistent directory separators
        multiAssistExe = Path.GetFullPath(multiAssistExe);
        skeletonFile = Path.GetFullPath(skeletonFile);
        papFile = Path.GetFullPath(papFile);
        outputDir = Path.GetFullPath(outputDir);

        var papBase = Path.GetFileNameWithoutExtension(papFile);

        Console.WriteLine($"  Analyzing {papBase} for animation list...");

        var info = GetPapAnimInfo(multiAssistExe, skeletonFile, papFile);

        if (info.AnimCount <= 0)
        {
            Console.WriteLine($"    No animations discovered for {papBase}, skipping.");
            return;
        }

        Console.WriteLine($"    Found {info.AnimCount} animation(s).");

        for (int index = 0; index < info.AnimCount; index++)
        {
            string animName = (index < info.Names.Count && !string.IsNullOrWhiteSpace(info.Names[index]))
                ? info.Names[index]
                : $"idx{index:D3}";

            string fileName = BuildOutputFileName(papBase, animName, index, appendNames);
            string fbxPath = Path.Combine(outputDir, fileName);

            fbxPath = Path.GetFullPath(fbxPath);

            Console.WriteLine($"    [{index}] {animName} -> {fileName}");

            var exit = RunMultiAssistExtractFbx(
                multiAssistExe,
                skeletonFile,
                papFile,
                index,
                fbxPath
            );

            if (exit != 0)
                Console.WriteLine($"      WARN: MultiAssist exit code {exit} for index {index}");
        }
    }

    private sealed class MultiAssistResult
    {
        public int ExitCode { get; init; }
        public string StdOut { get; init; } = "";
        public string StdErr { get; init; } = "";
    }

    private static MultiAssistResult RunMultiAssist(string multiAssistExe, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = multiAssistExe,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(multiAssistExe) ?? Environment.CurrentDirectory
        };

        using var proc = Process.Start(psi)!;

        // Order here is fine for this volume of output
        string stdout = proc.StandardOutput.ReadToEnd();
        string stderr = proc.StandardError.ReadToEnd();

        proc.WaitForExit();
        return new MultiAssistResult
        {
            ExitCode = proc.ExitCode,
            StdOut = stdout,
            StdErr = stderr
        };
    }

    private static int RunMultiAssistExtractFbx(string multiAssistExe, string skeletonFile, string papFile, int index, string outputPath)
    {
        var args = $"extract -s \"{skeletonFile}\" -p \"{papFile}\" -i {index} -t fbx -o \"{outputPath}\"";

        var result = RunMultiAssist(multiAssistExe, args);

        if (result.ExitCode != 0)
        {
            Console.WriteLine($"      MultiAssist failed with exit code {result.ExitCode}");
            Console.WriteLine("      StdOut:");
            Console.WriteLine(result.StdOut);
            Console.WriteLine("      StdErr:");
            Console.WriteLine(result.StdErr);
        }

        return result.ExitCode;
    }

    private sealed class PapAnimInfo
    {
        public int AnimCount { get; init; }
        public List<string> Names { get; init; } = new();
    }

    /// <summary>
    /// Calls MultiAssist once with -t xml, captures console output,
    /// and parses anim_count and anim_infos names.
    /// </summary>
    private static PapAnimInfo GetPapAnimInfo(string multiAssistExe, string skeletonFile, string papFile)
    {
        // Dummy output path; we don't care about the XML file itself.
        var tmpOut = Path.GetTempFileName();

        var args =
            $"extract -s \"{skeletonFile}\" -p \"{papFile}\" -i 0 -t xml -o \"{tmpOut}\"";

        var result = RunMultiAssist(multiAssistExe, args);

        // We don't actually need the file, only the logs.
        try
        { File.Delete(tmpOut); }
        catch { }

        var combined = (result.StdOut ?? "") + "\n" + (result.StdErr ?? "");

        // 1) anim_count
        var countMatch = Regex.Match(combined, @"anim_count\s*:\s*(\d+)");
        int animCount = 0;
        if (countMatch.Success && int.TryParse(countMatch.Groups[1].Value, out var parsed))
            animCount = parsed;

        // 2) anim_infos names
        //    We search the *last* line that contains 'anim_infos' and pull every `'name': 'xxx'`.
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

    private static string BuildOutputFileName(string papBase, string animName, int index, List<string> appendNames)
    {
        // Clean up animName for filesystem (just in case)
        foreach (var c in Path.GetInvalidFileNameChars())
            animName = animName.Replace(c, '_');

        for (int i = 0; i < appendNames.Count; i++)
        {
            if (papBase.StartsWith(appendNames[i], StringComparison.OrdinalIgnoreCase))
            {
                return $"{papBase}_{animName}.fbx";
            }
        }

        return $"{animName}.fbx";
    }

    private static void RunBlenderAutomation(string blenderExe, string baseFbx, string animFolder, string folder)
    {
        string fileName = Path.GetFileNameWithoutExtension(baseFbx);

        string blendOut = Path.Combine(folder, $"{fileName}.blend");
        string finalFbx = Path.Combine(folder, $"{fileName}.fbx");
        // Assuming the script is located alongside the executable for now
        string blenderScript = Path.Combine(AppContext.BaseDirectory, "xivanim_blender.py");

        var exit = RunBlender(
            blenderExe,
            blenderScript,
            baseFbx,
            animFolder,
            blendOut,
            finalFbx
        );

        if (exit != 0)
        {
            Console.WriteLine($"Blender automation failed with exit code {exit}");
        }
    }

    private static int RunBlender(string blenderExe, string blenderScript, string baseFbx, string animFolder, string blendOut, string finalFbx)
    {
        // blender -b -P script.py -- base.fbx animFolder out.blend out.fbx
        string args = $"-b -P \"{blenderScript}\" -- \"{baseFbx}\" \"{animFolder}\" \"{blendOut}\" \"{finalFbx}\"";

        var psi = new ProcessStartInfo
        {
            FileName = blenderExe,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(blenderExe) ?? Environment.CurrentDirectory
        };

        using var proc = Process.Start(psi)!;
        string stdout = proc.StandardOutput.ReadToEnd();
        string stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit();

        Console.WriteLine("=== Blender stdout ===");
        Console.WriteLine(stdout);
        Console.WriteLine("=== Blender stderr ===");
        Console.WriteLine(stderr);

        return proc.ExitCode;
    }
}
