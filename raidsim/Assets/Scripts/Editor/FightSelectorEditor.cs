using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using dev.susybaka.raidsim.UI;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

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
            scenesProp = serializedObject.FindProperty(nameof(FightSelector.scenes));
            currentIndexProp = serializedObject.FindProperty(nameof(FightSelector.currentSceneIndex));
            CreateList();
        }

        void OnDisable() => scenesList = null;

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
                if (!element.isExpanded)
                    return h + s * 2;

                // Calculate expanded height with dropdown
                var bundlesProp = element.FindPropertyRelative(nameof(FightSelector.TimelineScene.assetBundles));
                // one line for dropdown
                float total = 0;
                total += h + s;    // foldout
                total += h + s;    // scene field
                total += h + s;    // dropdown
                total += h + s;    // set button
                total += h + s * 2;// load button + padding
                return total;
            };

            scenesList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = scenesProp.GetArrayElementAtIndex(index);
                if (element == null)
                    return;

                float h = EditorGUIUtility.singleLineHeight;
                float s = EditorGUIUtility.standardVerticalSpacing;
                float indent = 15f;

                var sceneProp = element.FindPropertyRelative(nameof(FightSelector.TimelineScene.scene));
                var bundlesProp = element.FindPropertyRelative(nameof(FightSelector.TimelineScene.assetBundles));

                string sceneName = string.IsNullOrEmpty(sceneProp.stringValue)
                    ? $"Scene #{index}" : sceneProp.stringValue;

                // Foldout header
                Rect foldRect = new Rect(rect.x + indent, rect.y + s, rect.width - indent, h);
                element.isExpanded = EditorGUI.Foldout(foldRect, element.isExpanded, sceneName, true);
                if (!element.isExpanded)
                    return;

                float y = rect.y + h + s * 2;

                // Scene field
                var sceneRect = new Rect(rect.x, y, rect.width, h);
                EditorGUI.PropertyField(sceneRect, sceneProp, new GUIContent("Scene"));
                y += h + s;

                // AssetBundles multi-select dropdown
                string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
                var selectedBundles = new List<string>();
                for (int bi = 0; bi < bundlesProp.arraySize; bi++)
                    selectedBundles.Add(bundlesProp.GetArrayElementAtIndex(bi).stringValue);

                Rect labelRect = new Rect(rect.x, y, EditorGUIUtility.labelWidth, h);
                Rect popupRect = new Rect(rect.x + EditorGUIUtility.labelWidth, y,
                                          rect.width - EditorGUIUtility.labelWidth, h);

                EditorGUI.LabelField(labelRect, "Asset Bundles");
                string display = selectedBundles.Count > 0 ? string.Join(", ", selectedBundles) : "(None)";
                if (GUI.Button(popupRect, display, EditorStyles.popup))
                {
                    var menu = new GenericMenu();
                    foreach (var bundle in allBundles)
                    {
                        bool isSel = selectedBundles.Contains(bundle);
                        menu.AddItem(new GUIContent(bundle), isSel, () =>
                        {
                            serializedObject.Update();
                            var newSel = new List<string>(selectedBundles);
                            if (isSel)
                                newSel.Remove(bundle);
                            else
                                newSel.Add(bundle);

                            bundlesProp.arraySize = newSel.Count;
                            for (int j = 0; j < newSel.Count; j++)
                                bundlesProp.GetArrayElementAtIndex(j).stringValue = newSel[j];

                            serializedObject.ApplyModifiedProperties();
                        });
                    }
                    menu.DropDown(popupRect);
                }
                y += h + s;

                // Set as Current Scene button
                var btnRect = new Rect(rect.x, y, rect.width, h);
                if (GUI.Button(btnRect, "Set as Current Scene"))
                    currentIndexProp.intValue = index;
                y += h + s;

                // Load Scene button
                var loadRect = new Rect(rect.x, y, rect.width, h);
                if (GUI.Button(loadRect, "Load Scene"))
                {
                    string path = EditorBuildSettings.scenes
                        .FirstOrDefault(s => s.path.Contains(sceneProp.stringValue))?.path;
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (Application.isPlaying)
                            UnityEngine.SceneManagement.SceneManager.LoadScene(path);
                        else
                        {
                            EditorSceneManager.SaveOpenScenes();
                            EditorSceneManager.OpenScene(path);
                            GUIUtility.ExitGUI();
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
            if (scenesList == null)
                CreateList();

            serializedObject.Update();
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
