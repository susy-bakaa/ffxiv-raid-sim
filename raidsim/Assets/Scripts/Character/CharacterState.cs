using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static GlobalStructs;
using static GlobalStructs.Damage;
using static Unity.VisualScripting.Member;
using static UnityEngine.Rendering.DebugUI;

public class CharacterState : MonoBehaviour
{
    public enum Role { healer, tank, dps, unassigned }

    [Header("Status")]
    public string characterName = "Unknown";

    private int defaultMaxHealth;
    [HideInInspector] public int currentMaxHealth;
    public int health = 16000;
    public Dictionary<string, float> maxHealthModifiers = new Dictionary<string, float>();

    public float defaultSpeed { private set; get; }
    public float currentSpeed = 6.3f;
    public Dictionary<string, float> speedModifiers = new Dictionary<string, float>();
    public Dictionary<string, float> speed = new Dictionary<string, float>();
    //private float maxSpeed = 15f;

    public float currentDamageReduction = 1f;
    public Dictionary<string,float> damageReduction = new Dictionary<string, float>();

    public Dictionary<string, float> magicalTypeDamageModifiers = new Dictionary<string, float>();
    public float magicalTypeDamageModifier = 1f;
    public Dictionary<string, float> physicalTypeDamageModifiers = new Dictionary<string, float>();
    public float physicalTypeDamageModifier = 1f;
    public Dictionary<string, float> uniqueTypeDamageModifiers = new Dictionary<string, float>();
    public float uniqueTypeDamageModifier = 1f;
    public Dictionary<string, float> unaspectedElementDamageModifiers = new Dictionary<string, float>();
    public float unaspectedElementDamageModifier = 1f;
    public Dictionary<string, float> fireElementDamageModifiers = new Dictionary<string, float>();
    public float fireElementDamageModifier = 1f;
    public Dictionary<string, float> iceElementDamageModifiers = new Dictionary<string, float>();
    public float iceElementDamageModifier = 1f;
    public Dictionary<string, float> lightningElementDamageModifiers = new Dictionary<string, float>();
    public float lightningElementDamageModifier = 1f;
    public Dictionary<string, float> waterElementDamageModifiers = new Dictionary<string, float>();
    public float waterElementDamageModifier = 1f;
    public Dictionary<string, float> windElementDamageModifiers = new Dictionary<string, float>();
    public float windElementDamageModifier = 1f;
    public Dictionary<string, float> earthElementDamageModifiers = new Dictionary<string, float>();
    public float earthElementDamageModifier = 1f;
    public Dictionary<string, float> darkElementDamageModifiers = new Dictionary<string, float>();
    public float darkElementDamageModifier = 1f;
    public Dictionary<string, float> lightElementDamageModifiers = new Dictionary<string, float>();
    public float lightElementDamageModifier = 1f;
    public Dictionary<string, float> slashingElementDamageModifiers = new Dictionary<string, float>();
    public float slashingElementDamageModifier = 1f;
    public Dictionary<string, float> piercingElementDamageModifiers = new Dictionary<string, float>();
    public float piercingElementDamageModifier = 1f;
    public Dictionary<string, float> bluntElementDamageModifiers = new Dictionary<string, float>();
    public float bluntElementDamageModifier = 1f;

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

    [Header("Personal - Name")]
    public bool showCharacterName = true;
    public TextMeshProUGUI characterNameText;
    private CanvasGroup characterNameTextGroup;
    [Header("Personal - Status Effects")]
    public bool showStatusEffects = true;
    public Transform statusEffectPositiveIconParent;
    public Transform statusEffectNegativeIconParent;
    public Color ownStatusEffectColor = Color.green;
    public Color otherStatusEffectColor = Color.white;
    [Header("Personal - Damage Popups")]
    public bool showDamagePopups = true;
    public GameObject damagePopupPrefab;
    public Transform damagePopupParent;
    public Color negativePopupColor = Color.red;
    public Color positivePopupColor = Color.green;
    public float popupTextDelay = 0.5f;
    private Queue<FlyText> popupTexts = new Queue<FlyText>();
    private Coroutine popupCoroutine;
    [Header("Personal - Health Bar")]
    public bool showHealthBar = true;
    public Slider healthBar;
    public TextMeshProUGUI healthBarText;
    public bool healthBarTextInPercentage = false;

    [Header("Party - Name")]
    public bool showPartyCharacterName = true;
    public TextMeshProUGUI characterNameTextParty;
    private CanvasGroup characterNameTextGroupParty;
    [Header("Party - Status Effects")]
    public bool showPartyListStatusEffects = true;
    public Transform statusEffectIconParentParty;
    [Header("Party - Health Bar")]
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

        currentMaxHealth = health;
        defaultMaxHealth = health;
        defaultSpeed = currentSpeed;
        dead = false;
        uncontrollable = false;
        untargetable = false;
        bound = false;
        knockbackResistant = false;

        if (healthBar != null)
        {
            healthBar.maxValue = currentMaxHealth;
            healthBar.value = health;
        }
        if (healthBarText != null)
        {
            if (healthBarTextInPercentage)
            {
                float healthPercentage = ((float)health / (float)currentMaxHealth) * 100f;
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
            healthBarParty.maxValue = currentMaxHealth;
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
            // Simulate FFXIV server ticks by updating status effects every 3 seconds (By default).
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
            int damage = fromMax ? Mathf.RoundToInt((currentMaxHealth * percentage)) : Mathf.RoundToInt((health * percentage)); // / 100.0f
            //Debug.Log($"damage {damage}");
            ModifyHealth(damage, kill);
        }
    }

    public void ModifyHealth(Damage damage, bool kill = false)
    {
        Damage m_damage = new Damage(damage);

        //Debug.Log($"m_damage {m_damage} | m_damage.value {m_damage.value} | m_damage.negative {m_damage.negative}");

        if (m_damage.negative)
        {
            switch (damage.type)
            {
                case DamageType.magical:
                {
                    m_damage.value = Mathf.RoundToInt(m_damage.value * magicalTypeDamageModifier);
                    if (magicalTypeDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    break;
                }
                case DamageType.physical:
                {
                    m_damage.value = Mathf.RoundToInt(m_damage.value * physicalTypeDamageModifier);
                    if (physicalTypeDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    break;
                }
                case DamageType.unique:
                {
                    m_damage.value = Mathf.RoundToInt(m_damage.value * uniqueTypeDamageModifier);
                    if (uniqueTypeDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    break;
                }
            }
            switch (damage.elementalAspect)
            {
                case ElementalAspect.unaspected:
                {
                    m_damage.value = Mathf.RoundToInt(m_damage.value * unaspectedElementDamageModifier);
                    if (unaspectedElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    break;
                }
                case ElementalAspect.fire:
                {
                    m_damage.value = Mathf.RoundToInt(m_damage.value * fireElementDamageModifier);
                    if (fireElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    break;
                }
                case ElementalAspect.ice:
                {
                    m_damage.value = Mathf.RoundToInt(m_damage.value * iceElementDamageModifier);
                    if (iceElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    break;
                }
                case ElementalAspect.lightning:
                {
                    m_damage.value = Mathf.RoundToInt(m_damage.value * lightningElementDamageModifier);
                    if (lightningElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    break;
                }
                case ElementalAspect.water:
                {
                    m_damage.value = Mathf.RoundToInt(m_damage.value * waterElementDamageModifier);
                    if (waterElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    break;
                }
                case ElementalAspect.wind:
                {
                    m_damage.value = Mathf.RoundToInt(m_damage.value * windElementDamageModifier);
                    if (windElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    break;
                }
                case ElementalAspect.earth:
                {
                    m_damage.value = Mathf.RoundToInt(m_damage.value * earthElementDamageModifier);
                    if (earthElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    break;
                }
                case ElementalAspect.dark:
                {
                    m_damage.value = Mathf.RoundToInt(m_damage.value * darkElementDamageModifier);
                    if (darkElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    break;
                }
                case ElementalAspect.light:
                {
                    m_damage.value = Mathf.RoundToInt(m_damage.value * lightElementDamageModifier);
                    if (lightElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    break;
                }
            }
            switch (damage.physicalAspect)
            {
                case PhysicalAspect.slashing:
                {
                    m_damage.value = Mathf.RoundToInt(m_damage.value * slashingElementDamageModifier);
                    if (slashingElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    break;
                }
                case PhysicalAspect.piercing:
                {
                    m_damage.value = Mathf.RoundToInt(m_damage.value * piercingElementDamageModifier);
                    if (piercingElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    break;
                }
                case PhysicalAspect.blunt:
                {
                    m_damage.value = Mathf.RoundToInt(m_damage.value * bluntElementDamageModifier);
                    if (bluntElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    break;
                }
            }
        }

        float percentage;

        switch (damage.applicationType)
        {
            default:
            {
                if (m_damage.value != 0)
                    ModifyHealth(m_damage.value, kill);
                //Debug.Log($"modify {damage.value} kill {kill}");
                break;
            }
            case DamageApplicationType.percentage:
            {
                percentage = Mathf.Abs(damage.value) / 100f;

                if (percentage > 1f)
                    percentage = 1f;
                else if (percentage < 0f)
                    percentage = 0f;

                if (percentage > 0f)
                    RemoveHealth(damage.value / 100.0f, false, kill);

                //Debug.Log($"remove damage.value {damage.value} damage.value / 100.0f {damage.value / 100.0f} kill {kill}");
                break;
            }
            case DamageApplicationType.percentageFromMax:
            {
                percentage = Mathf.Abs(damage.value) / 100f;

                if (percentage > 1f)
                    percentage = 1f;
                else if (percentage < 0f)
                    percentage = 0f;

                if (percentage > 0f)
                    RemoveHealth(damage.value / 100.0f, true, kill);

                //Debug.Log($"remove damage.value {damage.value} damage.value / 100.0f {damage.value / 100.0f} kill {kill}");
                break;
            }
            case DamageApplicationType.set:
            {
                int damageAbs = Mathf.Abs(damage.value);
                if (damageAbs > 0)
                    SetHealth(damageAbs, kill);

                //Debug.Log($"set {damage.value} kill {kill}");
                break;
            }
        }

        ShowDamageFlyText(damage);

        //ModifyHealth(damage.value, kill);
    }

    public void ModifyHealth(int value, bool kill = false)
    {
        if (!gameObject.activeSelf)
            return;

        if (health <= 0)
            return;

        if (kill)
        {
            value = Mathf.RoundToInt(-1 * (float)currentMaxHealth);
            //Debug.Log($"hp: {value}");
        }

        if (!kill)
        {
            health += Mathf.RoundToInt(value * currentDamageReduction);
        }
        else
        {
            health += value;
        }

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
        if (health > currentMaxHealth)
        {
            health = currentMaxHealth;
        }

        // USER INTERFACE
        if (healthBar != null)
        {
            healthBar.value = health;
            healthBar.maxValue = currentMaxHealth;
        }
        if (healthBarText != null)
        {
            if (healthBarTextInPercentage)
            {
                float healthPercentage = ((float)health / (float)currentMaxHealth) * 100f;
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
            healthBarParty.maxValue = currentMaxHealth;
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

    public void AddMovementSpeedModifier(float value, string identifier)
    {
        if (!speedModifiers.ContainsKey(identifier))
        {
            speedModifiers.Add(identifier, value);
        }
        else
        {
            return;
        }

        RecalculateCurrentMovementSpeed();
    }

    public void RemoveMovementSpeedModifier(string identifier)
    {
        if (speedModifiers.ContainsKey(identifier))
        {
            speedModifiers.Remove(identifier);
        }
        else
        {
            return;
        }

        RecalculateCurrentMovementSpeed();
    }

    public void AddMovementSpeed(float value, string identifier)
    {
        if (!speed.ContainsKey(identifier))
        {
            speed.Add(identifier, value);
        }
        else
        {
            return;
        }

        RecalculateCurrentMovementSpeed();
    }

    public void RemoveMovementSpeed(string identifier)
    {
        if (speed.ContainsKey(identifier))
        {
            speed.Remove(identifier);
        }
        else
        {
            return;
        }

        RecalculateCurrentMovementSpeed();
    }

    private void RecalculateCurrentMovementSpeed()
    {
        float result = defaultSpeed;

        List<float> sList = new List<float>(speed.Values.ToArray());

        if (speed.Count > 0)
        {
            sList.Sort();
            result = sList.Max();
        }

        List<float> mList = new List<float>(speedModifiers.Values.ToArray());

        if (speedModifiers.Count > 0)
        {
            mList.Sort();
            for (int i = 0; i < speedModifiers.Count; i++)
            {
                result *= mList[i];
            }
        }

        currentSpeed = result;
    }

    public void AddDamageModifier(float value, string identifier, DamageType damageType, ElementalAspect elementalAspect = ElementalAspect.none, PhysicalAspect physicalAspect = PhysicalAspect.none)
    {
        bool update = false;

        switch (damageType)
        {
            case DamageType.magical:
            {
                if (!magicalTypeDamageModifiers.ContainsKey(identifier))
                {
                    magicalTypeDamageModifiers.Add(identifier, value);
                    update = true;
                }
                break;
            }
            case DamageType.physical:
            {
                if (!physicalTypeDamageModifiers.ContainsKey(identifier))
                {
                    physicalTypeDamageModifiers.Add(identifier, value);
                    update = true;
                }
                break;
            }
            case DamageType.unique:
            {
                if (!uniqueTypeDamageModifiers.ContainsKey(identifier))
                {
                    uniqueTypeDamageModifiers.Add(identifier, value);
                    update = true;
                }
                break;
            }
        }
        switch (elementalAspect)
        {
            case ElementalAspect.unaspected:
            {
                if (!unaspectedElementDamageModifiers.ContainsKey(identifier))
                {
                    unaspectedElementDamageModifiers.Add(identifier, value);
                    update = true;
                }
                break;
            }
            case ElementalAspect.fire:
            {
                if (!fireElementDamageModifiers.ContainsKey(identifier))
                {
                    fireElementDamageModifiers.Add(identifier, value);
                    update = true;
                }
                break;
            }
            case ElementalAspect.ice:
            {
                if (!iceElementDamageModifiers.ContainsKey(identifier))
                {
                    iceElementDamageModifiers.Add(identifier, value);
                    update = true;
                }
                break;
            }
            case ElementalAspect.lightning:
            {
                if (!lightningElementDamageModifiers.ContainsKey(identifier))
                {
                    lightningElementDamageModifiers.Add(identifier, value);
                    update = true;
                }
                break;
            }
            case ElementalAspect.water:
            {
                if (!waterElementDamageModifiers.ContainsKey(identifier))
                {
                    waterElementDamageModifiers.Add(identifier, value);
                    update = true;
                }
                break;
            }
            case ElementalAspect.wind:
            {
                if (!windElementDamageModifiers.ContainsKey(identifier))
                {
                    windElementDamageModifiers.Add(identifier, value);
                    update = true;
                }
                break;
            }
            case ElementalAspect.earth:
            {
                if (!earthElementDamageModifiers.ContainsKey(identifier))
                {
                    earthElementDamageModifiers.Add(identifier, value);
                    update = true;
                }
                break;
            }
            case ElementalAspect.dark:
            {
                if (!darkElementDamageModifiers.ContainsKey(identifier))
                {
                    darkElementDamageModifiers.Add(identifier, value);
                    update = true;
                }
                break;
            }
            case ElementalAspect.light:
            {
                if (!lightElementDamageModifiers.ContainsKey(identifier))
                {
                    lightElementDamageModifiers.Add(identifier, value);
                    update = true;
                }
                break;
            }
        }
        switch (physicalAspect)
        {
            case PhysicalAspect.slashing:
            {
                if (!slashingElementDamageModifiers.ContainsKey(identifier))
                {
                    slashingElementDamageModifiers.Add(identifier, value);
                    update = true;
                }
                break;
            }
            case PhysicalAspect.piercing:
            {
                if (!piercingElementDamageModifiers.ContainsKey(identifier))
                {
                    piercingElementDamageModifiers.Add(identifier, value);
                    update = true;
                }
                break;
            }
            case PhysicalAspect.blunt:
            {
                if (!bluntElementDamageModifiers.ContainsKey(identifier))
                {
                    bluntElementDamageModifiers.Add(identifier, value);
                    update = true;
                }
                break;
            }
        }

        if (!update)
        {
            return;
        }

        RecalculateCurrentDamageModifiers(damageType, elementalAspect, physicalAspect);
    }

    public void RemoveDamageModifier(string identifier, DamageType damageType, ElementalAspect elementalAspect = ElementalAspect.none, PhysicalAspect physicalAspect = PhysicalAspect.none)
    {
        bool update = false;

        switch (damageType)
        {
            case DamageType.magical:
            {
                if (magicalTypeDamageModifiers.ContainsKey(identifier))
                {
                    magicalTypeDamageModifiers.Remove(identifier);
                    update = true;
                }
                break;
            }
            case DamageType.physical:
            {
                if (physicalTypeDamageModifiers.ContainsKey(identifier))
                {
                    physicalTypeDamageModifiers.Remove(identifier);
                    update = true;
                }
                break;
            }
            case DamageType.unique:
            {
                if (uniqueTypeDamageModifiers.ContainsKey(identifier))
                {
                    uniqueTypeDamageModifiers.Remove(identifier);
                    update = true;
                }
                break;
            }
        }
        switch (elementalAspect)
        {
            case ElementalAspect.unaspected:
            {
                if (unaspectedElementDamageModifiers.ContainsKey(identifier))
                {
                    unaspectedElementDamageModifiers.Remove(identifier);
                    update = true;
                }
                break;
            }
            case ElementalAspect.fire:
            {
                if (fireElementDamageModifiers.ContainsKey(identifier))
                {
                    fireElementDamageModifiers.Remove(identifier);
                    update = true;
                }
                break;
            }
            case ElementalAspect.ice:
            {
                if (iceElementDamageModifiers.ContainsKey(identifier))
                {
                    iceElementDamageModifiers.Remove(identifier);
                    update = true;
                }
                break;
            }
            case ElementalAspect.lightning:
            {
                if (lightningElementDamageModifiers.ContainsKey(identifier))
                {
                    lightningElementDamageModifiers.Remove(identifier);
                    update = true;
                }
                break;
            }
            case ElementalAspect.water:
            {
                if (waterElementDamageModifiers.ContainsKey(identifier))
                {
                    waterElementDamageModifiers.Remove(identifier);
                    update = true;
                }
                break;
            }
            case ElementalAspect.wind:
            {
                if (windElementDamageModifiers.ContainsKey(identifier))
                {
                    windElementDamageModifiers.Remove(identifier);
                    update = true;
                }
                break;
            }
            case ElementalAspect.earth:
            {
                if (earthElementDamageModifiers.ContainsKey(identifier))
                {
                    earthElementDamageModifiers.Remove(identifier);
                    update = true;
                }
                break;
            }
            case ElementalAspect.dark:
            {
                if (darkElementDamageModifiers.ContainsKey(identifier))
                {
                    darkElementDamageModifiers.Remove(identifier);
                    update = true;
                }
                break;
            }
            case ElementalAspect.light:
            {
                if (lightElementDamageModifiers.ContainsKey(identifier))
                {
                    lightElementDamageModifiers.Remove(identifier);
                    update = true;
                }
                break;
            }
        }
        switch (physicalAspect)
        {
            case PhysicalAspect.slashing:
            {
                if (slashingElementDamageModifiers.ContainsKey(identifier))
                {
                    slashingElementDamageModifiers.Remove(identifier);
                    update = true;
                }
                break;
            }
            case PhysicalAspect.piercing:
            {
                if (piercingElementDamageModifiers.ContainsKey(identifier))
                {
                    piercingElementDamageModifiers.Remove(identifier);
                    update = true;
                }
                break;
            }
            case PhysicalAspect.blunt:
            {
                if (bluntElementDamageModifiers.ContainsKey(identifier))
                {
                    bluntElementDamageModifiers.Remove(identifier);
                    update = true;
                }
                break;
            }
        }

        if (!update)
        {
            return;
        }

        RecalculateCurrentDamageModifiers(damageType, elementalAspect, physicalAspect);
    }

    private void RecalculateCurrentDamageModifiers(DamageType damageType, ElementalAspect elementalAspect, PhysicalAspect physicalAspect)
    {
        switch (damageType)
        {
            case DamageType.magical:
            {
                float result = 1f;

                List<float> a = new List<float>(magicalTypeDamageModifiers.Values.ToArray());

                if (magicalTypeDamageModifiers.Count > 0)
                {
                    a.Sort();
                    for (int i = 0; i < magicalTypeDamageModifiers.Count; i++)
                    {
                        result *= a[i];
                    }
                }

                magicalTypeDamageModifier = result;
                break;
            }
            case DamageType.physical:
            {
                float result = 1f;

                List<float> a = new List<float>(physicalTypeDamageModifiers.Values.ToArray());

                if (physicalTypeDamageModifiers.Count > 0)
                {
                    a.Sort();
                    for (int i = 0; i < physicalTypeDamageModifiers.Count; i++)
                    {
                        result *= a[i];
                    }
                }

                physicalTypeDamageModifier = result;
                break;
            }
            case DamageType.unique:
            {
                float result = 1f;

                List<float> a = new List<float>(uniqueTypeDamageModifiers.Values.ToArray());

                if (uniqueTypeDamageModifiers.Count > 0)
                {
                    a.Sort();
                    for (int i = 0; i < uniqueTypeDamageModifiers.Count; i++)
                    {
                        result *= a[i];
                    }
                }

                uniqueTypeDamageModifier = result;
                break;
            }
        }
        switch (elementalAspect)
        {
            case ElementalAspect.unaspected:
            {
                float result = 1f;

                List<float> a = new List<float>(unaspectedElementDamageModifiers.Values.ToArray());

                if (unaspectedElementDamageModifiers.Count > 0)
                {
                    a.Sort();
                    for (int i = 0; i < unaspectedElementDamageModifiers.Count; i++)
                    {
                        result *= a[i];
                    }
                }

                unaspectedElementDamageModifier = result;
                break;
            }
            case ElementalAspect.fire:
            {
                float result = 1f;

                List<float> a = new List<float>(fireElementDamageModifiers.Values.ToArray());

                if (fireElementDamageModifiers.Count > 0)
                {
                    a.Sort();
                    for (int i = 0; i < fireElementDamageModifiers.Count; i++)
                    {
                        result *= a[i];
                    }
                }

                fireElementDamageModifier = result;
                break;
            }
            case ElementalAspect.ice:
            {
                float result = 1f;

                List<float> a = new List<float>(iceElementDamageModifiers.Values.ToArray());

                if (iceElementDamageModifiers.Count > 0)
                {
                    a.Sort();
                    for (int i = 0; i < iceElementDamageModifiers.Count; i++)
                    {
                        result *= a[i];
                    }
                }

                iceElementDamageModifier = result;
                break;
            }
            case ElementalAspect.lightning:
            {
                float result = 1f;

                List<float> a = new List<float>(lightningElementDamageModifiers.Values.ToArray());

                if (lightningElementDamageModifiers.Count > 0)
                {
                    a.Sort();
                    for (int i = 0; i < lightningElementDamageModifiers.Count; i++)
                    {
                        result *= a[i];
                    }
                }

                lightningElementDamageModifier = result;
                break;
            }
            case ElementalAspect.water:
            {
                float result = 1f;

                List<float> a = new List<float>(waterElementDamageModifiers.Values.ToArray());

                if (waterElementDamageModifiers.Count > 0)
                {
                    a.Sort();
                    for (int i = 0; i < waterElementDamageModifiers.Count; i++)
                    {
                        result *= a[i];
                    }
                }

                waterElementDamageModifier = result;
                break;
            }
            case ElementalAspect.wind:
            {
                float result = 1f;

                List<float> a = new List<float>(windElementDamageModifiers.Values.ToArray());

                if (windElementDamageModifiers.Count > 0)
                {
                    a.Sort();
                    for (int i = 0; i < windElementDamageModifiers.Count; i++)
                    {
                        result *= a[i];
                    }
                }

                windElementDamageModifier = result;
                break;
            }
            case ElementalAspect.earth:
            {
                float result = 1f;

                List<float> a = new List<float>(earthElementDamageModifiers.Values.ToArray());

                if (earthElementDamageModifiers.Count > 0)
                {
                    a.Sort();
                    for (int i = 0; i < earthElementDamageModifiers.Count; i++)
                    {
                        result *= a[i];
                    }
                }

                earthElementDamageModifier = result;
                break;
            }
            case ElementalAspect.dark:
            {
                float result = 1f;

                List<float> a = new List<float>(darkElementDamageModifiers.Values.ToArray());

                if (darkElementDamageModifiers.Count > 0)
                {
                    a.Sort();
                    for (int i = 0; i < darkElementDamageModifiers.Count; i++)
                    {
                        result *= a[i];
                    }
                }

                darkElementDamageModifier = result;
                break;
            }
            case ElementalAspect.light:
            {
                float result = 1f;

                List<float> a = new List<float>(lightElementDamageModifiers.Values.ToArray());

                if (lightElementDamageModifiers.Count > 0)
                {
                    a.Sort();
                    for (int i = 0; i < lightElementDamageModifiers.Count; i++)
                    {
                        result *= a[i];
                    }
                }

                lightElementDamageModifier = result;
                break;
            }
        }
        switch (physicalAspect)
        {
            case PhysicalAspect.slashing:
            {
                float result = 1f;

                List<float> a = new List<float>(slashingElementDamageModifiers.Values.ToArray());

                if (slashingElementDamageModifiers.Count > 0)
                {
                    a.Sort();
                    for (int i = 0; i < slashingElementDamageModifiers.Count; i++)
                    {
                        result *= a[i];
                    }
                }

                slashingElementDamageModifier = result;
                break;
            }
            case PhysicalAspect.piercing:
            {
                float result = 1f;

                List<float> a = new List<float>(piercingElementDamageModifiers.Values.ToArray());

                if (piercingElementDamageModifiers.Count > 0)
                {
                    a.Sort();
                    for (int i = 0; i < piercingElementDamageModifiers.Count; i++)
                    {
                        result *= a[i];
                    }
                }

                piercingElementDamageModifier = result;
                break;
            }
            case PhysicalAspect.blunt:
            {
                float result = 1f;

                List<float> a = new List<float>(bluntElementDamageModifiers.Values.ToArray());

                if (bluntElementDamageModifiers.Count > 0)
                {
                    a.Sort();
                    for (int i = 0; i < bluntElementDamageModifiers.Count; i++)
                    {
                        result *= a[i];
                    }
                }

                bluntElementDamageModifier = result;
                break;
            }
        }
    }

    public void AddMaxHealth(float value, string identifier)
    {
        if (!maxHealthModifiers.ContainsKey(identifier))
        {
            maxHealthModifiers.Add(identifier, value);
        }
        else
        {
            return;
        }

        RecalculateCurrentMaxHealth();
    }

    public void RemoveMaxHealth(string identifier)
    {
        if (maxHealthModifiers.ContainsKey(identifier))
        {
            maxHealthModifiers.Remove(identifier);
        }
        else
        {
            return;
        }

        RecalculateCurrentMaxHealth();
    }

    private void RecalculateCurrentMaxHealth()
    {
        float result = 1f;

        List<float> a = new List<float>(maxHealthModifiers.Values.ToArray());

        if (maxHealthModifiers.Count > 0)
        {
            a.Sort();
            for (int i = 0; i < maxHealthModifiers.Count; i++)
            {
                result *= a[i];
            }
        }

        currentMaxHealth = Mathf.RoundToInt(result);

        if (healthBar != null)
        {
            healthBar.value = health;
            healthBar.maxValue = currentMaxHealth;
        }
        if (healthBarParty != null)
        {
            healthBarParty.value = health;
            healthBarParty.maxValue = currentMaxHealth;
        }
    }

    public void AddEffect(StatusEffectData data, bool self = false, int tag = 0)
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
        AddEffect(newStatusEffect.GetComponent<StatusEffect>(), self, tag);
    }

    public void AddEffect(string name, bool self = false, int tag = 0)
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
                AddEffect(newStatusEffect.GetComponent<StatusEffect>(), self, tag);
            }
        }
    }

    public void AddEffect(StatusEffect effect, bool self = false, int tag = 0)
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
        {
            if (self)
            {
                effect.Initialize(statusEffectNegativeIconParent, statusEffectIconParentParty, ownStatusEffectColor);
            }
            else
            {
                effect.Initialize(statusEffectNegativeIconParent, statusEffectIconParentParty, otherStatusEffectColor);
            }
        }
        else
        {
            if (self)
            {
                effect.Initialize(statusEffectPositiveIconParent, statusEffectIconParentParty, ownStatusEffectColor);
            }
            else
            {
                effect.Initialize(statusEffectPositiveIconParent, statusEffectIconParentParty, otherStatusEffectColor);
            }
        }
        effect.onApplication.Invoke(this);

        if (showDamagePopups)
        {
            ShowStatusEffectFlyText(effect, " + ");
        }
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

        if (showDamagePopups)
        {
            ShowStatusEffectFlyText(temp, " - ");
        }

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

    public void ShowStatusEffectFlyText(StatusEffect statusEffect, string prefix)
    {
        ShowStatusEffectFlyText(statusEffect.data, prefix);
    }

    public void ShowStatusEffectFlyText(StatusEffectData data, string prefix)
    {
        if (data.hidden) 
            return;

        // Hard coded the Short (@S) and Long (@L) that are used to distinguish between few of the same debuffs, needs a better implementation
        string result = $"{prefix}{Utilities.InsertSpaceBeforeCapitals(data.statusName).Replace("@S","").Replace("@L","")}";

        Color color = positivePopupColor;

        if (data.negative)
        {
            color = negativePopupColor;
        }

        Sprite icon = data.hudElement.transform.GetChild(0).GetComponent<Image>().sprite;

        ShowFlyText(new FlyText(result, color, icon, string.Empty));
    }

    public void ShowDamageFlyText(Damage damage)
    {
        string result = $"<sprite=\"damage_types\" name=\"{(int)damage.type}\">{Mathf.Abs(damage.value)}";

        string source = damage.name;

        Color color = positivePopupColor;

        if (damage.negative || damage.value < 0)
        {
            color = negativePopupColor;
        }

        ShowFlyText(new FlyText(result, color, null, source));
    }

    public void ShowDamageFlyText(int value, string source)
    {
        string result = Mathf.Abs(value).ToString();

        Color color = positivePopupColor;

        if (value < 0)
        {
            color = negativePopupColor;
        }

        ShowFlyText(new FlyText(result, color, null, source));
    }

    public void ShowFlyText(FlyText text)
    {
        if (!gameObject.activeSelf)
            return;

        if (damagePopupPrefab == null || damagePopupParent == null)
            return;

        if (string.IsNullOrEmpty(text.content))
            return;

        popupTexts.Enqueue(text);

        if (popupCoroutine == null)
        {
            popupCoroutine = StartCoroutine(ProcessFlyTextQueue());
        }
    }

    private IEnumerator ProcessFlyTextQueue()
    {
        while (popupTexts.Count > 0)
        {
            FlyText text = popupTexts.Dequeue();

            GameObject go = Instantiate(damagePopupPrefab, damagePopupParent);
            TextMeshProUGUI tm = go.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            tm.text = text.content;
            tm.color = text.color;

            TextMeshProUGUI sTm = go.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

            if (!string.IsNullOrEmpty(text.source))
            {
                sTm.gameObject.SetActive(true);
                sTm.text = text.source;
                sTm.color = text.color;
            }
            else
            {
                sTm.gameObject.SetActive(false);
            }

            Image image = go.transform.GetComponentInChildren<Image>();
            if (text.icon != null)
            {
                image.sprite = text.icon;
                image.gameObject.SetActive(true);
            }
            else
            {
                image.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(popupTextDelay);
        }

        popupCoroutine = null;
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
                characterNameTextGroupParty.alpha = 1f;
            }
            else
            {
                characterNameTextGroupParty.alpha = 0f;
            }
        }
    }

    public struct FlyText
    {
        public string content;
        public Color color;
        public Sprite icon;
        public string source;

        public FlyText(string content, Color color, Sprite icon, string source)
        {
            this.content = content;
            this.color = color;
            this.icon = icon;
            this.source = source;
        }
    }
}
