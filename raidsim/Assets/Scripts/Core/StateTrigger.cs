using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.StatusEffects;
using dev.susybaka.raidsim.Targeting;
using dev.susybaka.raidsim.UI;
using dev.susybaka.raidsim.Visuals;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.Core
{
    public class StateTrigger : MonoBehaviour
    {
        public bool state = false;
        public PartyList party;
        public bool autoFindParty = false;
        public GameObject source;
        public GameObject target;
        public string targetName = string.Empty;
        public string targetParent = string.Empty;
        public int targetIndex = 0;
        public bool useStatusEffectTagAsTargetIndex = false;
        public string moveToTargetName = string.Empty;
        public string moveToSourceName = string.Empty;
        public bool localParent = false;
        public bool hiddenActions = false;
        public bool toggleObject = true;
        public bool toggleCharacterState = false;
        public bool toggleCharacterEffect = false;
        public bool toggleShaderEffect = false;
        [ShowIf("toggleShaderEffect")] public float fadeTime = 0.33f;
        public bool log = false;

        private StatusEffect sourceStatusEffect;
        private CharacterState sourceCharacterState;
        private TargetController sourceTargetController;
        private ActionController sourceActionController;
        private SimpleShaderFade sourceShaderFade;
        private Transform moveToSource;

        private CharacterState characterState;
        private CharacterEffect characterEffect;
        private TargetController targetController;
        private ActionController actionController;
        private SimpleShaderFade shaderFade;
        private Transform moveToTarget;

        public void Initialize(CharacterState targetCharacter)
        {
            if (targetCharacter != null)
            {
                if (string.IsNullOrEmpty(targetName))
                {
                    targetName = targetCharacter.characterName;
                }
                else if (string.IsNullOrEmpty(targetParent))
                {
                    targetParent = targetCharacter.characterName;
                }
            }

            Initialize();
        }

        public void InitializeSelf(CharacterState sourceCharacter)
        {
            if (sourceCharacter != null)
            {
                source = sourceCharacter.gameObject;
            }
            Initialize();
        }

        public void Initialize()
        {
            if (autoFindParty && FightTimeline.Instance != null)
            {
                party = FightTimeline.Instance.partyList;
            }
            if (source == null)
            {
                source = gameObject;
            }
            sourceStatusEffect = GetComponent<StatusEffect>();
            sourceCharacterState = Utilities.GetComponentInParents<CharacterState>(source.transform);
            sourceTargetController = Utilities.GetComponentInParents<TargetController>(source.transform);
            sourceActionController = Utilities.GetComponentInParents<ActionController>(source.transform);
            sourceShaderFade = Utilities.GetComponentInParents<SimpleShaderFade>(source.transform);

            if (sourceCharacterState == null)
                sourceCharacterState = source.GetComponent<CharacterState>();
            if (sourceTargetController == null)
                sourceTargetController = source.GetComponent<TargetController>();
            if (sourceActionController == null)
                sourceActionController = source.GetComponent<ActionController>();
            if (sourceShaderFade == null)
                sourceShaderFade = source.GetComponent<SimpleShaderFade>();
            if (sourceShaderFade != null)
                sourceShaderFade.defaultFadeTime = fadeTime;

            if (party == null)
            {
                if (!string.IsNullOrEmpty(targetName) && string.IsNullOrEmpty(targetParent))
                {
                    target = Utilities.FindAnyByName(targetName);
                }
                if (!string.IsNullOrEmpty(targetParent))
                {
                    Transform parent;
                    if (localParent)
                    {
                        parent = transform;
                        for (int i = 0; i < targetIndex; i++)
                        {
                            parent = parent.parent;
                        }
                    }
                    else
                    {
                        parent = Utilities.FindAnyByName(targetParent).transform;
                    }
                    int finalIndex = targetIndex;
                    if (useStatusEffectTagAsTargetIndex && !localParent)
                    {
                        finalIndex = sourceStatusEffect.uniqueTag;
                    }
                    if (!localParent)
                        target = parent.GetChild(finalIndex).gameObject;
                    else
                        target = parent.Find(targetName).gameObject;
                }
            }
            else
            {
                List<CharacterState> members = party.GetActiveMembers();

                if (!string.IsNullOrEmpty(targetName) && string.IsNullOrEmpty(targetParent))
                {
                    for (int i = 0; i < members.Count; i++)
                    {
                        if (members[i].characterName == targetName)
                        {
                            target = members[i].gameObject;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(targetParent))
                {
                    Transform parent = null;

                    for (int i = 0; i < members.Count; i++)
                    {
                        if (members[i].characterName == targetParent)
                        {
                            parent = members[i].transform;
                        }
                    }

                    if (parent == null)
                        return;

                    if (!string.IsNullOrEmpty(targetName))
                    {
                        target = parent.Find(targetName).gameObject;
                    }
                    else
                    {
                        int finalIndex = targetIndex;
                        if (useStatusEffectTagAsTargetIndex)
                        {
                            finalIndex = sourceStatusEffect.uniqueTag;
                        }
                        target = parent.GetChild(finalIndex).gameObject;
                    }
                }
            }

            if (target != null)
            {
                target.TryGetComponent(out characterState);
                target.TryGetComponent(out characterEffect);
                target.TryGetComponent(out targetController);
                target.TryGetComponent(out actionController);
                if (target.TryGetComponent(out shaderFade))
                {
                    shaderFade.defaultFadeTime = fadeTime;
                }
                if (!string.IsNullOrEmpty(moveToTargetName))
                {
                    moveToTarget = target.transform.Find(moveToTargetName);
                }
                else
                {
                    moveToTarget = target.transform;
                }
            }

            if (!string.IsNullOrEmpty(moveToSourceName))
            {
                moveToSource = source.transform.Find(moveToSourceName);
            }
            else
            {
                moveToSource = source.transform;
            }
        }

        public void ToggleTarget(bool state)
        {
            if (target != null && toggleObject)
            {
                target.SetActive(state);
            }
            if (characterState != null && toggleCharacterState)
            {
                characterState.ToggleState(state);
            }
            if (characterEffect != null && toggleCharacterEffect)
            {
                if (state)
                    characterEffect.EnableEffect();
                else
                    characterEffect.DisableEffect();
            }
            if (shaderFade != null && toggleShaderEffect)
            {
                if (state)
                    shaderFade.FadeIn(fadeTime);
                else
                    shaderFade.FadeOut(fadeTime);
            }
            this.state = state;
        }

        public void ToggleTarget()
        {
            if (target != null)
            {
                ToggleTarget(!state);
            }
        }

        public void MoveTargetToSource()
        {
            if (target != null && source != null)
            {
                target.transform.position = moveToSource.position;
            }
        }

        public void MoveSourceToTarget()
        {
            if (source != null && target != null)
            {
                source.transform.position = moveToTarget.position;
            }
        }

        public void FaceTargetToSource()
        {
            if (target != null && source != null)
            {
                if (target.TryGetComponent(out BossController bc))
                {
                    bc.SetLookRotation(source.transform);
                }
            }
        }

        public void Target(bool inverted)
        {
            if (sourceTargetController != null && targetController != null)
            {
                if (!inverted)
                {
                    if (log)
                    {
                        Debug.Log($"[StateTrigger] '{targetController.gameObject.name}' targeting '{sourceTargetController.gameObject.name}'");
                    }
                    targetController.SetTarget(sourceTargetController.self);
                }
                else
                {
                    if (log)
                    {
                        Debug.Log($"[StateTrigger] '{sourceTargetController.gameObject.name}' targeting '{targetController.gameObject.name}'");
                    }
                    sourceTargetController.SetTarget(targetController.self);
                }
            }
        }

        public void CastTarget(CharacterActionData data)
        {
            CastInternal(data, false);
        }

        public void CastSource(CharacterActionData data)
        {
            CastInternal(data, true);
        }

        private void CastInternal(CharacterActionData data, bool inverted)
        {
            if (!inverted)
            {
                if (actionController != null)
                {
                    if (!hiddenActions)
                        actionController.PerformAction(data.actionName);
                    else
                        actionController.PerformActionHidden(data.actionName);
                }
            }
            else
            {
                if (sourceActionController != null)
                {
                    if (!hiddenActions)
                        sourceActionController.PerformAction(data.actionName);
                    else
                        sourceActionController.PerformActionHidden(data.actionName);
                }
            }
        }
    }
}