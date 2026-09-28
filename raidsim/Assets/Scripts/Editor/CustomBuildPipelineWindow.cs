// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.Editor
{
    public class CustomBuildPipelineWindow : EditorWindow
    {
        [SerializeField]
        private bool buildWindows = true;
        [SerializeField]
        private bool buildLinux = true;
        [SerializeField]
        private bool buildWebGL = true;
        [SerializeField]
        private bool packageBuilds = true;
        [SerializeField]
        private bool rebuildProgram = true;
        [SerializeField]
        private bool rebuildBundles = true;
        [SerializeField]
        private bool useCustomExtension = true;
        [SerializeField]
        private string unityVersion = "1.0.0";
        [SerializeField]
        private int versionNumber = 0;

        [SerializeField]
        private bool initialized;

        [MenuItem("Tools/Custom Build Pipeline Window")]
        public static void ShowWindow()
        {
            GetWindow<CustomBuildPipelineWindow>(false, "Build Pipeline", true);
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Build Pipeline", EditorGUIUtility.IconContent("BuildSettings.Editor").image);
            if (!initialized)
            {
                unityVersion = PlayerSettings.bundleVersion;
                versionNumber = GlobalVariables.versionNumber;
                initialized = true;
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Build Pipeline Settings", EditorStyles.boldLabel);

            DrawPlatformSelector();

            unityVersion = EditorGUILayout.TextField("Unity Version", unityVersion);
            versionNumber = EditorGUILayout.IntField("Global Version Number", versionNumber);
            rebuildProgram = EditorGUILayout.Toggle("Rebuild Program", rebuildProgram);
            rebuildBundles = EditorGUILayout.Toggle("Rebuild Asset Bundles", rebuildBundles);
            useCustomExtension = EditorGUILayout.Toggle("Use Bundle File Extension", useCustomExtension);

            if (useCustomExtension)
                GUILayout.Label($"Current Extension: {GlobalVariables.assetBundleExtension}", EditorStyles.label);
            else
                GUILayout.Space(17); // Just to keep the layout consistent

            packageBuilds = EditorGUILayout.Toggle("Package All Platforms", packageBuilds);
            EditorGUILayout.HelpBox("Packaging requires existing builds for all three platforms. Select no build platforms to package only.", MessageType.Info);

            if (GUILayout.Button("Run Full Build Pipeline"))
            {
                CustomBuildPipeline.BuildWindows = buildWindows;
                CustomBuildPipeline.BuildLinux = buildLinux;
                CustomBuildPipeline.BuildWebGL = buildWebGL;
                CustomBuildPipeline.ShouldPackageBuilds = packageBuilds;
                CustomBuildPipeline.ShouldRebuildProgram = rebuildProgram;
                CustomBuildPipeline.ShouldRebuildAssetBundles = rebuildBundles;
                CustomBuildPipeline.useCustomExtension = useCustomExtension;
                CustomBuildPipeline.ManualUnityVersion = unityVersion;
                CustomBuildPipeline.ManualVersionNumber = versionNumber;
                CustomBuildPipeline.RunFullBuildPipeline();
            }
        }

        private void DrawPlatformSelector()
        {
            bool buildAllPlatforms = buildWindows && buildLinux && buildWebGL;
            List<string> selectedPlatforms = new List<string>();
            if (buildWindows)
                selectedPlatforms.Add("Windows64");
            if (buildLinux)
                selectedPlatforms.Add("Linux64");
            if (buildWebGL)
                selectedPlatforms.Add("WebGL");

            Rect rowRect = EditorGUILayout.GetControlRect();
            Rect popupRect = EditorGUI.PrefixLabel(rowRect, new GUIContent("Build Platforms:"));
            string displayText = buildAllPlatforms ? "All (3)" : selectedPlatforms.Count == 0
                ? "(None)" : string.Join(", ", selectedPlatforms);

            if (!GUI.Button(popupRect, displayText, EditorStyles.popup))
                return;

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("All"), buildAllPlatforms, SelectAllPlatforms);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Standalone Windows x64"), !buildAllPlatforms && buildWindows, () => TogglePlatform(BuildTarget.StandaloneWindows64));
            menu.AddItem(new GUIContent("Standalone Linux x64"), !buildAllPlatforms && buildLinux, () => TogglePlatform(BuildTarget.StandaloneLinux64));
            menu.AddItem(new GUIContent("WebGL"), !buildAllPlatforms && buildWebGL, () => TogglePlatform(BuildTarget.WebGL));
            menu.DropDown(popupRect);
        }

        private void SelectAllPlatforms()
        {
            buildWindows = true;
            buildLinux = true;
            buildWebGL = true;
            Repaint();
        }

        private void TogglePlatform(BuildTarget target)
        {
            if (buildWindows && buildLinux && buildWebGL)
            {
                buildWindows = false;
                buildLinux = false;
                buildWebGL = false;
            }

            switch (target)
            {
                case BuildTarget.StandaloneWindows64:
                    buildWindows = !buildWindows;
                    break;
                case BuildTarget.StandaloneLinux64:
                    buildLinux = !buildLinux;
                    break;
                case BuildTarget.WebGL:
                    buildWebGL = !buildWebGL;
                    break;
            }

            Repaint();
        }
    }
}