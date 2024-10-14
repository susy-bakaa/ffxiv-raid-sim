using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static GlobalStructs;

public class StatusEffect : MonoBehaviour
{
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

    public void Initialize(Transform hudElementParent, Transform partyHudElementParent, Transform targetHudElementParent, Color labelColor, int tag = 0)
    {
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
            if (partyHudElementGroup != null)
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
        uniqueTag = tag;

        if (stacks > data.maxStacks)
            stacks = data.maxStacks;

        if (hasHudElement)
        {
            if ((!data.infinite || data.hidden) && duration >= 1)
                hudTimer.text = duration.ToString("F0");
            else
                hudTimer.text = "";
            if (data.icons.Count > 0)
            {
                hudIcon.sprite = data.icons[stacks - 1];
            }
        }
        if (hasPartyHudElement)
        {
            if ((!data.infinite || data.hidden) && duration >= 1)
                partyHudTimer.text = duration.ToString("F0");
            else
                partyHudTimer.text = "";
            if (data.icons.Count > 0)
            {
                partyHudIcon.sprite = data.icons[stacks - 1];
            }
        }
        if (hasTargetHudElement)
        {
            if ((!data.infinite || data.hidden) && duration >= 1)
                targetHudTimer.text = duration.ToString("F0");
            else
                targetHudTimer.text = "";
            if (data.icons.Count > 0)
            {
                targetHudIcon.sprite = data.icons[stacks - 1];
            }
        }
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
            Refresh(-1);
    }

    public virtual void OnUpdate(CharacterState state)
    {
        if (hasHudElement)
        {
            if ((!data.infinite || data.hidden) && duration >= 1)
                hudTimer.text = duration.ToString("F0");
            else
                hudTimer.text = "";
            if (data.icons.Count > 0)
            {
                hudIcon.sprite = data.icons[stacks - 1];
            }
        }
        if (hasPartyHudElement)
        {
            if ((!data.infinite || data.hidden) && duration >= 1)
                partyHudTimer.text = duration.ToString("F0");
            else
                partyHudTimer.text = "";
            if (data.icons.Count > 0)
            {
                partyHudIcon.sprite = data.icons[stacks - 1];
            }
        }
        if (hasTargetHudElement)
        {
            if ((!data.infinite || data.hidden) && duration >= 1)
                targetHudTimer.text = duration.ToString("F0");
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
