// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using UnityEngine;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Actions
{
    [RequireComponent(typeof(ActionOverrideResolver))]
    public sealed class RoleOverrideDriver : MonoBehaviour
    {
        [Serializable]
        public struct Rule
        {
            public Role role;

            [Tooltip("Base ActionId, e.g. AutoAttack")]
            public string baseActionId;

            [Tooltip("ActionId to execute for this role")]
            public string executionActionId;

            [Tooltip("Optional: ActionId to show in UI. If empty, executionActionId is used.")]
            public string presentationActionId;

            [Tooltip("Higher wins against other override sources.")]
            public int priority;
        }

        [SerializeField] private CharacterState character;
        [SerializeField] private ActionOverrideResolver resolver;
        [SerializeField] private Rule[] rules;

        private const string SourceKey = "role";

        private void Reset()
        {
            if (!character)
                character = transform.GetComponentInParents<CharacterState>();
            if (!resolver)
                resolver = GetComponent<ActionOverrideResolver>();
        }

        private void OnEnable()
        {
            ApplyForCurrentRole();

            character.onRoleChanged.AddListener(HandleRoleChanged);
        }

        private void OnDisable()
        {
            if (character != null) character.onRoleChanged.RemoveListener(HandleRoleChanged);

            if (resolver)
                resolver.ClearAllFromSource(SourceKey);
        }

        private void HandleRoleChanged(Role role)
        {
            ApplyForCurrentRole();
        }

        public void ApplyForCurrentRole()
        {
            if (!character || !resolver)
                return;

            resolver.ClearAllFromSource(SourceKey);

            var currentRole = character.role;

            for (int i = 0; i < rules.Length; i++)
            {
                var r = rules[i];

                if (r.role != currentRole)
                    continue;

                if (string.IsNullOrWhiteSpace(r.baseActionId) ||
                    string.IsNullOrWhiteSpace(r.executionActionId))
                    continue;

                var presentationId = string.IsNullOrWhiteSpace(r.presentationActionId)
                    ? r.executionActionId
                    : r.presentationActionId;

                resolver.SetOverride(
                    sourceKey: SourceKey,
                    baseId: r.baseActionId,
                    executionId: r.executionActionId,
                    presentationId: presentationId,
                    durationSeconds: 0f,
                    priority: r.priority
                );
            }
        }
    }
}