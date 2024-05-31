using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class CharacterState : MonoBehaviour
{
    public enum Role { healer, tank, dps, unassigned }

    [Header("Status")]
    public string characterName = "Unknown";
    public int health = 16000;
    private int maxHealth;
    public float currentDamageReduction = 1f;
    public Dictionary<string,float> damageReduction = new Dictionary<string, float>();
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
    public bool healthBarTextInPercentage = false;

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
            if (healthBarTextInPercentage)
            {
                float healthPercentage = ((float)health / (float)maxHealth) * 100f;
                // Set the health bar text with proper formatting
                if (Mathf.Approximately(healthPercentage, 100f))  // Use Mathf.Approximately for floating point comparison
                {
                    healthBarText.text = "100%";
                }
                else
                {
                    healthBarText.text = healthPercentage.ToString("F1") + "%";
                }
            }
            else
            {
                healthBarText.text = health.ToString();
            }
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
        if (!gameObject.activeSelf)
            return;

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
                    RemoveEffect(effectsArray[i], true, effectsArray[i].uniqueTag);
                }
            }
        }

        if (characterNameText != null)
            characterNameText.text = characterName;
        if (characterNameTextParty)
            characterNameTextParty.text = characterName;
    }

    public void SetHealth(int value, bool kill = false)
    {
        if (kill)
        {
            ModifyHealth(0, kill);
        }
        else
        {
            ModifyHealth(-1 * (health - value), kill);
        }
    }

    public void RemoveHealth(float percentage, bool fromMax, bool kill = false)
    {
        if (kill)
        {
            ModifyHealth(0, kill);
        }
        else
        {
            int damage = fromMax ? Mathf.RoundToInt((maxHealth * percentage / 100.0f)) : Mathf.RoundToInt((int)(health * percentage / 100.0f));
            ModifyHealth(damage, kill);
        }
    }

    public void ModifyHealth(int value, bool kill = false)
    {
        if (!gameObject.activeSelf)
            return;

        if (health <= 0)
            return;

        if (kill)
        {
            value = Mathf.RoundToInt(-1 * (float)health);
            Debug.Log($"hp: {value}");
        }

        health += Mathf.RoundToInt(value * currentDamageReduction);

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
            if (healthBarTextInPercentage)
            {
                float healthPercentage = ((float)health / (float)maxHealth) * 100f;
                //Debug.Log("hp: " + healthPercentage);
                // Set the health bar text with proper formatting
                if (Mathf.Approximately(healthPercentage, 100f))  // Use Mathf.Approximately for floating point comparison
                {
                    healthBarText.text = "100%";
                }
                else
                {
                    healthBarText.text = healthPercentage.ToString("F1") + "%";
                }
            }
            else
            {
                healthBarText.text = health.ToString();
            }
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

    public void AddDamageReduction(float value, string identifier)
    {
        if (!damageReduction.ContainsKey(identifier))
        {
            damageReduction.Add(identifier, value);
        }
        else
        {
            return;
        }

        RecalculateCurrentDamageReduction();
    }

    public void RemoveDamageReduction(string identifier)
    {
        if (damageReduction.ContainsKey(identifier))
        {
            damageReduction.Remove(identifier);
        }
        else
        {
            return;
        }

        RecalculateCurrentDamageReduction();
    }

    private void RecalculateCurrentDamageReduction()
    {
        float result = 1f;

        List<float> a = new List<float>(damageReduction.Values.ToArray());

        if (damageReduction.Count > 0)
        {
            a.Sort();
            for (int i = 0; i < damageReduction.Count; i++)
            {
                result *= a[i];
            }
        }

        currentDamageReduction = result;
    }

    public void AddEffect(StatusEffectData data, int tag = 0)
    {
        if (data.statusEffect == null)
            return;

        if (!gameObject.activeSelf)
            return;

        if (dead)
            return;

        string name = data.statusName;

        if (tag > 0)
        {
            name = $"{data.statusName}_{tag}";
        }

        if (effects.ContainsKey(name))
        {
            if (!data.unique)
                effects[name].Refresh();
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
        AddEffect(newStatusEffect.GetComponent<StatusEffect>(), tag);
    }

    public void AddEffect(string name, int tag = 0)
    {
        if (!gameObject.activeSelf)
            return;

        if (dead)
            return;

        if (tag > 0)
        {
            name = $"{name}_{tag}";
        }

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
                AddEffect(newStatusEffect.GetComponent<StatusEffect>(), tag);
            }
        }
    }

    public void AddEffect(StatusEffect effect, int tag = 0)
    {
        if (!gameObject.activeSelf)
            return;

        if (dead)
            return;

        string name = effect.data.statusName;

        if (tag > 0)
        {
            name = $"{effect.data.statusName}_{tag}";
        }

        if (effects.ContainsKey(name))
        {
            if (!effect.data.unique)
                effects[name].Refresh();
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
        effect.uniqueTag = tag;
        effects.Add(name, effect);
        effectsArray = effects.Values.ToArray();
        if (effect.data.negative)
            effect.Initialize(statusEffectNegativeIconParent, statusEffectIconParentParty);
        else
            effect.Initialize(statusEffectPositiveIconParent, statusEffectIconParentParty);
        effect.onApplication.Invoke(this);
    }

    public void RemoveEffect(StatusEffectData data, bool expired, int tag = 0)
    {
        RemoveEffect(data.statusName, expired, tag);
    }

    public void RemoveEffect(StatusEffect effect, bool expired, int tag = 0)
    {
        RemoveEffect(effect.data.statusName, expired, tag);
    }

    public void RemoveEffect(string name, bool expired, int tag = 0)
    {
        if (!gameObject.activeSelf)
            return;

        if (tag > 0)
        {
            name = $"{name}_{tag}";
        }
        if (tag < 0)
        {
            for (int i = 0; i < (tag * -1); i++)
            {
                RemoveEffect(name, expired, i + 1);
            }
            return;
        }

        if (!effects.ContainsKey(name))
            return;

        StatusEffect temp = effects[name];

        if (expired)
            effects[name].onExpire.Invoke(this);
        else
            effects[name].onCleanse.Invoke(this);

        effects.Remove(name);
        effectsArray = effects.Values.ToArray();

        temp.Remove();
    }

    public bool HasEffect(string name, int tag = 0)
    {
        if (tag > 0)
        {
            name = $"{name}_{tag}";
        }

        return effects.ContainsKey(name);
    }

    public StatusEffect[] GetEffects()
    {
        return effectsArray;
    }

    public void ShowDamagePopupText(int value)
    {
        if (!gameObject.activeSelf)
            return;

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
        if (!gameObject.activeSelf)
            return;

        if (characterNameTextGroup != null)
        {
            if (!hideNameplate)
            {
                characterNameTextGroup.alpha = 1f;
            }
            else
            {
                characterNameTextGroup.alpha = 0f;
            }
        }
        // No idea why these gotta be reversed lmao
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
