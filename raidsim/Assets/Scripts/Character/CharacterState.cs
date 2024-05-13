using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterState : MonoBehaviour
{
    public enum Role { healer, tank, dps, unassigned }

    [Header("Status")]
    public string characterName = "Unknown";
    public int health = 16000;
    private int maxHealth;
    public float damageReduction = 1f;
    //private float maxDamageReduction = 100f;
    public float speed = 6.3f;
    public float defaultSpeed { private set; get; }
    //private float maxSpeed = 15f;

    [Header("States")]
    public bool dead = false;
    public bool uncontrollable = false;
    public bool untargetable = false;
    public bool bound = false;
    public bool stunned = false;
    public bool knockbackResistant = false;
    public bool still = false;
    public bool silenced = false;
    public bool pacificied = false;
    public bool amnesia = false;
    public bool canDoActions = true;

    [Header("Effects")]
    private Dictionary<string, StatusEffect> effects = new Dictionary<string, StatusEffect>();
    private StatusEffect[] effectsArray = null;

    [Header("Events")]
    public UnityEvent onDeath;

    [Header("Config")]
    public Transform statusEffectParent;
    public float statusEffectUpdateInterval = 3f;
    private float statusEffectUpdateTimer = 0f;
    public Role role = Role.dps;
    public bool hideNameplate = false;
    public bool hidePartyName = false;

    [Header("Personal")]
    public bool showCharacterName = true;
    public TextMeshProUGUI characterNameText;
    private CanvasGroup characterNameTextGroup;
    public bool showStatusEffects = true;
    public Transform statusEffectPositiveIconParent;
    public Transform statusEffectNegativeIconParent;

    public bool showDamagePopup = true;
    public TextMeshProUGUI damagePopup;

    public bool showHealthBar = true;
    public Slider healthBar;
    public TextMeshProUGUI healthBarText;

    [Header("Party")]
    public bool showPartyCharacterName = true;
    public TextMeshProUGUI characterNameTextParty;
    private CanvasGroup characterNameTextGroupParty;
    public bool showPartyListStatusEffects = true;
    public Transform statusEffectIconParentParty;

    public bool showPartyHealthBar = true;
    public Slider healthBarParty;
    public TextMeshProUGUI healthBarTextParty;

    void Awake()
    {
        if (statusEffectNegativeIconParent != null)
        {
            foreach (Transform child in statusEffectNegativeIconParent)
            {
                Destroy(child.gameObject);
            }
        }
        if (statusEffectPositiveIconParent != null)
        {
            foreach (Transform child in statusEffectPositiveIconParent)
            {
                Destroy(child.gameObject);
            }
        }
        if (statusEffectIconParentParty != null)
        {
            foreach (Transform child in statusEffectIconParentParty)
            {
                Destroy(child.gameObject);
            }
        }

        maxHealth = health;
        defaultSpeed = speed;
        dead = false;
        uncontrollable = false;
        untargetable = false;
        bound = false;
        knockbackResistant = false;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }
        if (healthBarText != null)
        {
            healthBarText.text = health.ToString();
        }
        if (healthBarParty != null)
        {
            healthBarParty.maxValue = maxHealth;
            healthBarParty.value = health;
        }
        if (healthBarTextParty != null)
        {
            healthBarTextParty.text = health.ToString();
        }
        if (characterNameText != null)
        {
            characterNameText.text = characterName;
            characterNameTextGroup = characterNameText.GetComponentInParent<CanvasGroup>();
        }
        if (characterNameTextGroup != null)
        {
            if (!hideNameplate)
            {
                characterNameTextGroup.alpha = 1f;
            }
            else
            {
                characterNameTextGroup.LeanAlpha(0f, 0.5f);
            }
        }
        if (characterNameTextParty != null)
        {
            characterNameTextParty.text = characterName;
            characterNameTextGroupParty = characterNameTextParty.GetComponentInParent<CanvasGroup>();
        }
        if (characterNameTextGroupParty != null)
        {
            if (!hidePartyName)
            {
                characterNameTextGroupParty.alpha = 1f;
            }
            else
            {
                characterNameTextGroupParty.LeanAlpha(0f, 0.5f);
            }
        }
    }

    void Update()
    {
        statusEffectUpdateTimer += Time.deltaTime;

        if (effects.Count > 0 && effectsArray != null)
        {
            // Simulate FFXIV server ticks by updating status effects every 3 seconds.
            if (statusEffectUpdateTimer >= statusEffectUpdateInterval)
            {
                statusEffectUpdateTimer = 0f;
                for (int i = 0; i < effectsArray.Length; i++)
                {
                    effectsArray[i].onTick.Invoke(this);
                }
            }
            // Update the status effect durations every frame.
            for (int i = 0; i < effectsArray.Length; i++)
            {
                effectsArray[i].duration -= Time.deltaTime;
                effectsArray[i].onUpdate.Invoke(this);
                if (effectsArray[i].duration <= 0f)
                {
                    RemoveEffect(effectsArray[i], true);
                }
            }
        }
    }

    public void ModifyHealth(int value, bool kill = false)
    {
        if (health <= 0)
            return;

        if (kill)
        {
            value = health;
        }

        health += Mathf.RoundToInt(value * damageReduction);

        if (health <= 0)
        {
            health = 0;
            dead = true;
            onDeath.Invoke();
            if (effectsArray != null)
            {
                for (int i = 0; i < effectsArray.Length; i++)
                {
                    effectsArray[i].onExpire.Invoke(this);
                    effectsArray[i].Remove();
                }
                effects = new Dictionary<string, StatusEffect>();
                effectsArray = null;
            }
        }
        if (health > maxHealth)
        {
            health = maxHealth;
        }

        // USER INTERFACE
        if (healthBar != null)
        {
            healthBar.value = health;
        }
        if (healthBarText != null)
        {
            healthBarText.text = health.ToString();
        }
        if (healthBarParty != null)
        {
            healthBarParty.value = health;
        }
        if (healthBarTextParty != null)
        {
            healthBarTextParty.text = health.ToString();
        }
    }

    public void AddEffect(StatusEffectData data)
    {
        if (dead)
            return;
        if (effects.ContainsKey(data.statusName))
        {
            if (!data.unique)
                effects[data.statusName].Refresh();
            return;
        }
        if (effectsArray != null)
        {
            for (int i = 0; i < effectsArray.Length; i++)
            {
                if (data.incompatableStatusEffects.Contains(effectsArray[i].data))
                {
                    return;
                }
            }
        }
        GameObject newStatusEffect = Instantiate(data.statusEffect, statusEffectParent);
        AddEffect(newStatusEffect.GetComponent<StatusEffect>());
    }

    public void AddEffect(string name)
    {
        if (dead)
            return;
        if (effects.ContainsKey(name))
        {
            if (!effects[name].data.unique)
                effects[name].Refresh();
            return;
        }
        for (int i = 0; i < FightTimeline.Instance.allAvailableStatusEffects.Count; i++)
        {
            if (FightTimeline.Instance.allAvailableStatusEffects[i].name == name)
            {
                if (effectsArray != null)
                {
                    for (int k = 0; k < effectsArray.Length; k++)
                    {
                        if (FightTimeline.Instance.allAvailableStatusEffects[i].incompatableStatusEffects.Contains(effectsArray[k].data))
                        {
                            return;
                        }
                    }
                }
                GameObject newStatusEffect = Instantiate(FightTimeline.Instance.allAvailableStatusEffects[i].statusEffect, statusEffectParent);
                AddEffect(newStatusEffect.GetComponent<StatusEffect>());
            }
        }
    }

    public void AddEffect(StatusEffect effect)
    {
        if (dead)
            return;
        if (effects.ContainsKey(effect.data.statusName))
        {
            if (!effect.data.unique)
                effects[effect.data.statusName].Refresh();
            return;
        }
        if (effectsArray != null)
        {
            for (int i = 0; i < effectsArray.Length; i++)
            {
                if (effect.data.incompatableStatusEffects.Contains(effectsArray[i].data))
                {
                    return;
                }
            }
        }
        effects.Add(effect.data.statusName, effect);
        effectsArray = effects.Values.ToArray();
        if (effect.data.negative)
            effect.Initialize(statusEffectNegativeIconParent, statusEffectIconParentParty);
        else
            effect.Initialize(statusEffectPositiveIconParent, statusEffectIconParentParty);
        effect.onApplication.Invoke(this);
    }

    public void RemoveEffect(StatusEffectData data, bool expired)
    {
        RemoveEffect(data.statusName, expired);
    }

    public void RemoveEffect(StatusEffect effect, bool expired)
    {
        RemoveEffect(effect.data.statusName, expired);
    }

    public void RemoveEffect(string name, bool expired)
    {
        StatusEffect temp = effects[name];

        if (expired)
            effects[name].onExpire.Invoke(this);
        else
            effects[name].onCleanse.Invoke(this);

        effects.Remove(name);
        effectsArray = effects.Values.ToArray();

        temp.Remove();
    }

    public bool HasEffect(string name)
    {
        return effects.ContainsKey(name);
    }

    public StatusEffect[] GetEffects()
    {
        return effectsArray;
    }

    public void ShowDamagePopupText(int value)
    {
        if (damagePopup == null)
            return;

        if (value >= 0)
        {
            damagePopup.text = $"+{value}";
            damagePopup.color = Color.green;
        }
        else
        {
            damagePopup.text = $"-{value}";
            damagePopup.color = Color.red;
        }
    }

    public void UpdateCharacterName()
    {
        if (characterNameTextGroup != null)
        {
            if (!hideNameplate)
            {
                characterNameTextGroup.alpha = 0f;
            }
            else
            {
                characterNameTextGroup.alpha = 1f;
            }
        }
        if (characterNameTextGroupParty != null)
        {
            if (!hidePartyName)
            {
                characterNameTextGroupParty.alpha = 0f;
            }
            else
            {
                characterNameTextGroupParty.alpha = 1f;
            }
        }
    }

    private IEnumerator HideDamagePopupText(float delay)
    {
        yield return new WaitForSeconds(delay);
        damagePopup.text = string.Empty;
        damagePopup.color = Color.white;
    }
}
