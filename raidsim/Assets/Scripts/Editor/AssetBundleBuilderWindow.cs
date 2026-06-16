// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.Shared.Editor 
{
    public class AssetBundleBuilderWindow : EditorWindow
    {
        private string sourceFolder = "Assets";
        private string outputFolder = "Assets/StreamingAssets";
        private int selectedTargetIndex = 0;
        private bool useCustomExtension = true;

        private static readonly BuildTarget[] buildTargets = new BuildTarget[]
        {
            BuildTarget.StandaloneWindows64,
            BuildTarget.StandaloneLinux64,
            BuildTarget.WebGL
        };

        [MenuItem("Tools/AssetBundle Builder")]
        public static void ShowWindow()
        {
            AssetBundleBuilderWindow window = GetWindow<AssetBundleBuilderWindow>("AssetBundle Builder");

            // Set the icon for the window using Unity's default scene icon
            GUIContent titleContent = new GUIContent("AssetBundle Builder", EditorGUIUtility.IconContent("ModelImporter Icon").image);
            window.titleContent = titleContent;
        }

        private void OnGUI()
        {
            GUILayout.Label("AssetBundle Build Settings", EditorStyles.boldLabel);

            sourceFolder = EditorGUILayout.TextField("Source Folder:", sourceFolder);
            outputFolder = EditorGUILayout.TextField("Output Folder:", outputFolder);

            selectedTargetIndex = EditorGUILayout.Popup("Build Target:", selectedTargetIndex, GetBuildTargetOptions());

            useCustomExtension = EditorGUILayout.Toggle("Use Custom File Extension", useCustomExtension);

            if (useCustomExtension)
                GUILayout.Label($"Current Extension: {raidsim.Core.GlobalVariables.assetBundleExtension}", EditorStyles.label);
            else
                GUILayout.Space(17); // Just to keep the layout consistent

            if (GUILayout.Button("Reset Paths to Default"))
            {
                sourceFolder = "Assets";
                outputFolder = "Assets/StreamingAssets";
            }

            if (GUILayout.Button("Build All Asset Bundles"))
            {
                BuildAssetBundles();
            }
        }

        private void BuildAssetBundles()
        {
            if (!Directory.Exists(sourceFolder))
            {
                Debug.LogError("Invalid source folder: " + sourceFolder);
                return;
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            else // This folder might have other files as well but at the moment it only contains asset bundles,
            {    // so we can safely delete it and recreate it to ensure a clean build.
                // Delete only asset bundle files and their .manifest files, preserve other files
                DeletePreviousAssetBundles();
            }

            try
            {
                List<BuildTarget> targetsToBuild = new List<BuildTarget>();

                if (selectedTargetIndex == 0) // Current Build Target
                {
                    targetsToBuild.Add(EditorUserBuildSettings.activeBuildTarget);
                }
                else if (selectedTargetIndex == 1) // All Predefined Targets
                {
                    targetsToBuild.AddRange(buildTargets);
                }
                else // Specific Target
                {
                    targetsToBuild.Add(buildTargets[selectedTargetIndex - 2]);
                }

                foreach (BuildTarget target in targetsToBuild)
                {
                    BuildAssetBundleOptions options = BuildAssetBundleOptions.None;

                    if (target == BuildTarget.WebGL)
                    {
                        options = BuildAssetBundleOptions.ChunkBasedCompression;
                    }

                    Debug.Log($"Building for target: {target} with the following {options}");

                    BuildPipeline.BuildAssetBundles(outputFolder, options, target);

                    if (useCustomExtension)
                    {
                        string extension = GlobalVariables.assetBundleExtension;
                        string[] files = Directory.GetFiles(outputFolder);

                        foreach (string filePath in files)
                        {
                            string fileName = Path.GetFileName(filePath);

                            if (fileName.EndsWith(extension) || fileName.EndsWith($"{extension}.manifest") || fileName.EndsWith($"{extension}.meta"))
                            {
                                File.Delete(filePath);
                                continue;
                            }

                            if (fileName.EndsWith(".manifest") || fileName.EndsWith(".meta"))
                                continue;

                            // Unity's default AssetBundle files are extensionless.
                            // Leave normal files like .json, .ini, .txt, etc. untouched.
                            if (Path.HasExtension(fileName))
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
                }

                Debug.Log("Asset bundle build completed!");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Asset bundle build failed: " + e);
            }
        }

        private string[] GetBuildTargetOptions()
        {
            List<string> options = new List<string> { "Current", "All" };
            foreach (var target in buildTargets)
            {
                options.Add(target.ToString());
            }
            return options.ToArray();
        }

        private void DeletePreviousAssetBundles()
        {
            string[] files = Directory.GetFiles(outputFolder);

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);

                // Ensure that under no circumstances we delete .ini or .json files, as they might be used for configuration and should be preserved ALWAYS
                if (fileName.EndsWith(".ini") || fileName.EndsWith(".json"))
                    continue;

                // Delete asset bundle files (with or without custom extension)
                if (fileName.EndsWith(GlobalVariables.assetBundleExtension))
                {
                    File.Delete(filePath);

                    // Also delete the corresponding .manifest file
                    string manifestPath = filePath + ".manifest";
                    if (File.Exists(manifestPath))
                    {
                        File.Delete(manifestPath);
                    }
                }
                // Delete default asset bundle files (no extension)
                else if (!fileName.Contains(".") || (fileName.EndsWith(".manifest") && !fileName.EndsWith($"{GlobalVariables.assetBundleExtension}.manifest")))
                {
                    // Check if this is a default asset bundle (no extension, or .manifest for a default bundle)
                    string baseNameWithoutManifest = fileName.EndsWith(".manifest") ? fileName.Substring(0, fileName.Length - 9) : fileName;

                    // Only delete if it doesn't have any extension (or if it's a .manifest for a non-extended bundle)
                    if (!baseNameWithoutManifest.Contains(".") || fileName.EndsWith(".manifest"))
                    {
                        if (!fileName.EndsWith(".meta") && !fileName.EndsWith(".manifest.meta"))
                        {
                            File.Delete(filePath);
                        }
                        else if (fileName.EndsWith(".manifest"))
                        {
                            File.Delete(filePath);
                        }
                    }
                }
            }
        }
    }
}
#endif