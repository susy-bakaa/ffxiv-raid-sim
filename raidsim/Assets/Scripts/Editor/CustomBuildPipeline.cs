// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.Editor
{
    public static class CustomBuildPipeline
    {
        private static readonly string ExecutableName = "raidsim";
        private static readonly string BuildRoot = Path.GetFullPath("builds");
        private static readonly string ChecksumFile = BuildRoot + "/checksums.sha256";
        private static readonly string BundleRoot = BuildRoot + "/bundles";
        private static readonly string[] DoNotShipBundles = new[]
        {
            "archive",
            "editor_only",
            "temp",
            "test",
            "StreamingAssets",
            "StandaloneWindows64.", // We need to add '.' to these or it skips everything as the folder has the same name as the bundle file, 
            "StandaloneLinux64."    // which means this filter won't unfortunately work without some kind of file extension. Should probably be fixed in the future.
        };
        public static readonly string[] DoNotShipFolders = new[]
        {
            "raidsim_BurstDebugInformation_DoNotShip",
            "bgm",
            "AssetBundles",
            "bundles"
        };
        public static readonly string[] DoNotShipFiles = new[]
        {
            "config.ini"
        };
        public static readonly string[] PreserveFiles = new[]
        {
            "config.ini",
            "debug.bat",
            "fps.bat",
            "debug.sh",
            "fps.sh",
            "updater",
            "updater.exe",
            "updater.x86_64",
            "log.txt",
            "output.log",
            "THIRD-PARTY-NOTICES.txt"
        };
        public static readonly string[] PreserveFolders = new[]
        {
            "bgm",
            "AssetBundles",
            "bundles",
            "temp"
        };
        public static readonly string[] PreserveLocalBundlesOnWebGL = new[]
        {
            "common",
            "hotbars"
        };

        public static bool BuildWindows = true;
        public static bool BuildLinux = true;
        public static bool BuildWebGL = true;
        public static bool ShouldPackageBuilds = true;
        public static bool ShouldRebuildProgram = true;
        public static bool ShouldRebuildAssetBundles = true;
        public static bool useCustomExtension = true;
        public static string ManualUnityVersion = "1.0.0";
        public static int ManualVersionNumber = 0;

        private static readonly (BuildTarget target, string outputFolder, string zipName)[] BuildConfigs = new[]
        {
            (BuildTarget.StandaloneWindows64, "winbuild", "win64"),
            (BuildTarget.StandaloneLinux64,    "linuxbuild", "linux64"),
            (BuildTarget.WebGL,                "webbuild", "webgl")
        };

        public static void RunFullBuildPipeline()
        {
            var selectedConfigs = BuildConfigs.Where(config =>
                (config.target == BuildTarget.StandaloneWindows64 && BuildWindows) ||
                (config.target == BuildTarget.StandaloneLinux64 && BuildLinux) ||
                (config.target == BuildTarget.WebGL && BuildWebGL)).ToArray();

            if (selectedConfigs.Length == 0)
            {
                if (ShouldPackageBuilds)
                    PackageBuilds();
                else
                    Debug.Log("No platforms selected and packaging is disabled. Nothing to do.");
                return;
            }

            string logFolder = Path.Combine(BuildRoot, "logs", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + "_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(logFolder);
            Debug.Log($"Build logs will be saved to '{logFolder}'.");

            try
            {
                // Ensure we begin from windows build target
                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                    throw new BuildFailedException("Could not switch to the Windows build target.");

                // Set the version and bundle version
                PlayerSettings.bundleVersion = ManualUnityVersion;
                GlobalVariables.versionNumber = ManualVersionNumber;

                // Execute each build configuration
                foreach (var (target, folder, zip) in selectedConfigs)
                {
                    string logPath = Path.Combine(logFolder, $"{target}.log");
                    using TextWriter buildLog = TextWriter.Synchronized(new StreamWriter(logPath) { AutoFlush = true });
                    object logLock = new object();
                    bool captureLogs = true;
                    Application.LogCallback logCallback = (message, stackTrace, type) =>
                    {
                        lock (logLock)
                        {
                            if (captureLogs)
                                buildLog.WriteLine($"[{DateTime.Now:O}] [{type}] {message}\n{stackTrace}");
                        }
                    };

                    Application.logMessageReceivedThreaded += logCallback;
                    try
                    {
                        Debug.Log($"Starting build for {target}. Rebuild program: {ShouldRebuildProgram}, rebuild bundles: {ShouldRebuildAssetBundles}.");
                        RunBuildForTarget(target, folder, zip, buildLog);
                        Debug.Log($"Build completed for {target}.");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Build failed for {target}: {ex}. Pipeline stopped; packaging skipped. Log: '{logPath}'.");
                        throw;
                    }
                    finally
                    {
                        Application.logMessageReceivedThreaded -= logCallback;
                        lock (logLock)
                        {
                            captureLogs = false;
                        }
                    }
                }
            }
            finally
            {
                // Reset things we changed for some builds, even when a build fails
                QualitySettings.globalTextureMipmapLimit = 0; // 4096
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, $"dev.susybaka.{ExecutableName}.windows");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }

            if (ShouldPackageBuilds)
                PackageBuilds();
        }

        private static void RunBuildForTarget(BuildTarget target, string folderName, string zipName, TextWriter buildLog)
        {
            string outputDir = Path.Combine(BuildRoot, folderName);

            if (ShouldRebuildProgram)
            {
                CleanBuildDirectory(outputDir);
                Directory.CreateDirectory(outputDir);
            }

            if (target == BuildTarget.WebGL)
            {
                QualitySettings.globalTextureMipmapLimit = 1; // 2048
            }
            else
            {
                QualitySettings.globalTextureMipmapLimit = 0; // 4096
            }

            string locationPathName = string.Empty;

            switch (target)
            {
                case BuildTarget.StandaloneWindows64:
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, $"dev.susybaka.{ExecutableName}.windows");
                    locationPathName = Path.Combine(outputDir, $"{ExecutableName}.exe");
                    break;
                case BuildTarget.StandaloneLinux64:
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, $"dev.susybaka.{ExecutableName}.linux");
                    locationPathName = Path.Combine(outputDir, $"{ExecutableName}.x86_64");
                    break;
                case BuildTarget.WebGL:
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.WebGL, $"dev.susybaka.{ExecutableName}.webgl");
                    locationPathName = Path.Combine(outputDir, "_temp"); // WebGL builds require a whole folder for output, so we build to a temp folder and then move the contents up to the final output directory after the build completes
                    break;
                default:
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, $"dev.susybaka.{ExecutableName}");
                    locationPathName = Path.Combine(outputDir, ExecutableName);
                    throw new BuildFailedException($"Unsupported build target: {target}");
            }

            BuildPlayerOptions buildOptions = new()
            {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = locationPathName,
                target = target,
                options = BuildOptions.CleanBuildCache
            };

            if (ShouldRebuildProgram)
            {
                if (target == BuildTarget.WebGL)
                {
                    if (Directory.Exists(locationPathName))
                        Directory.Delete(locationPathName, true);
                    Directory.CreateDirectory(locationPathName);
                }

                BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
                if (report == null)
                    throw new BuildFailedException($"Player build returned no BuildReport for {target}.");

                buildLog.WriteLine($"Player build result: {report.summary.result}, errors: {report.summary.totalErrors}, warnings: {report.summary.totalWarnings}, duration: {report.summary.totalTime}");
                foreach (BuildStep step in report.steps)
                {
                    buildLog.WriteLine($"Build step: {step.name} ({step.duration})");
                    foreach (BuildStepMessage message in step.messages)
                    {
                        buildLog.WriteLine($"[{message.type}] {message.content}");
                    }
                }

                if (report.summary.result != BuildResult.Succeeded)
                    throw new BuildFailedException($"Player build did not succeed for {target}: {report.summary.result} ({report.summary.totalErrors} errors).");

                // On Linux, rename the executable to remove the .x86_64 suffix for consistency with usual Linux conventions
                if (target == BuildTarget.StandaloneLinux64)
                {
                    string linuxExecutable = Path.Combine(outputDir, $"{ExecutableName}.x86_64");
                    if (File.Exists(linuxExecutable))
                    {
                        try
                        {
                            File.Move(linuxExecutable, Path.Combine(outputDir, ExecutableName));
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Failed to rename Linux executable: {ex.Message}");
                            throw;
                        }
                    }
                }

                // On WebGL, we need to move the contents of the temp folder up to the final output directory and then delete the temp folder, as WebGL builds require a whole folder for it's output
                if (target == BuildTarget.WebGL)
                {
                    MoveWebGLBuildIntoPlace(locationPathName, outputDir);
                    if (Directory.Exists(locationPathName))
                        Directory.Delete(locationPathName, true);
                }
            }

            string bundleTargetFolder = Path.Combine(BundleRoot, target.ToString());
            
            if (ShouldRebuildAssetBundles)
            {
                BuildAssetBundles(bundleTargetFolder, target);
            }
            else
            {
                // If we're not rebuilding bundles, we need to make sure they exist before we try to copy them into the build output
                // If they don't exist, we should build them to ensure the build has the necessary bundles to run properly,
                // otherwise we might end up with a broken build that doesn't have any bundles in it
                if (!Directory.Exists(bundleTargetFolder) || Directory.GetFiles(bundleTargetFolder).Length < 1)
                {
                    Debug.LogWarning($"Asset bundles do not exist for '{bundleTargetFolder}'. Building asset bundles for {target.ToString()}.");
                    BuildAssetBundles(bundleTargetFolder, target);
                }
            }

            if (useCustomExtension)
                ApplyBundleExtension(bundleTargetFolder);

            if (target != BuildTarget.WebGL)
            {
                string destBundleFolder = Path.Combine(outputDir, $"{ExecutableName}_Data", "StreamingAssets");

                if (Directory.Exists(destBundleFolder))
                    Directory.Delete(destBundleFolder, true);

                Directory.CreateDirectory(destBundleFolder);

                // Restore extra StreamingAssets from the project without its test bundles
                CopyProjectStreamingAssetsInto(destBundleFolder);

                Debug.Log($"Copying {Directory.GetFiles(bundleTargetFolder).Length} AssetBundles from '{bundleTargetFolder}' to '{destBundleFolder}'");

                foreach (string file in Directory.GetFiles(bundleTargetFolder))
                {
                    bool skip = false;

                    for (int i = 0; i < DoNotShipBundles.Length; i++)
                    {
                        if (file.Contains(DoNotShipBundles[i]))
                        {
                            Debug.Log($"Skipping AssetBundle '{file}' as it is in the DoNotShipBundles list.");
                            skip = true;
                            break;
                        }
                    }

                    if (skip)
                        continue;

                    string destFile = Path.Combine(destBundleFolder, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }
            }
            else
            {
                string destBundleFolder = Path.Combine(outputDir, "StreamingAssets");

                if (Directory.Exists(destBundleFolder))
                    Directory.Delete(destBundleFolder, true);

                if (PreserveLocalBundlesOnWebGL != null && PreserveLocalBundlesOnWebGL.Length > 0)
                {
                    Directory.CreateDirectory(destBundleFolder);

                    // Restore extra StreamingAssets from the project without its test bundles
                    CopyProjectStreamingAssetsInto(destBundleFolder);

                    foreach (string file in Directory.GetFiles(bundleTargetFolder))
                    {
                        bool preserve = false;
                        for (int i = 0; i < PreserveLocalBundlesOnWebGL.Length; i++)
                        {
                            if (file.Contains(PreserveLocalBundlesOnWebGL[i]))
                            {
                                preserve = true;
                                break;
                            }
                        }
                        if (preserve)
                        {
                            string destFile = Path.Combine(destBundleFolder, Path.GetFileName(file));
                            File.Copy(file, destFile, true);
                            Debug.Log($"Preserving local AssetBundle '{file}' for WebGL build.");
                        }
                    }
                }
            }
        }

        private static void CleanBuildDirectory(string outputDir)
        {
            if (!Directory.Exists(outputDir))
                return;

            // Delete files except preserved ones
            foreach (string file in Directory.GetFiles(outputDir))
            {
                string fileName = Path.GetFileName(file);
                if (!PreserveFiles.Any(preserved => string.Equals(preserved, fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    Debug.Log($"Deleting file '{file}' from build directory as it is not in the PreserveFiles list.");
                    File.Delete(file);
                }
            }

            // Delete folders except preserved ones
            foreach (string dir in Directory.GetDirectories(outputDir))
            {
                string dirName = Path.GetFileName(dir);
                if (!PreserveFolders.Any(preserved => string.Equals(preserved, dirName, StringComparison.OrdinalIgnoreCase)))
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        private static void BuildAssetBundles(string outputPath, BuildTarget target)
        {
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            try
            {
                BuildAssetBundleOptions options = BuildAssetBundleOptions.None;

                if (target == BuildTarget.WebGL)
                {
                    options = BuildAssetBundleOptions.ChunkBasedCompression;
                }

                if (useCustomExtension)
                    Debug.Log($"Building for target: {target.ToString()} with the following {options} AssetBundle extension: {GlobalVariables.assetBundleExtension}");
                else
                    Debug.Log($"Building for target: {target.ToString()} with the following {options} AssetBundle extension: none");

                AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputPath, options, target);
                if (manifest == null)
                    throw new BuildFailedException($"Asset bundle build returned no manifest for {target}.");

                if (useCustomExtension)
                    ApplyBundleExtension(outputPath);

                Debug.Log($"Asset bundle build completed for {target.ToString()}");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Asset bundle build failed: " + e);
                throw;
            }
        }

        private static void ApplyBundleExtension(string outputPath)
        {
            string extension = GlobalVariables.assetBundleExtension;
            foreach (string filePath in Directory.GetFiles(outputPath, "*", SearchOption.AllDirectories))
            {
                if (filePath.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ||
                    filePath.EndsWith(".manifest", StringComparison.OrdinalIgnoreCase) ||
                    filePath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    continue;

                string newPath = filePath + extension;
                // Replace the previous build before moving the new bundle, regardless of directory enumeration order.
                if (File.Exists(newPath))
                    File.Delete(newPath);
                File.Move(filePath, newPath);

                foreach (string suffix in new[] { ".manifest", ".meta" })
                {
                    if (File.Exists(newPath + suffix))
                        File.Delete(newPath + suffix);
                    if (File.Exists(filePath + suffix))
                        File.Move(filePath + suffix, newPath + suffix);
                }
            }
        }

        private static bool HasBuildOutput(BuildTarget target, string folder)
        {
            string outputDir = Path.Combine(BuildRoot, folder);
            if (target == BuildTarget.WebGL)
            {
                string buildDir = Path.Combine(outputDir, "Build");
                return File.Exists(Path.Combine(outputDir, "index.html")) &&
                    Directory.Exists(buildDir) && Directory.GetFiles(buildDir).Length > 0;
            }

            string executable = target == BuildTarget.StandaloneWindows64 ? ExecutableName + ".exe" : ExecutableName;
            return File.Exists(Path.Combine(outputDir, executable)) &&
                Directory.Exists(Path.Combine(outputDir, $"{ExecutableName}_Data"));
        }

        private static void PackageBuilds()
        {
            // Validate every platform before touching any existing archives or checksums.
            var missingBuilds = BuildConfigs.Where(config => !HasBuildOutput(config.target, config.outputFolder))
                .Select(config => config.target.ToString()).ToArray();
            if (missingBuilds.Length > 0)
                throw new BuildFailedException($"Packaging requires existing builds for all platforms. Missing or incomplete: {string.Join(", ", missingBuilds)}.");

            if (File.Exists(ChecksumFile))
                File.Delete(ChecksumFile);

            foreach (var (_, folder, zip) in BuildConfigs)
            {
                string sourceDir = Path.Combine(BuildRoot, folder);
                string zipPath = Path.Combine(BuildRoot, $"{ExecutableName}_v.{ManualUnityVersion}_{zip}.zip");

                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Create))
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                {
                    AddDirectoryToZipFiltered(sourceDir, sourceDir, archive);
                }

                string hash = ComputeSHA256(zipPath);
                File.AppendAllText(ChecksumFile, $"{hash}  {Path.GetFileName(zipPath)}\n");
            }

            Debug.Log("All builds packaged and checksummed.");
        }

        private static string ComputeSHA256(string filePath)
        {
            using FileStream stream = File.OpenRead(filePath);
            using SHA256 sha = SHA256.Create();
            byte[] hashBytes = sha.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        private static void AddDirectoryToZipFiltered(string rootDir, string currentDir, ZipArchive archive)
        {
            foreach (string filePath in Directory.GetFiles(currentDir))
            {
                string relativePath = Path.GetRelativePath(rootDir, filePath).Replace("\\", "/");

                string fileName = Path.GetFileName(filePath);
                if (DoNotShipFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                    continue;

                archive.CreateEntryFromFile(filePath, relativePath, System.IO.Compression.CompressionLevel.Optimal);
            }

            foreach (string subDir in Directory.GetDirectories(currentDir))
            {
                string dirName = Path.GetFileName(subDir);
                if (DoNotShipFolders.Contains(dirName, StringComparer.OrdinalIgnoreCase))
                    continue;

                AddDirectoryToZipFiltered(rootDir, subDir, archive);
            }
        }

        private static void MoveWebGLBuildIntoPlace(string tempDir, string finalDir)
        {
            foreach (var src in Directory.GetFileSystemEntries(tempDir))
            {
                var name = Path.GetFileName(src);
                var dest = Path.Combine(finalDir, name);

                if (PreserveFiles.Contains(name, StringComparer.OrdinalIgnoreCase) ||
                    PreserveFolders.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    Debug.LogWarning($"WebGL temp produced '{name}' which conflicts with Preserve lists. Skipping move.");
                    continue;
                }

                // Remove old Unity output in finalDir so move works
                if (File.Exists(dest))
                    File.Delete(dest);
                else if (Directory.Exists(dest))
                    Directory.Delete(dest, true);

                // Move into place
                if (Directory.Exists(src))
                    Directory.Move(src, dest);
                else
                    File.Move(src, dest);
            }
        }

        private static bool IsAssetBundleFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            using FileStream stream = File.OpenRead(filePath);
            byte[] header = new byte[8];
            int count = stream.Read(header, 0, header.Length);
            string signature = System.Text.Encoding.ASCII.GetString(header, 0, count);
            return signature.StartsWith("UnityFS\0", StringComparison.Ordinal) ||
                signature.StartsWith("UnityRaw", StringComparison.Ordinal) ||
                signature.StartsWith("UnityWeb", StringComparison.Ordinal);
        }

        private static void CopyProjectStreamingAssetsInto(string destStreamingAssetsDir)
        {
            string src = Path.Combine(Application.dataPath, "StreamingAssets");
            if (!Directory.Exists(src))
                return;

            HashSet<string> bundleNames = new HashSet<string>(AssetDatabase.GetAllAssetBundleNames(), StringComparer.OrdinalIgnoreCase);
            bundleNames.Add("StreamingAssets");
            foreach (var (target, _, _) in BuildConfigs)
                bundleNames.Add(target.ToString());

            CopyDirectoryRecursive(src, destStreamingAssetsDir);

            void CopyDirectoryRecursive(string srcDir, string dstDir)
            {
                Directory.CreateDirectory(dstDir);

                foreach (var file in Directory.GetFiles(srcDir))
                {
                    // Don't ship Unity meta files
                    if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string relativePath = Path.GetRelativePath(src, file).Replace("\\", "/");
                    string bundlePath = file;
                    if (relativePath.EndsWith(".manifest", StringComparison.OrdinalIgnoreCase))
                    {
                        relativePath = relativePath.Substring(0, relativePath.Length - ".manifest".Length);
                        bundlePath = file.Substring(0, file.Length - ".manifest".Length);
                    }

                    if (relativePath.EndsWith(GlobalVariables.assetBundleExtension, StringComparison.OrdinalIgnoreCase) ||
                        bundleNames.Contains(relativePath) || IsAssetBundleFile(bundlePath))
                        continue;

                    var dst = Path.Combine(dstDir, Path.GetFileName(file));
                    File.Copy(file, dst, true);
                }

                foreach (var dir in Directory.GetDirectories(srcDir))
                {
                    var dstSub = Path.Combine(dstDir, Path.GetFileName(dir));
                    CopyDirectoryRecursive(dir, dstSub);
                }
            }
        }
    }
}
