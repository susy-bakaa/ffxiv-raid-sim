using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StatusEffect : MonoBehaviour
{
    [Header("Base")]
    public StatusEffectData data;
    public float duration;
    public int stacks;
    public int uniqueTag;

    [Header("Events")]
    public UnityEvent<CharacterState> onApplication;
    public UnityEvent<CharacterState> onTick;
    public UnityEvent<CharacterState> onUpdate;
    public UnityEvent<CharacterState> onExpire;
    public UnityEvent<CharacterState> onCleanse;

    private bool hasHudElement = false;
    private GameObject hudElement;
    private TextMeshProUGUI hudTimer;
    private bool hasPartyHudElement = false;
    private GameObject partyHudElement;
    private TextMeshProUGUI partyHudTimer;

    public void Initialize(Transform hudElementParent, Transform partyHudElementParent, int tag = 0)
    {
        onApplication.AddListener(OnApplication);
        onTick.AddListener(OnTick);
        onUpdate.AddListener(OnUpdate);
        onExpire.AddListener(OnExpire);
        onCleanse.AddListener(OnCleanse);

        if (hudElementParent != null)
        {
            hasHudElement = true;
            hudElement = Instantiate(data.hudElement, hudElementParent);
            hudTimer = hudElement.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
        }
        if (partyHudElementParent != null)
        {
            hasPartyHudElement = true;
            partyHudElement = Instantiate(data.hudElement, partyHudElementParent);
            partyHudTimer = partyHudElement.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
        }

        duration = data.length;
        stacks += data.appliedStacks;
        uniqueTag = tag;

        if (stacks > data.maxStacks)
            stacks = data.maxStacks;

        if (hasHudElement)
        {
            if (!data.infinite && duration >= 1)
                hudTimer.text = duration.ToString("F0");
            else
                hudTimer.text = "";
        }
        if (hasPartyHudElement && duration >= 1)
        {
            if (!data.infinite)
                partyHudTimer.text = duration.ToString("F0");
            else
                partyHudTimer.text = "";
        }
    }

    public void Refresh(bool ignoreStacks = false, int tag = 0)
    {
        uniqueTag = tag;
        duration += data.length;
        if (!ignoreStacks)
            stacks += data.appliedStacks;

        if (duration > data.maxLength)
            duration = data.maxLength;
        if (stacks > data.maxStacks)
            stacks = data.maxStacks;
    }

    public void Remove()
    {
        if (hasHudElement)
            Destroy(hudElement, 0.1f);
        if (hasPartyHudElement)
            Destroy(partyHudElement, 0.1f);
        Destroy(gameObject, 0.1f);
    }

    public virtual void OnApplication(CharacterState state)
    {

    }

    public virtual void OnTick(CharacterState state)
    {
        if (data.infinite)
            Refresh(true);
    }

    public virtual void OnUpdate(CharacterState state)
    {
        if (hasHudElement)
        {
            if (!data.infinite && duration >= 1)
                hudTimer.text = duration.ToString("F0");
            else
                hudTimer.text = "";
        }
        if (hasPartyHudElement)
        {
            if (!data.infinite && duration >= 1)
                partyHudTimer.text = duration.ToString("F0");
            else
                partyHudTimer.text = "";
        }
    }

    public virtual void OnExpire(CharacterState state)
    {
        if (hasHudElement)
            hudTimer.text = "";
        if (hasPartyHudElement)
            partyHudTimer.text = "";
    }

    public virtual void OnCleanse(CharacterState state)
    {
        if (hasHudElement)
            hudTimer.text = "";
        if (hasPartyHudElement)
            partyHudTimer.text = "";
    }

    void OnDestroy()
    {
        onApplication.RemoveListener(OnApplication);
        onTick.RemoveListener(OnTick);
        onUpdate.RemoveListener(OnUpdate);
        onExpire.RemoveListener(OnExpire);
        onCleanse.RemoveListener(OnCleanse);
    }
}
