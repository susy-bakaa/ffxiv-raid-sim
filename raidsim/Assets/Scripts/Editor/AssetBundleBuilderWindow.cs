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

        [SerializeField]
        private bool buildAllAssetBundles = true;

        [SerializeField]
        private List<string> selectedAssetBundles = new List<string>();

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

            DrawAssetBundleSelector();

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

            bool hasBundlesToBuild = buildAllAssetBundles || selectedAssetBundles.Count > 0;
            string buildButtonText = buildAllAssetBundles
                ? "Build All Asset Bundles"
                : $"Build Selected Asset Bundles ({selectedAssetBundles.Count})";

            using (new EditorGUI.DisabledScope(!hasBundlesToBuild))
            {
                if (GUILayout.Button(buildButtonText))
                {
                    BuildAssetBundles();
                }
            }
        }

        private void DrawAssetBundleSelector()
        {
            string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
            RemoveMissingSelections(allBundles);

            Rect rowRect = EditorGUILayout.GetControlRect();
            Rect popupRect = EditorGUI.PrefixLabel(rowRect, new GUIContent("Asset Bundles:"));

            string displayText;
            if (buildAllAssetBundles)
            {
                displayText = $"All ({allBundles.Length})";
            }
            else if (selectedAssetBundles.Count == 0)
            {
                displayText = "(None)";
            }
            else if (selectedAssetBundles.Count <= 3)
            {
                displayText = string.Join(", ", selectedAssetBundles);
            }
            else
            {
                displayText = $"{selectedAssetBundles.Count} selected";
            }

            if (!GUI.Button(popupRect, displayText, EditorStyles.popup))
                return;

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("All"), buildAllAssetBundles, SelectAllAssetBundles);
            menu.AddSeparator(string.Empty);

            if (allBundles.Length == 0)
            {
                menu.AddDisabledItem(new GUIContent("No AssetBundles found"));
            }
            else
            {
                foreach (string bundle in allBundles)
                {
                    string bundleName = bundle;
                    bool isSelected = !buildAllAssetBundles && selectedAssetBundles.Contains(bundleName);
                    menu.AddItem(new GUIContent(bundleName), isSelected, () => ToggleAssetBundle(bundleName));
                }
            }

            menu.DropDown(popupRect);
        }

        private void SelectAllAssetBundles()
        {
            buildAllAssetBundles = true;
            selectedAssetBundles.Clear();
            Repaint();
        }

        private void ToggleAssetBundle(string bundleName)
        {
            if (buildAllAssetBundles)
            {
                buildAllAssetBundles = false;
                selectedAssetBundles.Clear();
            }

            if (!selectedAssetBundles.Remove(bundleName))
                selectedAssetBundles.Add(bundleName);

            Repaint();
        }

        private void RemoveMissingSelections(string[] allBundles)
        {
            HashSet<string> existingBundles = new HashSet<string>(allBundles);
            selectedAssetBundles.RemoveAll(bundle => !existingBundles.Contains(bundle));
        }

        private void BuildAssetBundles()
        {
            if (!Directory.Exists(sourceFolder))
            {
                Debug.LogError("Invalid source folder: " + sourceFolder);
                return;
            }

            string[] allBundleNames = AssetDatabase.GetAllAssetBundleNames();
            RemoveMissingSelections(allBundleNames);

            List<string> bundleNamesToBuild = buildAllAssetBundles
                ? new List<string>(allBundleNames)
                : new List<string>(selectedAssetBundles);

            if (bundleNamesToBuild.Count == 0)
            {
                Debug.LogError("No AssetBundles are selected for building.");
                return;
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            else if (buildAllAssetBundles)
            {
                // A full build keeps the previous clean-build behavior.
                DeletePreviousAssetBundles();
            }
            else
            {
                // A partial build removes only the bundles that are about to be rebuilt.
                DeletePreviousAssetBundles(bundleNamesToBuild);
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

                AssetBundleBuild[] partialBuildMap = buildAllAssetBundles
                    ? null
                    : CreateBuildMap(bundleNamesToBuild);

                if (!buildAllAssetBundles && partialBuildMap.Length == 0)
                {
                    Debug.LogError("None of the selected AssetBundles contain buildable assets.");
                    return;
                }

                foreach (BuildTarget target in targetsToBuild)
                {
                    BuildAssetBundleOptions options = BuildAssetBundleOptions.None;

                    if (target == BuildTarget.WebGL)
                    {
                        options = BuildAssetBundleOptions.ChunkBasedCompression;
                    }

                    Debug.Log($"Building {(buildAllAssetBundles ? "all AssetBundles" : $"{partialBuildMap.Length} selected AssetBundles")} for target: {target} with the following {options}");

                    AssetBundleManifest manifest = buildAllAssetBundles
                        ? BuildPipeline.BuildAssetBundles(outputFolder, options, target)
                        : BuildPipeline.BuildAssetBundles(outputFolder, partialBuildMap, options, target);

                    if (manifest == null)
                    {
                        Debug.LogError($"AssetBundle build returned no manifest for target: {target}");
                        return;
                    }

                    if (useCustomExtension)
                    {
                        AddCustomExtension(manifest.GetAllAssetBundles());
                    }
                }

                AssetDatabase.Refresh();
                Debug.Log("Asset bundle build completed!");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Asset bundle build failed: " + e);
            }
        }

        private AssetBundleBuild[] CreateBuildMap(List<string> bundleNames)
        {
            List<AssetBundleBuild> buildMap = new List<AssetBundleBuild>();

            foreach (string fullBundleName in bundleNames)
            {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(fullBundleName);
                if (assetPaths.Length == 0)
                {
                    Debug.LogWarning($"Skipping empty AssetBundle: {fullBundleName}");
                    continue;
                }

                // Read the importer so AssetBundle variants are preserved correctly.
                AssetImporter importer = AssetImporter.GetAtPath(assetPaths[0]);
                string bundleName = importer != null && !string.IsNullOrEmpty(importer.assetBundleName)
                    ? importer.assetBundleName
                    : fullBundleName;
                string bundleVariant = importer != null
                    ? importer.assetBundleVariant
                    : string.Empty;

                buildMap.Add(new AssetBundleBuild
                {
                    assetBundleName = bundleName,
                    assetBundleVariant = bundleVariant,
                    assetNames = assetPaths
                });
            }

            return buildMap.ToArray();
        }

        private void AddCustomExtension(string[] builtBundleNames)
        {
            foreach (string bundleName in builtBundleNames)
            {
                AddCustomExtension(bundleName);
            }

            // Unity also creates the root manifest bundle named after the output directory.
            string rootManifestBundleName = new DirectoryInfo(outputFolder).Name;
            AddCustomExtension(rootManifestBundleName);
        }

        private void AddCustomExtension(string bundleName)
        {
            string extension = GlobalVariables.assetBundleExtension;
            string sourcePath = Path.Combine(outputFolder, bundleName);
            string destinationPath = sourcePath + extension;

            MoveReplacingExisting(sourcePath, destinationPath);
            MoveReplacingExisting(sourcePath + ".manifest", destinationPath + ".manifest");
            MoveReplacingExisting(sourcePath + ".meta", destinationPath + ".meta");
            MoveReplacingExisting(sourcePath + ".manifest.meta", destinationPath + ".manifest.meta");
        }

        private void MoveReplacingExisting(string sourcePath, string destinationPath)
        {
            if (!File.Exists(sourcePath))
                return;

            if (File.Exists(destinationPath))
                File.Delete(destinationPath);

            File.Move(sourcePath, destinationPath);
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

        private void DeletePreviousAssetBundles(List<string> bundleNames)
        {
            foreach (string bundleName in bundleNames)
            {
                DeleteBundleFiles(bundleName);
            }
        }

        private void DeleteBundleFiles(string bundleName)
        {
            string basePath = Path.Combine(outputFolder, bundleName);

            DeleteIfExists(basePath);
            DeleteIfExists(basePath + ".manifest");
            DeleteIfExists(basePath + ".meta");
            DeleteIfExists(basePath + ".manifest.meta");

            string customPath = basePath + GlobalVariables.assetBundleExtension;
            DeleteIfExists(customPath);
            DeleteIfExists(customPath + ".manifest");
            DeleteIfExists(customPath + ".meta");
            DeleteIfExists(customPath + ".manifest.meta");
        }

        private void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private void DeletePreviousAssetBundles()
        {
            string[] files = Directory.GetFiles(outputFolder, "*", SearchOption.AllDirectories);

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