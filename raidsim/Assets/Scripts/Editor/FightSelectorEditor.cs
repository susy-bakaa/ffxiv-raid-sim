using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using dev.susybaka.raidsim.UI;

namespace dev.susybaka.raidsim.Editor
{
    [CustomEditor(typeof(FightSelector))]
    [CanEditMultipleObjects]
    public class FightSelectorEditor : UnityEditor.Editor
    {
        SerializedProperty scenesProp;
        SerializedProperty currentIndexProp;
        ReorderableList scenesList;

        void OnEnable()
        {
            // (Re)grab references every time we enable
            scenesProp = serializedObject.FindProperty(nameof(FightSelector.scenes));
            currentIndexProp = serializedObject.FindProperty(nameof(FightSelector.currentSceneIndex));
            CreateList();
        }

        void OnDisable()
        {
            // Force rebuild on next enable if domain reload or scene load happened
            scenesList = null;
        }

        // Call from OnEnable and any time we detect scenesList == null
        void CreateList()
        {
            scenesList = new ReorderableList(serializedObject, scenesProp, true, true, true, true);

            scenesList.drawHeaderCallback = rect =>
                EditorGUI.LabelField(rect, "Fight Scenes");

            scenesList.elementHeightCallback = index =>
            {
                var element = scenesProp.GetArrayElementAtIndex(index);
                float h = EditorGUIUtility.singleLineHeight;
                float s = EditorGUIUtility.standardVerticalSpacing;
                return element.isExpanded
                    ? h * 4 + s * 18  // expanded + extra padding
                    : h + s * 2;     // collapsed
            };

            scenesList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = scenesProp.GetArrayElementAtIndex(index);
                if (element == null)
                    return;  // guard

                float h = EditorGUIUtility.singleLineHeight;
                float s = EditorGUIUtility.standardVerticalSpacing;
                float indent = 15f;

                var sceneProp = element.FindPropertyRelative(nameof(FightSelector.TimelineScene.scene));
                string sceneName = string.IsNullOrEmpty(sceneProp.stringValue)
                    ? $"Scene #{index}"
                    : sceneProp.stringValue;

                // Foldout (indented past drag handle)
                var foldRect = new Rect(rect.x + indent, rect.y + s, rect.width - indent, h);
                element.isExpanded = EditorGUI.Foldout(foldRect, element.isExpanded, sceneName, true);

                if (element.isExpanded)
                {
                    // Scene field
                    var sceneRect = new Rect(rect.x, rect.y + h + s * 2, rect.width, h);
                    EditorGUI.PropertyField(sceneRect, sceneProp, new GUIContent("Scene"));

                    // AssetBundle field
                    var abRect = new Rect(rect.x, rect.y + (h + s) * 2 + s * 2, rect.width, h);
                    EditorGUI.PropertyField(abRect,
                        element.FindPropertyRelative(nameof(FightSelector.TimelineScene.assetBundle)),
                        new GUIContent("Asset Bundle"));

                    // “Set as Current Scene” button
                    var btnRect = new Rect(rect.x, rect.y + (h + s) * 3 + s * 3, rect.width, h);
                    if (GUI.Button(btnRect, "Set as Current Scene"))
                        currentIndexProp.intValue = index;

                    // “Load Scene” button
                    var loadRect = new Rect(rect.x, rect.y + (h + s) * 4 + s * 3, rect.width, h);
                    if (GUI.Button(loadRect, "Load Scene"))
                    {
                        string path = EditorBuildSettings.scenes
                            .FirstOrDefault(s => s.path.Contains(sceneProp.stringValue))
                            ?.path;
                        if (!string.IsNullOrEmpty(path))
                        {
                            if (Application.isPlaying)
                                UnityEngine.SceneManagement.SceneManager.LoadScene(path);
                            else
                            {
                                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
                                GUIUtility.ExitGUI();  // stop drawing this inspector now
                            }
                        }
                    }
                }
            };

            scenesList.onAddCallback = list =>
            {
                scenesProp.arraySize++;
                serializedObject.ApplyModifiedProperties();
            };
        }

        public override void OnInspectorGUI()
        {
            // If somehow our list got disposed, rebuild it
            if (scenesList == null)
                CreateList();

            serializedObject.Update();

            // Draw everything except those two props
            DrawPropertiesExcluding(serializedObject,
                                    nameof(FightSelector.scenes),
                                    nameof(FightSelector.currentSceneIndex));

            EditorGUILayout.Space();
            scenesList.DoLayoutList();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(currentIndexProp, new GUIContent("Current Scene Index"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}