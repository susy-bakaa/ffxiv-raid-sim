// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.Targeting
{
    public class TargetNode : MonoBehaviour
    {
        private SphereCollider sphereCollider;
        private CapsuleCollider capsuleCollider;

        [SerializeField] private CharacterState characterState;
        [SerializeField] private ActionController actionController;
        [SerializeField] private TargetController targetController;
        [SerializeField] private bool allowAutomaticSetup = true;
        [SerializeField] private bool targetable;
        public bool Targetable { get { return characterState != null ? !characterState.untargetable.value : targetable; } }
        [SerializeField] private int group;
        public int Group { get { return group; } }
        public float hitboxRadius = 0f;
        private float originalHitboxRadius = 0f;

        public UnityEvent onTarget;
        public UnityEvent onDetarget;

        public CanvasGroup[] highlightGroups;

        private void Awake()
        {
            sphereCollider = GetComponent<SphereCollider>();
            capsuleCollider = GetComponent<CapsuleCollider>();

            if (capsuleCollider != null && hitboxRadius <= 0f)
                hitboxRadius = capsuleCollider.radius;
            if (sphereCollider != null && hitboxRadius <= 0f)
                hitboxRadius = sphereCollider.radius;

            originalHitboxRadius = hitboxRadius;

            SetupCharacterState();
            SetupActionController();
            SetupTargetController();
        }

        private void Update()
        {
            if (characterState != null)
            {
                targetable = !characterState.untargetable.value;
            }
        }

        public bool TryGetCharacterState(out CharacterState characterState)
        {
            if (this.characterState == null)
            {
                SetupCharacterState();
            }

            characterState = this.characterState;

            return characterState != null;
        }

        public bool TryGetActionController(out ActionController actionController)
        {
            if (this.actionController == null)
            {
                SetupActionController();
            }

            actionController = this.actionController;

            return actionController != null;
        }

        public bool TryGetTargetController(out TargetController targetController)
        {
            if (this.targetController == null)
            {
                SetupTargetController();
            }

            targetController = this.targetController;

            return targetController != null;
        }

        public CharacterState GetCharacterState()
        {
            return characterState ?? SetupCharacterState();
        }

        public ActionController GetActionController()
        {
            return actionController ?? SetupActionController();
        }

        public TargetController GetTargetController()
        {
            return targetController ?? SetupTargetController();
        }

        private CharacterState SetupCharacterState()
        {
            if (!allowAutomaticSetup)
                return characterState;

            if (characterState == null)
            {
                if (TryGetComponent(out CharacterState resultSelf))
                {
                    characterState = resultSelf;
                }
                else if (transform.parent.TryGetComponent(out CharacterState resultParent))
                {
                    characterState = resultParent;
                }
                else if (transform.root.TryGetComponent(out CharacterState resultRoot))
                {
                    characterState = resultRoot;
                }
            }

            return characterState;
        }

        private ActionController SetupActionController()
        {
            if (!allowAutomaticSetup)
                return actionController;

            if (actionController == null)
            {
                if (TryGetComponent(out ActionController resultSelf))
                {
                    actionController = resultSelf;
                }
                else if (transform.parent.TryGetComponent(out ActionController resultParent))
                {
                    actionController = resultParent;
                }
                else if (transform.root.TryGetComponent(out ActionController resultRoot))
                {
                    actionController = resultRoot;
                }
            }

            return actionController;
        }

        private TargetController SetupTargetController()
        {
            if (!allowAutomaticSetup)
                return targetController;

            if (actionController == null)
            {
                if (TryGetComponent(out TargetController resultSelf))
                {
                    targetController = resultSelf;
                }
                else if (transform.parent.TryGetComponent(out TargetController resultParent))
                {
                    targetController = resultParent;
                }
                else if (transform.root.TryGetComponent(out TargetController resultRoot))
                {
                    targetController = resultRoot;
                }
            }

            return targetController;
        }

        public bool IsNodeInGroup(int group)
        {
            if (this != null)
            {
                if (this.Group == group)
                    return true;
            }
            return false;
        }

        public bool IsNodeInGroups(int[] groups)
        {
            for (int i = 0; i < groups.Length; i++)
            {
                if (IsNodeInGroup(groups[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public void UpdateUserInterface(float alpha, float duration)
        {
            if (highlightGroups != null && highlightGroups.Length > 0)
            {
                for (int i = 0; i < highlightGroups.Length; i++)
                {
                    if (highlightGroups[i] == null)
                        continue;

                    if (highlightGroups[i].alpha <= 0f || highlightGroups[i].alpha >= 1f)
                    {
                        if (!FightTimeline.Instance.paused)
                            highlightGroups[i].LeanAlpha(alpha, duration);
                        else
                            highlightGroups[i].alpha = alpha;
                    }
                }
            }
        }

        public void AddNodeToController(TargetNode node)
        {
            if (targetController == null)
                SetupTargetController();

            if (targetController == null)
                return;

            if (!targetController.targetTriggerNodes.Contains(node))
                targetController.targetTriggerNodes.Add(node);
        }

        public void RemoveNodeFromController(TargetNode node)
        {
            if (targetController == null)
                SetupTargetController();

            if (targetController == null)
                return;

            if (targetController.targetTriggerNodes.Contains(node))
                targetController.targetTriggerNodes.Remove(node);
        }

        public void SetHitboxRadius(float radius)
        {
            if (capsuleCollider != null && radius > 0f)
                capsuleCollider.radius = radius;
            if (sphereCollider != null && radius > 0f)
                sphereCollider.radius = radius;

            hitboxRadius = radius;
        }

        public void ResetHitboxRadius()
        {
            SetHitboxRadius(originalHitboxRadius);
        }
    }
}