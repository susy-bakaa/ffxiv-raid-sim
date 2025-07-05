using UnityEngine;
using UnityEditor;
using dev.susybaka.raidsim.UI;
using UnityEditorInternal;

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

            scenesList = new ReorderableList(serializedObject, scenesProp,
                                             true, true, true, true);

            // Header
            scenesList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Fight Scenes");
            };

            // Element height based on foldout state, with extra bottom padding
            scenesList.elementHeightCallback = index =>
            {
                var element = scenesProp.GetArrayElementAtIndex(index);
                float h = EditorGUIUtility.singleLineHeight;
                float s = EditorGUIUtility.standardVerticalSpacing;
                if (element.isExpanded)
                    // header + scene + spacing + assetBundle + spacing + button + spacing + extra padding
                    return h * 4 + s * 8;
                else
                    // just header + spacing
                    return h + s * 2;
            };

            // Draw each element
            scenesList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = scenesProp.GetArrayElementAtIndex(index);
                float h = EditorGUIUtility.singleLineHeight;
                float s = EditorGUIUtility.standardVerticalSpacing;
                float indent = 15f;

                // Get scene name or fallback to index
                var sceneProp = element.FindPropertyRelative(nameof(FightSelector.TimelineScene.scene));
                string sceneName = string.IsNullOrEmpty(sceneProp.stringValue)
                    ? $"Scene #{index}"
                    : sceneProp.stringValue;

                // Foldout header (indented to avoid overlap with drag handle)
                Rect foldRect = new Rect(rect.x + indent, rect.y + s, rect.width - indent, h);
                element.isExpanded = EditorGUI.Foldout(foldRect, element.isExpanded,
                                                       sceneName, true);

                if (element.isExpanded)
                {
                    // Scene field
                    Rect sceneRect = new Rect(rect.x, rect.y + h + s * 2, rect.width, h);
                    EditorGUI.PropertyField(sceneRect,
                        sceneProp,
                        new GUIContent("Scene"));

                    // AssetBundle field
                    Rect abRect = new Rect(rect.x, rect.y + (h + s) * 2 + s * 2, rect.width, h);
                    EditorGUI.PropertyField(abRect,
                        element.FindPropertyRelative(nameof(FightSelector.TimelineScene.assetBundle)),
                        new GUIContent("Asset Bundle"));

                    // Set-current button
                    Rect btnRect = new Rect(rect.x, rect.y + (h + s) * 3 + s * 3, rect.width, h);
                    if (GUI.Button(btnRect, "Set as Current Scene"))
                    {
                        currentIndexProp.intValue = index;
                    }
                }
            };

            // Add callback
            scenesList.onAddCallback = list =>
            {
                scenesProp.arraySize++;
                serializedObject.ApplyModifiedProperties();
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw default fields except scenes & currentSceneIndex
            DrawPropertiesExcluding(serializedObject,
                                    nameof(FightSelector.scenes),
                                    nameof(FightSelector.currentSceneIndex));

            EditorGUILayout.Space();
            // Draw the foldable, reorderable list
            scenesList.DoLayoutList();

            EditorGUILayout.Space();
            // Draw currentSceneIndex explicitly
            EditorGUILayout.PropertyField(currentIndexProp,
                new GUIContent("Current Scene Index"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}