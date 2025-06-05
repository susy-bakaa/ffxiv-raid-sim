using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static GlobalData;

public class StatusEffect : MonoBehaviour
{
    CharacterState character;

    [Header("Base")]
    public StatusEffectData data;
    public float duration;
    public int stacks;
    public int uniqueTag;
    public int sortOrder = 0;
    public Damage damage;

    [Header("Events")]
    public UnityEvent<CharacterState> onApplication;
    public UnityEvent<CharacterState> onTick;
    public UnityEvent<CharacterState> onUpdate;
    public UnityEvent<CharacterState> onExpire;
    public UnityEvent<CharacterState> onCleanse;
    public UnityEvent<CharacterState> onReduce;
    public UnityEvent<CharacterState> onAddStack;
    public UnityEvent<CharacterState> onMaxStacks;

    private bool hasHudElement = false;
    private CanvasGroup hudElementGroup;
    private GameObject hudElement;
    private TextMeshProUGUI hudTimer;
    private Image hudIcon;
    private bool hasPartyHudElement = false;
    private CanvasGroup partyHudElementGroup;
    private GameObject partyHudElement;
    private TextMeshProUGUI partyHudTimer;
    private Image partyHudIcon;
    private bool hasTargetHudElement = false;
    private CanvasGroup targetHudElementGroup;
    private GameObject targetHudElement;
    private TextMeshProUGUI targetHudTimer;
    private Image targetHudIcon;
    private StatusEffectIcon icon;

    public void Initialize(CharacterState holder, Transform hudElementParent, Transform partyHudElementParent, Transform targetHudElementParent, Color labelColor)
    {
        character = holder;
        onApplication.AddListener(OnApplication);
        onTick.AddListener(OnTick);
        onUpdate.AddListener(OnUpdate);
        onExpire.AddListener(OnExpire);
        onCleanse.AddListener(OnCleanse);
        onReduce.AddListener(OnReduce);

        if (hudElementParent != null)
        {
            hasHudElement = true;
            hudElement = Instantiate(data.hudElement, hudElementParent);
            hudElementGroup = hudElement.GetComponent<CanvasGroup>();
            if (hudElement.TryGetComponent(out icon))
            {
                icon.Initialize(character, this);
            }
            if (hudElementGroup != null)
            {
                if (data.hidden)
                {
                    hudElementGroup.alpha = 0f;
                    hudElementGroup.blocksRaycasts = true;
                }
                else
                {
                    hudElementGroup.alpha = 1f;
                    hudElementGroup.blocksRaycasts = true;
                }
            }
            hudTimer = hudElement.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
            hudIcon = hudElement.transform.GetChild(0).GetComponent<Image>();
            hudTimer.color = labelColor;
        }
        if (partyHudElementParent != null)
        {
            hasPartyHudElement = true;
            partyHudElement = Instantiate(data.hudElement, partyHudElementParent);
            partyHudElementGroup = partyHudElement.GetComponent<CanvasGroup>();
            if (partyHudElementGroup != null)
            {
                if (data.hidden)
                {
                    partyHudElementGroup.alpha = 0f;
                    partyHudElementGroup.blocksRaycasts = true;
                }
                else
                {
                    partyHudElementGroup.alpha = 1f;
                    partyHudElementGroup.blocksRaycasts = true;
                }
            }
            partyHudTimer = partyHudElement.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
            partyHudIcon = partyHudElement.transform.GetChild(0).GetComponent<Image>();
            partyHudTimer.color = labelColor;
        }
        if (targetHudElementParent != null)
        {
            hasTargetHudElement = true;
            targetHudElement = Instantiate(data.hudElement, targetHudElementParent);
            targetHudElementGroup = targetHudElement.GetComponent<CanvasGroup>();
            if (targetHudElementGroup != null)
            {
                if (data.hidden)
                {
                    targetHudElementGroup.alpha = 0f;
                    targetHudElementGroup.blocksRaycasts = true;
                }
                else
                {
                    targetHudElementGroup.alpha = 1f;
                    targetHudElementGroup.blocksRaycasts = true;
                }
            }
            targetHudTimer = targetHudElement.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
            targetHudIcon = targetHudElement.transform.GetChild(0).GetComponent<Image>();
            targetHudTimer.color = labelColor;
        }

        duration = data.length;
        stacks += data.appliedStacks;

        if (stacks > data.maxStacks)
            stacks = data.maxStacks;

        if (hasHudElement)
        {
            if ((!data.infinite && !data.hidden) && duration >= 0.5)
            {
                hudTimer.text = Utilities.FormatDuration(duration);
            }
            else
                hudTimer.text = "";
            if (data.icons.Count > 0)
            {
                hudIcon.sprite = data.icons[stacks - 1];
            }
        }
        if (hasPartyHudElement)
        {
            if ((!data.infinite && !data.hidden) && duration >= 0.5)
            {
                partyHudTimer.text = Utilities.FormatDuration(duration);
            }
            else
                partyHudTimer.text = "";
            if (data.icons.Count > 0)
            {
                partyHudIcon.sprite = data.icons[stacks - 1];
            }
        }
        if (hasTargetHudElement)
        {
            if ((!data.infinite && !data.hidden) && duration >= 0.5)
            {
                if ((!data.infinite && !data.hidden) && duration >= 1)
                {
                    targetHudTimer.text = Utilities.FormatDuration(duration);
                }
                if (data.icons.Count > 0)
                {
                    targetHudIcon.sprite = data.icons[stacks - 1];
                }
            }
            else
                targetHudTimer.text = "";
            if (data.icons.Count > 0)
            {
                targetHudIcon.sprite = data.icons[stacks - 1];
            }
        }
    }

    public void AddStack(int appliedStacks)
    {
        stacks += appliedStacks;

        if (character != null)
        {
            string prefix = " + ";
            if (appliedStacks < 0)
                prefix = " - ";

            character.ShowStatusEffectFlyTextWorldspace(data, stacks, prefix);
            if (character.showStatusPopups)
                character.ShowStatusEffectFlyText(data, stacks, prefix);
        }

        if (stacks > data.maxStacks)
            stacks = data.maxStacks;

        if (data.icons.Count > 0)
        {
            hudIcon.sprite = data.icons[stacks - 1];
        }

        Debug.Log($"AddStack {character}");
        onAddStack.Invoke(character);
    }

    public void Refresh(int appliedStacks = 0, int tag = 0, float duration = 0)
    {
        if (duration == 0)
        {
            this.duration += data.length;
            if (this.duration > data.maxLength)
                this.duration = data.maxLength;
        }
        else if (duration <= -1)
        {
            if (this.duration > data.maxLength)
                this.duration = data.maxLength;
        }
        else
        {
            this.duration = duration;
        }

        uniqueTag = tag;
        if (appliedStacks == 0)
            stacks += data.appliedStacks;
        else if (appliedStacks > 0)
            stacks += stacks;

        if (character != null && data.maxStacks > 1)
        {
            string prefix = " + ";
            if (appliedStacks < 0)
                prefix = " - ";

            character.ShowStatusEffectFlyTextWorldspace(data, stacks, prefix);
            if (character.showDamagePopups)
                character.ShowStatusEffectFlyText(data, stacks, prefix);
        }

        if (stacks > data.maxStacks)
            stacks = data.maxStacks;

        if (data.icons.Count > 0)
        {
            hudIcon.sprite = data.icons[stacks - 1];
        }
    }

    public void Remove()
    {
        if (hasHudElement)
            Destroy(hudElement, 0.1f);
        if (hasPartyHudElement)
            Destroy(partyHudElement, 0.1f);
        if (hasTargetHudElement)
            Destroy(targetHudElement, 0.1f);
        Destroy(gameObject, 0.1f);
    }

    public virtual void OnApplication(CharacterState state)
    {

    }

    public virtual void OnTick(CharacterState state)
    {
        // I think this is being used incorrectly/broken, but infinite effects work as intended so don't touch it I guess?
        if (data.infinite)
            Refresh(0);

        if (stacks >= data.maxStacks)
        {
            onMaxStacks.Invoke(state);
        }

        //Debug.Log($"{gameObject.name} onTick");
    }

    public virtual void OnUpdate(CharacterState state)
    {
        if (hasHudElement)
        {
            if ((!data.infinite && !data.hidden) && duration >= 1)
            {
                hudTimer.text = Utilities.FormatDuration(duration);
            }
            else
                hudTimer.text = "";
            if (data.icons.Count > 0)
            {
                hudIcon.sprite = data.icons[stacks - 1];
            }
        }
        if (hasPartyHudElement)
        {
            if ((!data.infinite && !data.hidden) && duration >= 1)
            {
                partyHudTimer.text = Utilities.FormatDuration(duration);
            }
            else
                partyHudTimer.text = "";
            if (data.icons.Count > 0)
            {
                partyHudIcon.sprite = data.icons[stacks - 1];
            }
        }
        if (hasTargetHudElement)
        {
            if ((!data.infinite && !data.hidden) && duration >= 1)
            {
                targetHudTimer.text = Utilities.FormatDuration(duration);
            }
            else
                targetHudTimer.text = "";
            if (data.icons.Count > 0)
            {
                targetHudIcon.sprite = data.icons[stacks - 1];
            }
        }
    }

    public virtual void OnExpire(CharacterState state)
    {
        if (hasHudElement)
            hudTimer.text = "";
        if (hasPartyHudElement)
            partyHudTimer.text = "";
        if (hasTargetHudElement)
            targetHudTimer.text = "";
    }

    public virtual void OnCleanse(CharacterState state)
    {
        if (hasHudElement)
            hudTimer.text = "";
        if (hasPartyHudElement)
            partyHudTimer.text = "";
        if (hasTargetHudElement)
            targetHudTimer.text = "";
    }

    public virtual void OnReduce(CharacterState state)
    {

    }

    void OnDestroy()
    {
        onApplication.RemoveListener(OnApplication);
        onTick.RemoveListener(OnTick);
        onUpdate.RemoveListener(OnUpdate);
        onExpire.RemoveListener(OnExpire);
        onCleanse.RemoveListener(OnCleanse);
        onReduce.RemoveListener(OnReduce);
    }
}
