using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace susy_baka.Shared.Utils.Editor
{
    [ExecuteAlways]
    public class SceneLoaderWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<SceneAsset> sceneAssets = new List<SceneAsset>();
        private bool filterScenes;
        private const string FilterScenesKey = "susy_baka.Shared.Utils.Editor.SceneLoader.FilterScenes";
        private const string LastSceneKey = "susy_baka.Shared.Utils.Editor.SceneLoader.LastScene";

        private string previousScenePath = "";
        private string previousSceneName = "No Previous Scene";

        [MenuItem("Tools/Scene Loader")]
        public static void ShowWindow()
        {
            SceneLoaderWindow window = GetWindow<SceneLoaderWindow>("Scene Loader");

            // Set the icon for the window using Unity's default scene icon
            GUIContent titleContent = new GUIContent("Scene Loader", EditorGUIUtility.IconContent("SceneAsset Icon").image);
            window.titleContent = titleContent;
        }

        private void OnEnable()
        {
            // Load the checkbox state from EditorPrefs
            filterScenes = EditorPrefs.GetBool(FilterScenesKey, false);

            // Load the previous scene path and name
            previousScenePath = EditorPrefs.GetString(LastSceneKey, "");
            if (!string.IsNullOrEmpty(previousScenePath))
            {
                previousSceneName = System.IO.Path.GetFileNameWithoutExtension(previousScenePath);
            }

            RefreshSceneList();
        }

        private void OnGUI()
        {
            GUILayout.Label("Available Scenes", EditorStyles.boldLabel);

            // Add a checkbox for filtering scenes
            bool newFilterState = GUILayout.Toggle(filterScenes, "Filter Scenes (Assets/Scenes and Build Settings)");
            if (newFilterState != filterScenes)
            {
                filterScenes = newFilterState;
                // Save the checkbox state
                EditorPrefs.SetBool(FilterScenesKey, filterScenes);
                // Refresh the scene list based on the new filter state
                RefreshSceneList();
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            // Add the option to load the previous scene
            if (GUILayout.Button($"Load Previous Scene: {previousSceneName}"))
            {
                LoadPreviousSceneEditor();
            }

            foreach (var sceneAsset in sceneAssets)
            {
                if (GUILayout.Button(sceneAsset.name))
                {
                    // Load the selected scene
                    LoadScene(sceneAsset);
                }
            }
            GUILayout.EndScrollView();
        }

        private void RefreshSceneList()
        {
            sceneAssets.Clear();
            string[] guids = AssetDatabase.FindAssets("t:Scene");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

                if (sceneAsset != null)
                {
                    // Apply the filtering logic
                    if (filterScenes)
                    {
                        bool inAssetsScenes = path.StartsWith("Assets/Scenes");
                        bool inBuildSettings = IsSceneInBuildSettings(path);

                        if (inAssetsScenes || inBuildSettings)
                        {
                            sceneAssets.Add(sceneAsset);
                        }
                    }
                    else
                    {
                        sceneAssets.Add(sceneAsset);
                    }
                }
            }
        }

        private bool IsSceneInBuildSettings(string scenePath)
        {
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                if (buildScene.path == scenePath)
                {
                    return true;
                }
            }
            return false;
        }

        private void LoadScene(SceneAsset sceneAsset)
        {
            // Get the current active scene path before switching to a new scene
            string currentScenePath = EditorSceneManager.GetActiveScene().path;

            // Only save the current scene as the "last scene" if it's not the new one being loaded
            if (!string.IsNullOrEmpty(currentScenePath) && currentScenePath != AssetDatabase.GetAssetPath(sceneAsset))
            {
                // Save the current scene as the "last scene" in EditorPrefs
                EditorPrefs.SetString(LastSceneKey, currentScenePath);

                // Update the previous scene variables for UI display
                previousScenePath = currentScenePath;
                previousSceneName = System.IO.Path.GetFileNameWithoutExtension(currentScenePath);
            }

            // Load the new scene
            string newScenePath = AssetDatabase.GetAssetPath(sceneAsset);
            if (!string.IsNullOrEmpty(newScenePath))
            {
                EditorSceneManager.SaveOpenScenes();
                EditorSceneManager.OpenScene(newScenePath);
            }
        }

        private void LoadPreviousSceneEditor()
        {
            if (!string.IsNullOrEmpty(previousScenePath))
            {
                EditorSceneManager.SaveOpenScenes();
                EditorSceneManager.OpenScene(previousScenePath);
            }
            else
            {
                Debug.LogWarning("No previously loaded scene found.");
            }
        }
    }
}