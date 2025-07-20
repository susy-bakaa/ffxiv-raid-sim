using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEditor;
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
            "StreamingAssets"
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
            "config.ini",
            "debug.bat",
            "fps.bat",
            "debug.sh",
            "fps.sh"
        };
        public static readonly string[] PreserveFiles = new[]
        {
            "config.ini",
            "debug.bat",
            "fps.bat",
            "debug.sh",
            "fps.sh",
            "updater.dll",
            "updater.exe",
            "updater.x86_64",
            "updater.runtimeconfig.json",
            "log.txt",
            "output.log"
        };
        public static readonly string[] PreserveFolders = new[]
        {
            "bgm",
            "AssetBundles",
            "bundles",
            "temp"
        };

        public static bool ShouldRebuildAssetBundles = true;
        public static bool useCustomExtension = true;
        public static string ManualUnityVersion = "1.0.0";
        public static int ManualVersionNumber = 0;

        private static readonly (BuildTarget target, string outputFolder, string zipName)[] BuildConfigs = new[]
        {
            (BuildTarget.StandaloneWindows64, "winbuild1", "win64"),
            (BuildTarget.StandaloneLinux64,    "linuxbuild1", "linux64"),
            (BuildTarget.WebGL,                "webbuild1", "webgl")
        };

        public static void RunFullBuildPipeline()
        {
            PlayerSettings.bundleVersion = ManualUnityVersion;
            GlobalVariables.versionNumber = ManualVersionNumber;

            foreach (var (target, folder, zip) in BuildConfigs)
            {
                try
                {
                    RunBuildForTarget(target, folder, zip);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Build failed for {target.ToString()}: {ex.Message}");
                }
            }

            PackageBuilds();
        }

        private static void RunBuildForTarget(BuildTarget target, string folderName, string zipName)
        {
            string outputDir = Path.Combine(BuildRoot, folderName);

            CleanBuildDirectory(outputDir);
            Directory.CreateDirectory(outputDir);

            if (ShouldRebuildAssetBundles)
            {
                string bundleTargetFolder = Path.Combine(BundleRoot, target.ToString());
                BuildAssetBundles(bundleTargetFolder, target);

                if (target != BuildTarget.WebGL)
                {
                    string destBundleFolder = Path.Combine(outputDir, $"{ExecutableName}_Data", "StreamingAssets");

                    if (Directory.Exists(destBundleFolder))
                        Directory.Delete(destBundleFolder, true);

                    Directory.CreateDirectory(destBundleFolder);

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
                    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, $"dev.susybaka.{ExecutableName}.windows");
                    locationPathName = Path.Combine(outputDir, $"{ExecutableName}.exe");
                    break;
                case BuildTarget.StandaloneLinux64:
                    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, $"dev.susybaka.{ExecutableName}.linux");
                    locationPathName = Path.Combine(outputDir, $"{ExecutableName}.x86_64");
                    break;
                case BuildTarget.WebGL:
                    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.WebGL, $"dev.susybaka.{ExecutableName}.webgl");
                    locationPathName = outputDir;
                    break;
                default:
                    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, $"dev.susybaka.{ExecutableName}");
                    locationPathName = Path.Combine(outputDir, ExecutableName);
                    Debug.LogError($"Unsupported build target: {target}");
                    return;
            }

            BuildPlayerOptions buildOptions = new()
                {
                    scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                    locationPathName = locationPathName,
                    target = target,
                    options = BuildOptions.CleanBuildCache
                };

            BuildPipeline.BuildPlayer(buildOptions);
        }

        private static void CleanBuildDirectory(string outputDir)
        {
            if (!Directory.Exists(outputDir))
                return;

            // Delete files except preserved ones
            foreach (string file in Directory.GetFiles(outputDir))
            {
                string fileName = Path.GetFileName(file);
                if (!PreserveFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                {
                    File.Delete(file);
                }
            }

            // Delete folders except preserved ones
            foreach (string dir in Directory.GetDirectories(outputDir))
            {
                string dirName = Path.GetFileName(dir);
                if (!PreserveFolders.Contains(dirName, StringComparer.OrdinalIgnoreCase))
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

                Debug.Log($"Building for target: {target.ToString()} with the following {options}");

                BuildPipeline.BuildAssetBundles(outputPath, options, target);

                if (useCustomExtension)
                {
                    string extension = GlobalVariables.assetBundleExtension;
                    string[] files = Directory.GetFiles(outputPath);

                    foreach (string filePath in files)
                    {
                        if (filePath.EndsWith(extension) || filePath.EndsWith($"{extension}.manifest") || filePath.EndsWith($"{extension}.meta"))
                        {
                            File.Delete(filePath);
                            continue;
                        }

                        if (filePath.EndsWith(".manifest") || filePath.EndsWith(".meta"))
                            continue;

                        string newPath = filePath + extension;

                        if (!File.Exists(newPath))
                        {
                            File.Move(filePath, newPath);
                            if (File.Exists(filePath + ".manifest"))
                                File.Move(filePath + ".manifest", newPath + ".manifest");
                            if (File.Exists(filePath + ".meta"))
                                File.Move(filePath + ".meta", newPath + ".meta");
                        }
                    }
                }

                Debug.Log($"Asset bundle build completed for {target.ToString()}");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Asset bundle build failed: " + e);
            }
        }

        private static void PackageBuilds()
        {
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
    }
    
    public class CustomBuildPipelineWindow : EditorWindow
    {
        private bool rebuildBundles = true;
        private bool useCustomExtension = true;
        private string unityVersion = "1.0.0";
        private int versionNumber = 0;

        [MenuItem("Tools/Custom Build Pipeline Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<CustomBuildPipelineWindow>("Build Pipeline", true);
            window.titleContent = new GUIContent("Build Pipeline", EditorGUIUtility.IconContent("BuildSettings.Editor").image);
        }

        private void OnEnable()
        {
            unityVersion = PlayerSettings.bundleVersion;
            versionNumber = GlobalVariables.versionNumber;
        }

        private void OnGUI()
        {
            GUILayout.Label("Build Pipeline Settings", EditorStyles.boldLabel);

            unityVersion = EditorGUILayout.TextField("Unity Version", unityVersion);
            versionNumber = EditorGUILayout.IntField("Global Version Number", versionNumber);
            rebuildBundles = EditorGUILayout.Toggle("Rebuild Asset Bundles", rebuildBundles);
            useCustomExtension = EditorGUILayout.Toggle("Use Bundle File Extension", useCustomExtension);

            if (useCustomExtension)
                GUILayout.Label($"Current Extension: {GlobalVariables.assetBundleExtension}", EditorStyles.label);
            else
                GUILayout.Space(17); // Just to keep the layout consistent

            if (GUILayout.Button("Run Full Build Pipeline"))
            {
                CustomBuildPipeline.ShouldRebuildAssetBundles = rebuildBundles;
                CustomBuildPipeline.useCustomExtension = useCustomExtension;
                CustomBuildPipeline.ManualUnityVersion = unityVersion;
                CustomBuildPipeline.ManualVersionNumber = versionNumber;
                CustomBuildPipeline.RunFullBuildPipeline();
            }
        }
    }
}