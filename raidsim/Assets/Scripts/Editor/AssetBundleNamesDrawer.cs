// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using dev.susybaka.Shared.Attributes;

namespace dev.susybaka.Shared.Editor
{
    [CustomPropertyDrawer(typeof(AssetBundleNamesAttribute), true)]
    public class AssetBundleNamesDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Only one line tall
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin property block (handles prefab overrides, undo, etc.)
            EditorGUI.BeginProperty(position, label, property);

            // If not an array of strings, fallback to default field
            if (!property.isArray)
            {
                EditorGUI.PropertyField(position, property, label);
                EditorGUI.EndProperty();
                return;
            }

            // Fetch all asset bundle names
            string[] allBundles = AssetDatabase.GetAllAssetBundleNames();

            // Gather current selections
            List<string> selected = new List<string>();
            for (int i = 0; i < property.arraySize; i++)
            {
                var elem = property.GetArrayElementAtIndex(i);
                if (elem.propertyType == SerializedPropertyType.String)
                    selected.Add(elem.stringValue);
            }

            // Split label and popup rects
            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            Rect popupRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y,
                                      position.width - EditorGUIUtility.labelWidth, position.height);

            // Draw label
            EditorGUI.LabelField(labelRect, label);

            // Draw popup with current selections
            string display = selected.Count > 0 ? string.Join(", ", selected) : "(None)";
            if (GUI.Button(popupRect, display, EditorStyles.popup))
            {
                var menu = new GenericMenu();
                foreach (var bundle in allBundles)
                {
                    bool isSel = selected.Contains(bundle);
                    menu.AddItem(new GUIContent(bundle), isSel, () =>
                    {
                        // Toggle selection
                        var so = property.serializedObject;
                        so.Update();
                        var newSel = new List<string>(selected);
                        if (isSel)
                            newSel.Remove(bundle);
                        else
                            newSel.Add(bundle);

                        // Write back to the array
                        property.arraySize = newSel.Count;
                        for (int j = 0; j < newSel.Count; j++)
                        {
                            var e = property.GetArrayElementAtIndex(j);
                            if (e.propertyType == SerializedPropertyType.String)
                                e.stringValue = newSel[j];
                        }
                        so.ApplyModifiedProperties();
                    });
                }
                menu.DropDown(popupRect);
            }

            EditorGUI.EndProperty();
        }
    }
}