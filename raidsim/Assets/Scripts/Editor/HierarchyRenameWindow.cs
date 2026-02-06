// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace dev.susybaka.Shared.Editor
{
    public class HierarchyRenameWindow : EditorWindow
    {
        private string findText = "";
        private string replaceText = "";
        private bool includeChildren = true;
        private bool onlyActiveInHierarchy = false;
        private bool caseSensitive = true;
        private bool wholeNameMatch = false;

        private Vector2 scroll;
        private readonly List<RenamePreview> preview = new();
        private int lastSelectionHash;

        private struct RenamePreview
        {
            public GameObject go;
            public string oldName;
            public string newName;
        }

        [MenuItem("Tools/Hierarchy Rename (Find and Replace)")]
        public static void Open()
        {
            GetWindow<HierarchyRenameWindow>("Hierarchy Rename");
        }

        private void OnSelectionChange()
        {
            RebuildPreview();
            Repaint();
        }

        private void OnHierarchyChange()
        {
            // Keep preview from going stale when objects are renamed/moved externally.
            RebuildPreview();
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Pool = current hierarchy selection", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                findText = EditorGUILayout.TextField("Find", findText);
                replaceText = EditorGUILayout.TextField("Replace", replaceText);

                EditorGUILayout.Space(4);
                includeChildren = EditorGUILayout.ToggleLeft("Include children of selected objects", includeChildren);
                onlyActiveInHierarchy = EditorGUILayout.ToggleLeft("Only active objects (activeInHierarchy)", onlyActiveInHierarchy);
                caseSensitive = EditorGUILayout.ToggleLeft("Case sensitive", caseSensitive);
                wholeNameMatch = EditorGUILayout.ToggleLeft("Whole-name match (ignore substring)", wholeNameMatch);
            }

            EditorGUILayout.Space(8);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use Current Selection", GUILayout.Height(28)))
                    RebuildPreview(force: true);

                if (GUILayout.Button("Refresh Preview", GUILayout.Height(28)))
                    RebuildPreview(force: true);
            }

            EditorGUILayout.Space(8);

            DrawPreviewList();

            EditorGUILayout.Space(8);

            using (new EditorGUI.DisabledScope(preview.Count == 0))
            {
                if (GUILayout.Button($"Rename ({preview.Count})", GUILayout.Height(34)))
                {
                    ApplyRename();
                    RebuildPreview(force: true);
                }
            }

            if (preview.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No matching objects in the current pool.\n" +
                    "Select objects in the Hierarchy, set Find/Replace, and refresh.",
                    MessageType.Info);
            }
        }

        private void DrawPreviewList()
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(140), GUILayout.MaxHeight(320));

                foreach (var item in preview)
                {
                    if (item.go == null)
                        continue;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        // Ping/select on click
                        if (GUILayout.Button("+", GUILayout.Width(24)))
                        {
                            Selection.activeObject = item.go;
                            EditorGUIUtility.PingObject(item.go);
                        }

                        EditorGUILayout.ObjectField(item.go, typeof(GameObject), true, GUILayout.Width(220));
                        EditorGUILayout.LabelField(item.oldName, GUILayout.Width(200));
                        EditorGUILayout.LabelField("->", GUILayout.Width(18));
                        EditorGUILayout.LabelField(item.newName);
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void RebuildPreview(bool force = false)
        {
            // Rebuild when selection changed (cheap hash) unless forced
            int selectionHash = ComputeSelectionHash();
            if (!force && selectionHash == lastSelectionHash)
            {
                // Still rebuild if Find/Replace settings changed? If you want that,
                // call RebuildPreview(force:true) from GUI when fields change.
            }
            lastSelectionHash = selectionHash;

            preview.Clear();

            if (string.IsNullOrEmpty(findText))
                return;

            var pool = BuildPoolFromSelection(includeChildren);
            if (pool.Count == 0)
                return;

            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            foreach (var go in pool)
            {
                if (go == null)
                    continue;
                if (onlyActiveInHierarchy && !go.activeInHierarchy)
                    continue;

                string oldName = go.name;
                string newName;

                if (wholeNameMatch)
                {
                    if (!string.Equals(oldName, findText, comparison))
                        continue;

                    newName = replaceText;
                }
                else
                {
                    int idx = oldName.IndexOf(findText, comparison);
                    if (idx < 0)
                        continue;

                    // Replace all occurrences with optional case-sensitivity behavior.
                    // For case-insensitive replace, we do a manual loop.
                    newName = ReplaceAll(oldName, findText, replaceText, comparison);
                }

                if (newName == oldName)
                    continue;

                preview.Add(new RenamePreview { go = go, oldName = oldName, newName = newName });
            }
        }

        private void ApplyRename()
        {
            if (preview.Count == 0)
                return;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Bulk Rename GameObjects");
            int group = Undo.GetCurrentGroup();

            var touchedScenes = new HashSet<SceneAsset>();

            foreach (var item in preview)
            {
                if (item.go == null)
                    continue;

                Undo.RecordObject(item.go, "Rename GameObject");
                item.go.name = item.newName;

                // Mark scene dirty for saving (only for scene objects, not prefabs in project).
                var scene = item.go.scene;
                if (scene.IsValid() && scene.isLoaded)
                    EditorSceneManager.MarkSceneDirty(scene);
            }

            Undo.CollapseUndoOperations(group);
        }

        private static HashSet<GameObject> BuildPoolFromSelection(bool includeChildren)
        {
            var result = new HashSet<GameObject>();
            var selected = Selection.gameObjects;

            foreach (var root in selected)
            {
                if (root == null)
                    continue;

                result.Add(root);

                if (!includeChildren)
                    continue;

                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t != null && t.gameObject != null)
                        result.Add(t.gameObject);
                }
            }

            return result;
        }

        private static int ComputeSelectionHash()
        {
            unchecked
            {
                int hash = 17;
                var selected = Selection.gameObjects;
                hash = hash * 31 + selected.Length;

                for (int i = 0; i < selected.Length; i++)
                {
                    var go = selected[i];
                    hash = hash * 31 + (go ? go.GetInstanceID() : 0);
                }
                return hash;
            }
        }

        private static string ReplaceAll(string input, string find, string replace, StringComparison comparison)
        {
            if (string.IsNullOrEmpty(find))
                return input;

            int startIndex = 0;
            while (true)
            {
                int idx = input.IndexOf(find, startIndex, comparison);
                if (idx < 0)
                    break;

                input = input.Remove(idx, find.Length).Insert(idx, replace);
                startIndex = idx + replace.Length;
            }
            return input;
        }
    }
}