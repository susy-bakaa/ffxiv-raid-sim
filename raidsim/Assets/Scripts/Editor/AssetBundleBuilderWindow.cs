#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class AssetBundleBuilderWindow : EditorWindow
{
    private string sourceFolder = "Assets";
    private string outputFolder = "Assets/StreamingAssets";
    private int selectedTargetIndex = 0;

    private static readonly BuildTarget[] buildTargets = new BuildTarget[]
    {
        BuildTarget.StandaloneWindows64,
        BuildTarget.StandaloneLinux64,
        BuildTarget.WebGL
    };

    [MenuItem("Tools/AssetBundle Builder")]
    public static void ShowWindow()
    {
        GetWindow<AssetBundleBuilderWindow>("AssetBundle Builder");
    }

    private void OnGUI()
    {
        GUILayout.Label("AssetBundle Build Settings", EditorStyles.boldLabel);

        sourceFolder = EditorGUILayout.TextField("Source Folder:", sourceFolder);
        outputFolder = EditorGUILayout.TextField("Output Folder:", outputFolder);

        if (GUILayout.Button("Reset to Default Output"))
        {
            outputFolder = "Assets/StreamingAssets";
        }

        selectedTargetIndex = EditorGUILayout.Popup("Build Target:", selectedTargetIndex, GetBuildTargetOptions());

        if (GUILayout.Button("Build Asset Bundles"))
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
                Debug.Log("Building for target: " + target);
                BuildPipeline.BuildAssetBundles(outputFolder, BuildAssetBundleOptions.None, target);
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
}
#endif