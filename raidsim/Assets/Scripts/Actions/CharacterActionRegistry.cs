// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections.Generic;
using UnityEngine;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.Actions
{
    public sealed class CharacterActionRegistry : MonoBehaviour
    {
        [SerializeField] private Transform actionsRoot;

        public IReadOnlyList<CharacterAction> AllActions => allActions;

        private readonly List<CharacterAction> allActions = new();
        private readonly Dictionary<string, CharacterAction> byId = new();
        private readonly Dictionary<string, List<CharacterAction>> byName = new();
        private readonly Dictionary<CharacterActionData, List<CharacterAction>> byData = new();


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!actionsRoot)
                actionsRoot = transform.Find("Actions");
        }
#endif

        private void Awake() => Rebuild();

        public void Rebuild()
        {
            FightTimeline timeline = FightTimeline.Instance;

            allActions.Clear();
            byId.Clear();
            byName.Clear();
            byData.Clear();

            if (!actionsRoot)
                actionsRoot = transform; // fallback

            foreach (var action in EnumerateActionsInHierarchyOrder(actionsRoot))
            {
                if (action == null)
                    continue;

                // Add to master list (hierarchy order)
                allActions.Add(action);

                // ID lookup (must be unique)
                if (string.IsNullOrWhiteSpace(action.ActionId))
                {
                    Debug.LogWarning($"Action missing ID: {action.name}", action);
                    continue;
                }
                if (!byId.TryAdd(action.ActionId, action))
                {
                    Debug.LogWarning($"Duplicate ActionId '{action.ActionId}' on '{action.name}'.", action);
                    continue;
                }

                if (timeline != null && timeline.timelineForbiddenActionIds.Contains(action.ActionId))
                {
                    //Debug.LogWarning($"Action '{action.name}' has ID '{action.ActionId}' which is forbidden by the timeline.", action);
                    action.SetPermanentlyUnavailable(); // Mark as unavailable but still include in registry for reference (e.g. to show as disabled in UI).
                }

                // Name groups
                var n = action.Data.actionName;
                if (!byName.TryGetValue(n, out var listN))
                    byName[n] = listN = new List<CharacterAction>(1);
                listN.Add(action);

                // Data groups
                var d = action.Data;
                if (d != null)
                {
                    if (!byData.TryGetValue(d, out var listD))
                        byData[d] = listD = new List<CharacterAction>(1);
                    listD.Add(action);
                }
            }
        }

        public CharacterAction GetById(string id)
            => (id != null && byId.TryGetValue(id, out var a)) ? a : null;

        // Legacy: deterministic "first match", warn if ambiguous
        public CharacterAction GetFirstByName(string name)
        {
            if (name == null || !byName.TryGetValue(name, out var list) || list.Count == 0)
                return null;
            if (list.Count > 1)
                Debug.LogWarning($"Name '{name}' matched {list.Count} actions; using first in hierarchy: {list[0].name}", list[0]);
            return list[0];
        }

        public CharacterAction GetFirstByName(string name, StringComparison comparison)
        {
            if (name == null || allActions == null || allActions.Count < 1)
                return null;

            foreach (CharacterAction action in allActions)
            {
                if (action.Data.actionName.Equals(name, comparison))
                {
                    return action; 
                }
            }
            return null;
        }

        public CharacterAction GetFirstByData(CharacterActionData data)
        {
            if (data == null || !byData.TryGetValue(data, out var list) || list.Count == 0)
                return null;
            if (list.Count > 1)
                Debug.LogWarning($"Data '{data.name}' matched {list.Count} actions; using first in hierarchy: {list[0].name}", list[0]);
            return list[0];
        }

        private static IEnumerable<CharacterAction> EnumerateActionsInHierarchyOrder(Transform root)
        {
            // Pre-order DFS: root, then children left-to-right.
            var stack = new Stack<Transform>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var t = stack.Pop();

                // Emit actions on this transform (order stable)
                foreach (var a in t.GetComponents<CharacterAction>())
                    yield return a;

                // Push children reversed so we process in normal order
                for (int i = t.childCount - 1; i >= 0; --i)
                    stack.Push(t.GetChild(i));
            }
        }
    }
}