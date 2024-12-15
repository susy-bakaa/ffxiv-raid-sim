using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StateTrigger : MonoBehaviour
{
    public bool state = false;
    public GameObject source;
    public GameObject target;
    public string targetName = string.Empty;
    public string targetParent = string.Empty;
    public int targetIndex = 0;
    public bool useStatusEffectTagAsTargetIndex = false;
    public bool localParent = false;
    public bool toggleObject = true;
    public bool toggleCharacterState = false;
    public bool toggleCharacterEffect = false;
    public bool log = false;

    private StatusEffect sourceStatusEffect;
    private CharacterState sourceCharacterState;
    private TargetController sourceTargetController;
    private ActionController sourceActionController;

    private CharacterState characterState;
    private CharacterEffect characterEffect;
    private TargetController targetController;
    private ActionController actionController;

    public void Initialize()
    {
        if (source == null)
        {
            source = gameObject;
        }
        sourceStatusEffect = GetComponent<StatusEffect>();
        sourceCharacterState = Utilities.GetComponentInParents<CharacterState>(source.transform);
        sourceTargetController = Utilities.GetComponentInParents<TargetController>(source.transform);
        sourceActionController = Utilities.GetComponentInParents<ActionController>(source.transform);

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

        if (target != null)
        {
            target.TryGetComponent(out characterState);
        }
        if (target != null)
        {
            target.TryGetComponent(out characterEffect);
        }
        if (target != null)
        {
            target.TryGetComponent(out targetController);
        }
        if (target != null)
        {
            target.TryGetComponent(out actionController);
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
            target.transform.position = source.transform.position;
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
                actionController.PerformAction(data.actionName);
            }
        }
        else
        {
            if (sourceActionController != null)
            {
                sourceActionController.PerformAction(data.actionName);
            }
        }
    }
}