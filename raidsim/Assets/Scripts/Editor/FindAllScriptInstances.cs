// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace dev.susybaka.raidsim.Editor
{
    public static class FindAllScriptInstances
    {
        [MenuItem("Tools/Find All Instances Of Selected Script")]
        private static void FindUsageOfSelectedScript()
        {
            MonoScript script = Selection.activeObject as MonoScript;

            if (script == null)
            {
                Debug.LogError("Select a C# script asset first.");
                return;
            }

            Type targetType = script.GetClass();

            if (targetType == null)
            {
                Debug.LogError($"Could not resolve class from script: {script.name}");
                return;
            }

            if (!typeof(Component).IsAssignableFrom(targetType))
            {
                Debug.LogError($"{targetType.Name} is not a Component/MonoBehaviour type.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("Scan cancelled.");
                return;
            }

            SceneSetup[] previousSceneSetup = EditorSceneManager.GetSceneManagerSetup();

            try
            {
                List<string> results = new();

                ScanPrefabs(targetType, results);
                ScanBuildScenes(targetType, results);

                if (results.Count == 0)
                {
                    Debug.Log($"No usages found for {targetType.Name}.");
                    return;
                }

                StringBuilder sb = new();
                sb.AppendLine($"Found {results.Count} usage(s) of {targetType.Name}:");
                sb.AppendLine();

                foreach (string result in results)
                    sb.AppendLine(result);

                Debug.Log(sb.ToString());
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(previousSceneSetup);
            }
        }

        private static void ScanPrefabs(Type targetType, List<string> results)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = null;

                try
                {
                    root = PrefabUtility.LoadPrefabContents(path);

                    Component[] components = root.GetComponentsInChildren(targetType, true);

                    foreach (Component component in components)
                    {
                        results.Add(
                            $"[Prefab] {path} -> {GetHierarchyPath(component.transform, root.transform)}"
                        );
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to scan prefab: {path}\n{e}");
                }
                finally
                {
                    if (root != null)
                        PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static void ScanBuildScenes(Type targetType, List<string> results)
        {
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled)
                    continue;

                string path = buildScene.path;

                try
                {
                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        Component[] components = root.GetComponentsInChildren(targetType, true);

                        foreach (Component component in components)
                        {
                            results.Add(
                                $"[Scene] {path} -> {GetHierarchyPath(component.transform, null)}"
                            );
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to scan scene: {path}\n{e}");
                }
            }
        }

        private static string GetHierarchyPath(Transform transform, Transform stopAt)
        {
            Stack<string> names = new();

            Transform current = transform;

            while (current != null && current != stopAt)
            {
                names.Push(current.name);
                current = current.parent;
            }

            if (stopAt != null)
                names.Push(stopAt.name);

            return string.Join("/", names);
        }
    }
}