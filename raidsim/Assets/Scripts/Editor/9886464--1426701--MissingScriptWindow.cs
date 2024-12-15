using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class MissingScriptWindow : EditorWindow
{
    [SerializeField]
    private List<string> _missingAssets = new List<string>();
    [SerializeField]
    private List<GameObject> _objectsWithMissingScriptsInCurrentScene = new List<GameObject>();

    [MenuItem("Tools/Find missing scripts")]
    public static void ShowWindow()
    {
        GetWindow(typeof(MissingScriptWindow));
    }

    private void OnGUI()
    {
        GUILayout.Label("Find Missing Scripts", EditorStyles.boldLabel);
        GUILayout.Space(10);

        var myBoxStyle = new GUIStyle(GUI.skin.box);
        myBoxStyle.normal.background = MakeTex(2, 2, new Color(0.6f, 0.6f, 0.6f, 0.5f));

        // Scene block
        GUILayout.BeginVertical(myBoxStyle);
        if (GUILayout.Button("Find in current scene"))
        {
            FindMissingScriptsInCurrentScene();
        }
        GUILayout.Label("Results (Open Scenes):", EditorStyles.boldLabel);

        for (var i = _objectsWithMissingScriptsInCurrentScene.Count - 1; i >= 0; i--)
        {
            if (!_objectsWithMissingScriptsInCurrentScene[i])
            {
                _objectsWithMissingScriptsInCurrentScene.RemoveAt(i);
            }
        }
        
        foreach (var go in _objectsWithMissingScriptsInCurrentScene)
        {
            if (GUILayout.Button(go.name))
            {
                EditorGUIUtility.PingObject(go);
            }
        }
        GUILayout.EndVertical();

        GUILayout.Space(20);

        // Assets block
        GUILayout.BeginVertical(myBoxStyle);
        if (GUILayout.Button("Find in assets"))
        {
            FindMissingScriptsInAssets();
        }
        GUILayout.Label("Results (Assets):", EditorStyles.boldLabel);
        foreach (var path in _missingAssets)
        {
            if (GUILayout.Button(path))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
            }
        }
        GUILayout.EndVertical();
    }

    private static Texture2D MakeTex(int width, int height, Color col)
    {
        var pix = new Color[width * height];
        for (var i = 0; i < pix.Length; i++)
            pix[i] = col;
        var result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void FindMissingScriptsInCurrentScene()
    {
        _objectsWithMissingScriptsInCurrentScene.Clear();
        var allObjects = FindObjectsOfType<GameObject>();
        foreach (var go in allObjects)
        {
            if (go.transform.parent == null) // Only start with root objects
            {
                FindMissingScriptsInGameObjectAndChildren(go);
            }
        }
    }

    private void FindMissingScriptsInGameObjectAndChildren(GameObject go)
    {
        var components = go.GetComponents<Component>();
        var hasMissingScript = components.Any(c => c == null);
        if (hasMissingScript)
        {
            _objectsWithMissingScriptsInCurrentScene.Add(go);
        }
        foreach (Transform child in go.transform) // Recursively check children
        {
            FindMissingScriptsInGameObjectAndChildren(child.gameObject);
        }
    }

    private void FindMissingScriptsInAssets()
    {
        _missingAssets.Clear();
        var allAssets = AssetDatabase.GetAllAssetPaths();
        foreach (var assetPath in allAssets)
        {
            if (assetPath.StartsWith("Packages/"))
            {
                continue;
            }
            
            if (Path.GetExtension(assetPath) == ".prefab")
            {
                var assetRoot = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                var components = assetRoot.GetComponentsInChildren<Component>(true);
                var hasMissingScript = components.Any(c => c == null);
                if (hasMissingScript)
                {
                    _missingAssets.Add(assetPath);
                }
            }
        }
    }
}
