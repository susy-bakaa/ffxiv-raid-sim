#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEditor.SceneManagement;

public static class LogBrokenEvents
{
    static readonly HashSet<string> brokenTargetTypes = new HashSet<string>
    {
        "ActionController",
        "TargetController",
        "StateTrigger"
    };

    static readonly HashSet<string> brokenMethodNames = new HashSet<string>
    {
        "PerformAutoAction",
        "PerformAction",
        "PerformActionHidden",
        "PerformActionUnrestricted",
        "SetTarget",
        "CastTarget",
        "CastSource"
    };

    [MenuItem("Tools/Log Broken and Outdated UnityEvents")]
    public static void LogBrokenUnityEvents()
    {
        int brokenCount = 0;
        int watchedCount = 0;

        // Process all prefabs
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Skip built-in or package scenes
            if (path.StartsWith("Packages/") || path.StartsWith("Library/"))
                continue;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab)
            {
                CheckObject(prefab, path, ref brokenCount, ref watchedCount);
            }
        }

        // Process all scenes
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Skip built-in or package scenes
            if (path.StartsWith("Packages/") || path.StartsWith("Library/"))
                continue;

            try
            {
                // Check if the scene is already open
                bool alreadyOpen = false;
                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    var openScene = EditorSceneManager.GetSceneAt(i);
                    if (openScene.path == path)
                    {
                        alreadyOpen = true;
                        // Use the already open scene
                        foreach (var root in openScene.GetRootGameObjects())
                        {
                            CheckObject(root, path, ref brokenCount, ref watchedCount);
                        }
                        break;
                    }
                }

                if (!alreadyOpen)
                {
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    foreach (var root in scene.GetRootGameObjects())
                    {
                        CheckObject(root, path, ref brokenCount, ref watchedCount);
                    }
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Skipped unreadable scene: {path}\n{ex.Message}");
            }
        }

        Debug.Log($"Done scanning. Found {brokenCount} broken events, {watchedCount} outdated UnityEvent references.");
    }

    private static void CheckObject(GameObject go, string assetPath, ref int brokenCount, ref int watchedCount)
    {
        var components = go.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var comp in components)
        {
            if (comp == null)
                continue;

            SerializedObject so = new SerializedObject(comp);
            SerializedProperty prop = so.GetIterator();
            while (prop.NextVisible(true))
            {
                if (prop.propertyType == SerializedPropertyType.Generic && prop.type.Contains("UnityEvent"))
                {
                    var eventProp = prop.Copy();
                    SerializedProperty calls = eventProp.FindPropertyRelative("m_PersistentCalls.m_Calls");
                    if (calls == null || !calls.isArray)
                        continue;

                    for (int i = 0; i < calls.arraySize; i++)
                    {
                        SerializedProperty call = calls.GetArrayElementAtIndex(i);
                        var targetProp = call.FindPropertyRelative("m_Target");
                        var methodNameProp = call.FindPropertyRelative("m_MethodName");
                        var typeNameProp = call.FindPropertyRelative("m_TargetAssemblyTypeName");

                        Object target = targetProp.objectReferenceValue;
                        string methodName = methodNameProp.stringValue;
                        string typeNameFull = typeNameProp.stringValue;
                        string typeName = typeNameFull?.Split(',')[0].Trim();

                        bool isBroken = false;
                        bool isWatchedBroken = false;

                        if (target == null || string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(typeName))
                        {
                            isBroken = true;
                        }
                        else
                        {
                            var targetType = target.GetType();
                            bool methodFound = false;

                            foreach (var m in targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                            {
                                if (m.Name == methodName)
                                {
                                    methodFound = true;
                                    break;
                                }
                            }

                            if (!methodFound)
                                isBroken = true;
                        }

                        if (brokenTargetTypes.Contains(typeName) && brokenMethodNames.Contains(methodName))
                        {
                            isWatchedBroken = true;
                        }

                        if (isBroken)
                        {
                            brokenCount++;
                            Debug.LogWarning($"Broken UnityEvent in {assetPath} on {comp.name} -> Method: {methodName}, Target: {typeNameFull}", comp);
                        }
                        else if (isWatchedBroken)
                        {
                            watchedCount++;
                            Debug.Log($"Outdated UnityEvent reference in {assetPath} on {comp.name} -> Method: {methodName}, Target: {typeNameFull}", comp);
                        }
                    }
                }
            }
        }
    }
}
#endif