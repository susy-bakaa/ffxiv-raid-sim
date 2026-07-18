using System.Text.Json;
using Lumina;
using static dev.susy_baka.xivAnim.Core.Utility;

namespace dev.susy_baka.xivAnim.Core
{
    public class PipelineService
    {
        private readonly AppSettings settings;
        private readonly GameData gameData;

        public PipelineService(AppSettings settings)
        {
            this.settings = settings;

            if (string.IsNullOrEmpty(settings.ffxivGamePath))
            {
                Log.Error("FFXIV game path is not configured in settings.");
                throw new NullReferenceException("FFXIV game path is not configured in settings.");
            }

            gameData = new GameData(settings.ffxivGamePath);
            Log.Initialize(settings);
        }

        public void RunJob(string jobPath) => RunJob(jobPath, CancellationToken.None);

        public void RunJob(string jobPath, CancellationToken token)
        {
            ModelJob job = JobService.LoadJob(jobPath);
            if (job == null || string.IsNullOrEmpty(job.name))
            {
                Log.Info($"No valid job found from {jobPath}. Make sure your job has a name.");
                Log.Info("=== Skipped invalid job ===");
                return;
            }

            Log.Info($"=== Running job '{job.name}' ===");
            ProcessJob(job, token);
        }

        public void ClearJob(string jobPath)
        {
            ModelJob job = JobService.LoadJob(jobPath);
            if (job == null || string.IsNullOrEmpty(job.name))
            {
                Log.Info($"No valid job found from {jobPath}. Make sure your job has a name.");
                Log.Info("=== Skipped clearing invalid job ===");
                return;
            }
            
            ClearJobOutput(job);
        }

        private void ProcessJob(ModelJob job, CancellationToken token)
        {
            string workingDir = Path.GetFullPath(job.workingDirectory) ?? Path.Combine(AppContext.BaseDirectory, Utility.SanitizeFileName(job.name));
            string exportDir = Path.GetFullPath(job.exportDirectory) ?? workingDir;
            Directory.CreateDirectory(workingDir);
            Directory.CreateDirectory(exportDir);

            var skeletonDir = Path.Combine(workingDir, "Skeleton");
            var papDir = Path.Combine(workingDir, "Animations");

            Directory.CreateDirectory(skeletonDir);
            Directory.CreateDirectory(papDir);

            token.ThrowIfCancellationRequested();

            // 1. Extract raw animation pap files and skeleton via Lumina
            ExtractRawFiles(job, skeletonDir, papDir);

            var fbxDir = Path.Combine(workingDir, "Exported Animations");
            Directory.CreateDirectory(fbxDir);

            List<string> exportedAnimations;

            token.ThrowIfCancellationRequested();

            // 2. Export animation FBXs via MultiAssist if path is configured, otherwise halt
            if (!string.IsNullOrEmpty(settings.multiAssistPath))
            {
                ExportAnimationsWithMultiAssist(job, fbxDir, out exportedAnimations, token);
            }
            else
            {
                Log.Warning("MultiAssist path is not configured, cannot export animations. Please set the path in settings.");
                Log.Info($"=== Failed job '{job.name}' ===");
                return;
            }

            string loopJson = string.Empty;

            token.ThrowIfCancellationRequested();

            // 3. Build loop map JSON from MotionTimeline via Lumina
            BuildLoopJsonForJob(job, exportedAnimations, out loopJson);

            token.ThrowIfCancellationRequested();

            // 4. Run Blender pipeline if blender path is configured
            if (!string.IsNullOrEmpty(settings.blenderPath))
            {
                Log.Info("  Running Blender automation...");

                RunBlenderPipeline(job, fbxDir, exportDir, loopJson, token);
            }
            else
            {
                Log.Warning("  Skipping Blender automation because Blender path is not configured.");
            }

            Log.Info($"=== Finished job '{job.name}' ===");
        }

        /// <summary>
        /// Makes sure all raw files are extracted locally via Lumina and replaces game paths in the job with local paths.
        /// </summary>
        private void ExtractRawFiles(ModelJob job, string skeletonDir, string papDir)
        {
            var skeletonFile = ExtractRaw(job, job.skeletonGamePath, skeletonDir);

            Log.Info($"  Processing {Path.GetFileNameWithoutExtension(skeletonFile)}...");
            
            job.skeletonLocalPath = skeletonFile;

            job.papLocalPaths = new(job.papGamePaths.Count);
            job.papLocalPaths.Clear();

            for (int i = 0; i < job.papGamePaths.Count; i++)
            {
                var papFile = ExtractRaw(job, job.papGamePaths[i], papDir);

                Log.Info($"  Processing {Path.GetFileNameWithoutExtension(papFile)}...");

                job.papLocalPaths.Add(papFile);
            }
        }

        /// <summary>
        /// Using Lumina: read a game path and dump it to disk.
        /// </summary>
        private string ExtractRaw(ModelJob job, string gameRawFilePath, string outputDir)
        {
            // Determine output path and name
            string fileName = Path.GetFileName(gameRawFilePath);
            string extractionResult = Path.Combine(outputDir, fileName);

            // Use the job's appendFileNamesForPaths regex list to determine if a subfolder should be created for this raw file.
            // This ensures that files with the same name but different paths don't overwrite each other
            OutputPathResult pathResult = Utility.BuildOutputFileName(gameRawFilePath, fileName, job.appendFileNamesForPaths);

            // If a subfolder was captured by the regex, combine it into the destination directory
            if (!string.IsNullOrEmpty(pathResult.SubFolder))
            {
                outputDir = Path.Combine(outputDir, pathResult.SubFolder);
                extractionResult = Path.Combine(outputDir, fileName); // Only use the subfolder for the output directory, keep the original file name for raw files
            }

            // Ensure output directory exists
            Directory.CreateDirectory(outputDir);

            // Skip further work if file already exists locally, if user wants to update they can delete it manually
            if (File.Exists(extractionResult))
            {
                Log.Info($"  Skipping extraction of existing file '{fileName}' already at {extractionResult}.");
                return extractionResult;
            }

            // Try to get the file via Lumina
            var file = gameData.GetFile(gameRawFilePath);
            var data = file?.Data;
            if (data != null)
            {
                Log.Info($"  Extracting {gameRawFilePath} via Lumina...");
                File.WriteAllBytes(extractionResult, data);
            }
            else // Fallback to trying to find local file instead at the given path and copy it over
            {
                Log.Warning($"  Could not extract '{gameRawFilePath}' via Lumina!");
                Log.Info("   Attempting to find local file instead...");
                
                if (!File.Exists(gameRawFilePath))
                {
                    Log.Error($"  Local raw file not found: {gameRawFilePath}");
                    throw new FileNotFoundException($"Local raw file not found: {gameRawFilePath}");
                }

                if (!File.Exists(extractionResult))
                    File.Copy(gameRawFilePath, extractionResult);
            }

            return extractionResult;
        }

        private void ExportAnimationsWithMultiAssist(ModelJob job, string fbxDir, out List<string> exportedAnimations, CancellationToken token)
        {
            // Track all exported animations for loop map generation
            exportedAnimations = new List<string>();

            for (int i = 0; i < job.papLocalPaths.Count; i++)
            {
                string papLocalPath = job.papLocalPaths[i];
                string papGamePath = job.papGamePaths[i];

                var papBase = Path.GetFileNameWithoutExtension(papLocalPath);

                Log.Info($"  Analyzing {papBase} for animation list...");

                var info = Utility.GetPapAnimInfo(settings.multiAssistPath, job.skeletonLocalPath, papLocalPath, token);

                if (info.AnimCount <= 0)
                {
                    Log.Info($"    No animations found for {papBase}, skipping.");
                    return;
                }

                Log.Info($"    Found {info.AnimCount} animation(s).");

                for (int index = 0; index < info.AnimCount; index++)
                {
                    string animName = (index < info.Names.Count && !string.IsNullOrWhiteSpace(info.Names[index]))
                        ? info.Names[index]
                        : $"idx{index:D3}";

                    OutputPathResult pathResult = Utility.BuildOutputFileName(papGamePath, $"{animName}.fbx", job.appendFileNamesForPaths);

                    // If a subfolder was captured by the regex, combine it into the destination directory
                    string finalFbxDir = fbxDir;
                    if (!string.IsNullOrEmpty(pathResult.SubFolder))
                    {
                        finalFbxDir = Path.Combine(fbxDir, pathResult.SubFolder);
                        Directory.CreateDirectory(finalFbxDir); // Ensure the subfolder exists or MultiAssist will fail to write the file
                    }
                    
                    string fbxPath = Path.Combine(finalFbxDir, pathResult.FileName);
                    fbxPath = Path.GetFullPath(fbxPath);

                    exportedAnimations.Add(animName);

                    Log.Info($"    [{index}] {animName} -> {(string.IsNullOrEmpty(pathResult.SubFolder) ? pathResult.FileName : $"{pathResult.SubFolder}/{pathResult.FileName}")}");

                    var args = new List<string>
                    {
                        "extract",
                        "-s", job.skeletonLocalPath,
                        "-p", papLocalPath,
                        "-i", index.ToString(),
                        "-t", "fbx",
                        "-o", fbxPath
                    };

                    var exit = Utility.RunProcess(settings.multiAssistPath, args, token);

                    if (exit.ExitCode != 0)
                    {
                        Log.Info("      === MultiAssist stdout ===");
                        Log.Info(exit.StdOut);
                        Log.Info("      === End stdout ===");
                        Log.Error("      === MultiAssist stderr ===");
                        Log.Error(exit.StdErr);
                        Log.Error("      === End stderr ===");
                        Log.Warning($"      MultiAssist failed with exit code {exit.ExitCode} for index {index}!");
                    }
                }
            }
        }

        private void BuildLoopJsonForJob(ModelJob job, List<string> animationNames, out string loopJson)
        {
            // Gather distinct motion names for loop map
            IEnumerable<string> motionNamesForBoss = animationNames.Distinct(StringComparer.OrdinalIgnoreCase);

            // Build loop map from game data
            var loopMap = Utility.BuildLoopMapForMotions(gameData, motionNamesForBoss);

            // Serialise to JSON ready to pass to Blender
            loopJson = JsonSerializer.Serialize(loopMap);
        }

        private void RunBlenderPipeline(ModelJob job, string animFolder, string exportFolder, string loopJson, CancellationToken token)
        {
            string modelPathsJson = JsonSerializer.Serialize(job.modelPaths);
            string blendOut = Path.Combine(exportFolder, $"{Utility.SanitizeFileName(job.name)}.blend");
            string finalFbx = Path.Combine(exportFolder, $"{Utility.SanitizeFileName(job.name)}.fbx");

            // blender -b --python-expr "import raidsim_tools.cli as c; c.main()" -- modelsJson animFolder blendOut fbxOut loopJson
            List<string> args = new List<string>()
            {
                "-b",
                "--python-expr", "import raidsim_tools.cli as c; c.main()",
                "--",
                modelPathsJson,
                animFolder,
                blendOut,
                finalFbx,
                loopJson
            };

            var exit = Utility.RunProcess(settings.blenderPath, args, token);

            Log.Info("=== Blender stdout ===");
            Log.Info(exit.StdOut);
            Log.Info("=== End stdout ===");

            if (exit.ExitCode != 0)
            {
                Log.Error("=== Blender stderr ===");
                Log.Error(exit.StdErr);
                Log.Error("=== End stderr ===");
                Log.Warning($"  Blender automation failed and exited with code {exit.ExitCode}!");
            }
            else
            {
                Log.Info($"  Blender automation completed! Check Blender logs for more information.");
            }
        }

        public void ClearJobOutput(ModelJob job)
        {
            Log.Info($"=== Clearing output for job '{job.name}' ===");

            bool success = true;

            // Determine working and export directories
            string workingDir = Path.GetFullPath(job.workingDirectory) ?? Path.Combine(AppContext.BaseDirectory, Utility.SanitizeFileName(job.name));
            string exportDir = Path.GetFullPath(job.exportDirectory) ?? workingDir;
            Directory.CreateDirectory(workingDir);
            Directory.CreateDirectory(exportDir);

            // Handle output directories
            List<string> outputDirs = new List<string>(3);

            outputDirs.Add(Path.Combine(workingDir, "Skeleton"));
            outputDirs.Add(Path.Combine(workingDir, "Animations"));
            outputDirs.Add(Path.Combine(workingDir, "Exported Animations"));

            foreach (string dirPath in outputDirs)
            {
                if (Directory.Exists(dirPath))
                {
                    try
                    {
                        Directory.Delete(dirPath, recursive: true);
                        Log.Info($"Deleted {dirPath}");
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        Log.Error($"Failed to delete {dirPath}: {ex.Message}");
                    }
                }
            }

            // Handle single files
            List<string> exportedFiles = new List<string>(3);

            exportedFiles.Add(Path.Combine(exportDir, $"{Utility.SanitizeFileName(job.name)}.blend"));
            exportedFiles.Add(Path.Combine(exportDir, $"{Utility.SanitizeFileName(job.name)}.fbx"));
            exportedFiles.Add(Path.Combine(exportDir, $"{Utility.SanitizeFileName(job.name)}.events.xml"));

            foreach (string filePath in exportedFiles)
            {
                if (File.Exists(filePath))
                {
                    try 
                    {
                        File.Delete(filePath);
                        Log.Info($"Deleted {filePath}");
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        Log.Error($"Failed to delete {filePath}: {ex.Message}");
                    }
                }
            }

            if (success)
                Log.Info($"=== Completed clearing output for job '{job.name}' ===");
            else
                Log.Warning($"=== Completed clearing output for job '{job.name}' with errors ===");
        }
    }
}
