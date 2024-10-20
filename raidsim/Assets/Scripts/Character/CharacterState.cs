using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static GlobalStructs;
using static GlobalStructs.Damage;
using static PartyList;
using static UnityEngine.Rendering.DebugUI;

public class CharacterState : MonoBehaviour
{
    public enum Role { meleeDps, magicalRangedDps, physicalRangedDps, tank, healer, unassigned }

    [HideInInspector]
    public PlayerController playerController;
    [HideInInspector]
    public AIController aiController;
    [HideInInspector]
    public BossController bossController;
    [HideInInspector]
    public TargetController targetController;

    [Header("Status")]
    public string characterName = "Unknown";

    private long defaultMaxHealth;
    public long currentMaxHealth;
    public long health = 16000;
    public Dictionary<string, float> maxHealthModifiers = new Dictionary<string, float>();
    public long shield = 0;
    public List<Shield> currentShields = new List<Shield>();

    public float defaultSpeed { private set; get; }
    public float currentSpeed = 6.3f;
    public Dictionary<string, float> speedModifiers = new Dictionary<string, float>();
    public Dictionary<string, float> speed = new Dictionary<string, float>();
    //private float maxSpeed = 15f;

    public float currentDamageOutputMultiplier = 1f;
    public Dictionary<string, float> damageOutputModifiers = new Dictionary<string, float>();

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
    public Dictionary<string, float> poisonDamageModifiers = new Dictionary<string, float>();
    public float poisonDamageModifier = 1f;
    public Dictionary<string, float> enmityGenerationModifiers = new Dictionary<string, float>();
    public float enmityGenerationModifier = 1f;

    public Dictionary<CharacterState, long> enmity = new Dictionary<CharacterState, long>();
#if UNITY_EDITOR
    public List<Enmity> m_enmity = new List<Enmity>();

    [System.Serializable]
    public struct Enmity
    {
        public CharacterState characterState;
        public long enmityValue;

        public Enmity(CharacterState characterState, long enmityValue)
        {
            this.characterState = characterState;
            this.enmityValue = enmityValue;
        }
    }

    [System.Serializable]
    public struct StatusEffectPair
    {
        public string name;
        public StatusEffect statusEffect;

        public StatusEffectPair(string name, StatusEffect statusEffect)
        {
            this.name = name;
            this.statusEffect = statusEffect;
        }
    }
#endif
    [Header("States")]
    public bool invulnerable = false;
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
    public bool canDie = true;
    public bool disabled = false;
    public bool canGainEnmity = true;

    [Header("Effects")]
    private Dictionary<string, StatusEffect> effects = new Dictionary<string, StatusEffect>();
#if UNITY_EDITOR
    public List<StatusEffectPair> m_effects = new List<StatusEffectPair>();
#endif
    private StatusEffect[] effectsArray = null;
    private List<StatusEffect> instantCasts = new List<StatusEffect>();
    [HideInInspector]
    public UnityEvent<List<StatusEffect>> onInstantCastsChanged;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent onSpawn;

    [Header("Config")]
    public PartyList partyList;
    private PartyMember? partyMember;
    public Transform statusEffectParent;
    public float statusEffectUpdateInterval = 3f;
    private float statusEffectUpdateTimer = 0f;
    public int characterLevel = 0;
    public int characterAggressionLevel = 1;
    public int characterLetter = 0;
    public string letterSpriteAsset = "letters_1";
    public Role role = Role.unassigned;
    public bool isAggressive = true;
    public bool hideNameplate = false;
    public bool hidePartyName = false;
    public bool hidePartyListEntry = false;

    [Header("Personal - Name")]
    public bool showCharacterName = true;
    public bool showCharacterLevel = true;
    public bool showCharacterAggression = true;
    public bool showCharacterNameLetter = true;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI nameplateCharacterNameText;
    private CanvasGroup nameplateCharacterNameTextGroup;
    public CanvasGroup nameplateGroup;
    [Header("Personal - Status Effects")]
    public bool showStatusEffects = true;
    public Transform statusEffectPositiveIconParent;
    public Transform statusEffectNegativeIconParent;
    public Color ownStatusEffectColor = Color.green;
    public Color otherStatusEffectColor = Color.white;
    [Header("Personal - Damage Popups")]
    public bool showDamagePopups = true;
    public bool showElementalAspect = true;
    public GameObject damagePopupPrefab;
    public Transform damagePopupParent;
    public Color negativePopupColor = Color.red;
    public Color positivePopupColor = Color.green;
    public Color neutralPopupColor = Color.white;
    public float popupTextDelay = 0.5f;
    private Queue<FlyText> popupTexts = new Queue<FlyText>();
    private Coroutine popupCoroutine;
    [Header("Personal - Health Bar")]
    public bool showHealthBar = true;
    public Slider healthBar;
    public TextMeshProUGUI healthBarText;
    public bool healthBarTextInPercentage = false;
    public bool showNameplateHealthBar = true;
    public bool showOnlyBelowMaxHealth = false;
    public Slider nameplateHealthBar;
    private CanvasGroup nameplateHealthBarGroup;

    [Header("Party - Name")]
    public bool showPartyCharacterName = true;
    public bool showPartyCharacterLevel = true;
    public bool showPartyCharacterNameLetter = true;
    public TextMeshProUGUI characterNameTextParty;
    private CanvasGroup characterNameTextGroupParty;
    [Header("Party - Status Effects")]
    public bool showPartyListStatusEffects = true;
    public Transform statusEffectIconParentParty;
    [Header("Party - Health Bar")]
    public bool showPartyHealthBar = true;
    public Slider healthBarParty;
    public TextMeshProUGUI healthBarTextParty;
    public Slider shieldBarParty;
    public Slider overShieldBarParty;

    [Header("Target - StatusEffects")]
    public bool showStatusEffectsWhenTargeted = true;
    public GameObject targetStatusEffectHolderPrefab;
    public Transform targetStatusEffectHolderParent;
    public CanvasGroup targetStatusEffectIconGroup;
    public Transform targetStatusEffectIconParent;

#if UNITY_EDITOR
    [Header("Editor")]
    public StatusEffectData.StatusEffectInfo inflictedEffect;
    [Button("Inflict Status Effect")]
    public void InflictStatusEffectButton()
    {
        AddEffect(inflictedEffect.data, true, inflictedEffect.tag, inflictedEffect.stacks);
    }
    [Button("Cleanse Status Effect")]
    public void CleanseStatusEffectButton()
    {
        RemoveEffect(inflictedEffect.data, true, inflictedEffect.tag, inflictedEffect.stacks);
    }
#endif

    public string GetCharacterName()
    {
        int index = characterName.IndexOf('#');
        if (index >= 0) // Check if '#' exists in the string
        {
            return characterName.Substring(0, index);
        }
        return characterName; // Return the full string if no '#' is found
    }

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        aiController = GetComponent<AIController>();
        bossController = GetComponent<BossController>();
        targetController = GetComponent<TargetController>();

        if (nameplateGroup == null)
        {
            CanvasGroup[] children = transform.GetComponentsInChildren<CanvasGroup>();
            foreach (CanvasGroup child in children)
            {
                if (child.gameObject.name == "NameplateGroup")
                {
                    nameplateGroup = child.GetComponent<CanvasGroup>();
                    break;
                }
            }
        }

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
        if (targetStatusEffectIconParent != null)
        {
            foreach (Transform child in targetStatusEffectIconParent)
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
        enmity = new Dictionary<CharacterState, long>();

        if (healthBar != null)
        {
            healthBar.maxValue = currentMaxHealth;
            healthBar.value = health;
        }
        if (nameplateHealthBar != null)
        {
            nameplateHealthBar.maxValue = currentMaxHealth;
            nameplateHealthBar.value = health;

            nameplateHealthBarGroup = nameplateHealthBar.GetComponentInParent<CanvasGroup>();

            if (nameplateHealthBarGroup != null)
            {
                if (showNameplateHealthBar)
                {
                    if (showOnlyBelowMaxHealth && health >= currentMaxHealth)
                    {
                        nameplateHealthBarGroup.alpha = 0f;
                    }
                    else
                    {
                        nameplateHealthBarGroup.alpha = 1f;
                    }
                }
                else
                {
                    nameplateHealthBarGroup.alpha = 0f;
                }
            }
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
                    string result = healthPercentage.ToString("F1") + "%";

                    if (health > 0)
                    {
                        if (result == "0%" || result == "0.0%" || result == "0.00%" || result == "00.0%" || result == "00.00%" || result == "0,0%" || result == "0,00%" || result == "00,0%" || result == "00,00%")
                        {
                            result = "0.1%";
                        }
                    }

                    healthBarText.text = result;
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
            if (shieldBarParty != null)
            {
                shieldBarParty.maxValue = currentMaxHealth;
                shieldBarParty.value = health + shield;
            }
            if (overShieldBarParty != null)
            {
                overShieldBarParty.minValue = currentMaxHealth;
                overShieldBarParty.maxValue = currentMaxHealth + currentMaxHealth;
                overShieldBarParty.value = health + shield;
            }
        }
        if (healthBarTextParty != null)
        {
            healthBarTextParty.text = health.ToString();
        }
        if (nameplateCharacterNameText != null)
        {
            string letter = "";

            if (showCharacterNameLetter && characterLetter >= 0 && characterLetter <= 25)
            {
                letter = $"<sprite =\"{letterSpriteAsset}\" name=\"{characterLetter}\" tint=\"FF7E95\">";
            }

            if (showCharacterAggression)
            {
                string aggression = string.Empty;
                if (isAggressive)
                    aggression = $"a{characterAggressionLevel}";
                else
                    aggression = $"p{characterAggressionLevel}";

                if (showCharacterLevel)
                {
                    nameplateCharacterNameText.text = $"<sprite=\"enemy_ranks\" name=\"{aggression}\"><b>Lv{characterLevel}</b> {GetCharacterName()}{letter}>";
                }
                else
                {
                    nameplateCharacterNameText.text = $"<sprite=\"enemy_ranks\" name=\"{aggression}\"> {GetCharacterName()}{letter}";
                }
            }
            else
            {
                if (showCharacterLevel)
                {
                    nameplateCharacterNameText.text = $"<b>Lv{characterLevel}</b> {GetCharacterName()}{letter}";
                }
                else
                {
                    nameplateCharacterNameText.text = $"{GetCharacterName()}{letter}";
                }
            }
        }
        if (partyList != null)
        {
            partyMember = partyList.GetMember(this);

            if (partyMember != null && partyMember?.hudElement != null)
            {
                HudElement hudElement = partyMember?.hudElement;
                hudElement.hidden = hidePartyListEntry;
            }
        }
        if (nameplateGroup != null)
        {
            if (!hideNameplate)
            {
                nameplateGroup.alpha = 1f;
            }
            else
            {
                nameplateGroup.alpha = 0f;
            }
        }
        if (nameplateCharacterNameTextGroup != null)
        {
            if (showCharacterName)
            {
                nameplateCharacterNameTextGroup.alpha = 1f;
            }
            else
            {
                nameplateCharacterNameTextGroup.LeanAlpha(0f, 0.5f);
            }
        }
        if (characterNameTextParty != null)
        {
            string letter = "";

            if (showPartyCharacterNameLetter && characterLetter >= 0 && characterLetter <= 25)
            {
                letter = $"<sprite =\"{letterSpriteAsset}\" name=\"{characterLetter}\" tint=\"FF7E95\">";
            }

            if (showPartyCharacterLevel)
            {
                characterNameTextParty.text = $"{letter}Lv{characterLevel} {GetCharacterName()}";
            }
            else
            {
                characterNameTextParty.text = $"{letter}{GetCharacterName()}";
            }
        }

        if (characterNameTextGroupParty == null && characterNameTextParty != null)
            characterNameTextGroupParty = characterNameTextParty.transform.parent.GetComponent<CanvasGroup>();

        if (characterNameTextGroupParty != null)
        {
            if (!hidePartyName && showPartyCharacterName)
            {
                characterNameTextGroupParty.alpha = 1f;
            }
            else
            {
                characterNameTextGroupParty.LeanAlpha(0f, 0.5f);
            }
        }
    }

    void Start()
    {
        Utilities.FunctionTimer.Create(this, () => onSpawn.Invoke(), 0.85f, $"{characterName}_onSpawn_delay", false, true);

        if (disabled)
        {
            Utilities.FunctionTimer.Create(this, () => gameObject.SetActive(false), 1f, $"{characterName}_disable_on_start", true, true);
        }
    }

    void OnEnable()
    {
        if (partyList != null)
        {
            Utilities.FunctionTimer.Create(this, () => partyList.UpdatePartyList(), 1f, $"{characterName}_on_enable_update_party_list", true, true);
        }
    }

    void OnDisable()
    {
        if (partyList != null)
        {
            Utilities.FunctionTimer.Create(this, () => partyList.UpdatePartyList(), 1f, $"{characterName.Replace(" ","_")}_on_disable_update_party_list", true, true);
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
            for (int i = effectsArray.Length - 1; i >= 0; i--)
            {
                if (effectsArray[i].data.unaffectedByTimeScale)
                {
                    effectsArray[i].duration -= Time.deltaTime;
                }
                else
                {
                    effectsArray[i].duration -= FightTimeline.deltaTime;
                }
                effectsArray[i].onUpdate.Invoke(this);
                if (effectsArray[i].duration <= 0f)
                {
                    RemoveEffect(effectsArray[i], true, effectsArray[i].uniqueTag, effectsArray[i].stacks);
                }
            }
        }

        if (characterNameText != null)
            characterNameText.text = GetCharacterName();
        if (characterNameTextParty != null)
        {
            string letter = "";

            if (showPartyCharacterNameLetter && characterLetter >= 0 && characterLetter <= 25)
            {
                letter = $"<sprite=\"{letterSpriteAsset}\" name=\"{characterLetter}\">";
            }

            if (showPartyCharacterLevel)
            {
                characterNameTextParty.text = $"{letter}Lv{characterLevel} {GetCharacterName()}";
            }
            else
            {
                characterNameTextParty.text = $"{letter}{GetCharacterName()}";
            }
        }
        if (nameplateCharacterNameText != null)
        {
            string letter = "";

            if (showCharacterNameLetter && characterLetter >= 0 && characterLetter <= 25)
            {
                letter = $"<sprite=\"{letterSpriteAsset}\" name=\"{characterLetter}\" tint=\"FF7E95\">";
            }

            if (showCharacterAggression)
            {
                string aggression = string.Empty;
                if (isAggressive)
                    aggression = $"a{characterAggressionLevel}";
                else
                    aggression = $"p{characterAggressionLevel}";

                if (showCharacterLevel)
                {
                    nameplateCharacterNameText.text = $"<sprite=\"enemy_ranks\" name=\"{aggression}\"><b>Lv{characterLevel}</b> {GetCharacterName()}{letter}";
                }
                else
                {
                    nameplateCharacterNameText.text = $"<sprite=\"enemy_ranks\" name=\"{aggression}\"> {GetCharacterName()}{letter}";
                }
            }
            else
            {
                if (showCharacterLevel)
                {
                    nameplateCharacterNameText.text = $"<b>Lv{characterLevel}</b> {GetCharacterName()}{letter}";
                }
                else
                {
                    nameplateCharacterNameText.text = $"{GetCharacterName()}{letter}";
                }
            }
        }
        if (partyList != null)
        {
            if (partyMember != null && partyMember?.hudElement != null)
            {
                HudElement hudElement = partyMember?.hudElement;
                hudElement.hidden = hidePartyListEntry;
            }
        }
        if (nameplateGroup != null)
        {
            if (!hideNameplate)
            {
                nameplateGroup.alpha = 1f;
            }
            else
            {
                nameplateGroup.alpha = 0f;
            }
        }
    }

    void OnDestroy()
    {
        if (targetStatusEffectIconParent != null)
            Destroy(targetStatusEffectIconParent.gameObject, 0.1f);
    }

    public void ToggleState()
    {
        disabled = !disabled;
        gameObject.SetActive(disabled);
    }

    public void ToggleState(bool state)
    {
        if (state)
        {
            disabled = false;
            gameObject.SetActive(true);
        }
        else
        {
            disabled = true;
            gameObject.SetActive(false);
        }
    }

    public void ModifyHealth(Damage damage, bool kill = false, bool noFlyText = false)
    {
        if (!gameObject.activeSelf)
            return;

        if (health <= 0 || dead)
            return;

        Damage m_damage = new Damage(damage);

        if (m_damage.negative)
        {
            switch (m_damage.type)
            {
                case DamageType.magical:
                {
                    if (magicalTypeDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    else
                    {
                        m_damage.value = Mathf.RoundToInt(m_damage.value * magicalTypeDamageModifier);
                    }
                    break;
                }
                case DamageType.physical:
                {
                    if (physicalTypeDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    else
                    {
                        m_damage.value = Mathf.RoundToInt(m_damage.value * physicalTypeDamageModifier);
                    }
                    break;
                }
                case DamageType.unique:
                {
                    if (uniqueTypeDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    else
                    {
                        m_damage.value = Mathf.RoundToInt(m_damage.value * uniqueTypeDamageModifier);
                    }
                    break;
                }
            }
            switch (m_damage.elementalAspect)
            {
                case ElementalAspect.unaspected:
                {
                    if (unaspectedElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    else
                    {
                        m_damage.value = Mathf.RoundToInt(m_damage.value * unaspectedElementDamageModifier);
                    }
                    break;
                }
                case ElementalAspect.fire:
                {
                    if (fireElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    else
                    {
                        m_damage.value = Mathf.RoundToInt(m_damage.value * fireElementDamageModifier);
                    }
                    break;
                }
                case ElementalAspect.ice:
                {
                    if (iceElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    else
                    {
                        m_damage.value = Mathf.RoundToInt(m_damage.value * iceElementDamageModifier);
                    }
                    break;
                }
                case ElementalAspect.lightning:
                {
                    if (lightningElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    else
                    {
                        m_damage.value = Mathf.RoundToInt(m_damage.value * lightningElementDamageModifier);
                    }
                    break;
                }
                case ElementalAspect.water:
                {
                    if (waterElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    else
                    {
                        m_damage.value = Mathf.RoundToInt(m_damage.value * waterElementDamageModifier);
                    }
                    break;
                }
                case ElementalAspect.wind:
                {
                    if (windElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    else
                    {
                        m_damage.value = Mathf.RoundToInt(m_damage.value * windElementDamageModifier);
                    }
                    break;
                }
                case ElementalAspect.earth:
                {
                    if (earthElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    else
                    {
                        m_damage.value = Mathf.RoundToInt(m_damage.value * earthElementDamageModifier);
                    }
                    break;
                }
                case ElementalAspect.dark:
                {
                    if (darkElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    else
                    {
                        m_damage.value = Mathf.RoundToInt(m_damage.value * darkElementDamageModifier);
                    }
                    break;
                }
                case ElementalAspect.light:
                {
                    if (lightElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    else
                    {
                        m_damage.value = Mathf.RoundToInt(m_damage.value * lightElementDamageModifier);
                    }
                    break;
                }
            }
            switch (m_damage.physicalAspect)
            {
                case PhysicalAspect.slashing:
                {
                    if (slashingElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    else
                    {
                        m_damage.value = Mathf.RoundToInt(m_damage.value * slashingElementDamageModifier);
                    }
                    break;
                }
                case PhysicalAspect.piercing:
                {
                    if (piercingElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    else
                    {
                        m_damage.value = Mathf.RoundToInt(m_damage.value * piercingElementDamageModifier);
                    }
                    break;
                }
                case PhysicalAspect.blunt:
                {
                    if (bluntElementDamageModifier >= 999999)
                    {
                        kill = true;
                    }
                    else
                    {
                        m_damage.value = Mathf.RoundToInt(m_damage.value * bluntElementDamageModifier);
                    }
                    break;
                }
            }
        }

        float percentage;
        bool ignoreDamageReduction = m_damage.ignoreDamageReductions;
        Damage flyTextDamage = new Damage(m_damage);

        if (m_damage.value > 9999999)
        {
            m_damage = new Damage(m_damage, 9999999);
        }

        switch (m_damage.applicationType)
        {
            default:
            {
                if (m_damage.value != 0)
                    ModifyHealth(m_damage.value, kill, ignoreDamageReduction);
                // Fly text damage is already accurate to the actual damage dealt
                break;
            }
            case DamageApplicationType.percentage:
            {
                percentage = Mathf.Abs(m_damage.value) / 100f;

                if (percentage > 1f)
                    percentage = 1f;
                else if (percentage < 0f)
                    percentage = 0f;

                if (percentage > 0f)
                    RemoveHealth(m_damage.value / 100.0f, false, m_damage.negative, kill, ignoreDamageReduction);
                // Set the fly text damage to be accurate to the actual damage dealt
                flyTextDamage.value = Mathf.RoundToInt(health * percentage);
                break;
            }
            case DamageApplicationType.percentageFromMax:
            {
                percentage = Mathf.Abs(m_damage.value) / 100f;

                if (percentage > 1f)
                    percentage = 1f;
                else if (percentage < 0f)
                    percentage = 0f;

                if (percentage > 0f)
                    RemoveHealth(m_damage.value / 100.0f, true, m_damage.negative, kill, ignoreDamageReduction, true);
                // Set the fly text damage to be accurate to the actual damage dealt
                flyTextDamage.value = Mathf.RoundToInt(currentMaxHealth * percentage);
                break;
            }
            case DamageApplicationType.set:
            {
                long damageAbs = Math.Abs(m_damage.value);

                if (damageAbs > 0)
                {
                    long previousHealth = health; // Store the current health before applying the damage
                    SetHealth(damageAbs, m_damage.negative, kill, ignoreDamageReduction, true);
                    long healthChange = previousHealth - health; // Calculate the actual health change

                    // Set the fly text damage to show the actual health change
                    flyTextDamage.value = -1 * healthChange;
                }
                break;
            }
        }

        if (!noFlyText)
            ShowDamageFlyText(flyTextDamage);

        //ModifyHealth(damage.value, kill);
    }

    private void SetHealth(long value, bool negative, bool kill = false, bool ignoreDamageReduction = false, bool ignoreShields = false)
    {
        if (kill)
        {
            if (negative)
            {
                ModifyHealth(0, kill, ignoreDamageReduction, ignoreShields);
            }
            else
            {
                ModifyHealth(currentMaxHealth, false, ignoreDamageReduction, ignoreShields);
            }
        }
        else
        {
            if (negative)
            {
                ModifyHealth(-1 * (health - value), kill, ignoreDamageReduction, ignoreShields);
            }
            else
            {
                ModifyHealth(Math.Abs(health - value), kill, ignoreDamageReduction, ignoreShields);
            }
        }
    }

    private void RemoveHealth(float percentage, bool fromMax, bool negative, bool kill = false, bool ignoreDamageReduction = false, bool ignoreShields = false)
    {
        if (kill)
        {
            ModifyHealth(0, kill, ignoreDamageReduction, ignoreShields);
        }
        else
        {
            long damage = fromMax ? (long)Math.Round(currentMaxHealth * percentage) : (long)Math.Round(health * percentage);
            if (negative)
            {
                ModifyHealth(-1 * damage, kill, ignoreDamageReduction, ignoreShields);
            }
            else
            {
                ModifyHealth(Math.Abs(damage), kill, ignoreDamageReduction, ignoreShields);
            }
        }
    }

    private void ModifyHealth(long value, bool kill = false, bool ignoreDamageReduction = false, bool ignoreShields = false)
    {
        if (kill)
        {
            value = (long)Math.Round(-1 * (float)currentMaxHealth);
        }

        if (!kill && !ignoreDamageReduction)
        {
            // Apply damage reduction
            value = (long)Math.Round(value * currentDamageReduction);
        }

        /*if (shield > 0)
        {
            if (shield >= Mathf.Abs(value))
            {
                // If shield can absorb all the damage
                shield += value;
            }
            else
            {
                // If shield can't absorb all the damage
                int remainingDamage = Mathf.Abs(value) - shield;
                shield = 0;
                health += -1 * remainingDamage;
            }
        }
        else
        {
            // If no shield, apply damage directly to health
            health += value;
            if (kill)
                shield -= shield;
        }*/

        //
        long remainingDamage = value;

        if (!invulnerable)
        {
            // Check if shields should be ignored
            if (ignoreShields || kill)
            {
                // Directly reduce health if ignoring shields
                health += (int)remainingDamage;
            }
            else
            {
                // Shield reduction logic if not ignoring shields
                if (currentShields.Count > 0 && !kill && remainingDamage < 0)
                {
                    for (int i = currentShields.Count - 1; i >= 0; i--)
                    {
                        if (remainingDamage >= 0)
                        {
                            break;
                        }

                        if (currentShields[i].value > Mathf.Abs(remainingDamage))
                        {
                            currentShields[i] = new Shield(currentShields[i].key, currentShields[i].value + remainingDamage);
                            remainingDamage = 0;
                        }
                        else
                        {
                            remainingDamage += currentShields[i].value;
                            RemoveEffect(currentShields[i].key, false);
                            if (i < (currentShields.Count - 1))
                                currentShields.RemoveAt(i);
                        }
                    }
                }

                // Reduce health if any damage remains after shield reduction
                if (remainingDamage != 0)
                {
                    health += (int)remainingDamage;
                }
            }

            // Reset shields if 'kill' flag is active
            if (kill)
            {
                shield -= shield;
            }
        }
        //

        if (health <= 0 && !invulnerable)
        {
            if (canDie)
            {
                health = 0;
                dead = true;
                onDeath.Invoke();
                if (effectsArray != null)
                {
                    Dictionary<string, StatusEffect> temp = new Dictionary<string, StatusEffect>();

                    for (int i = 0; i < effectsArray.Length; i++)
                    {
                        if (effectsArray[i].data.lostOnDeath)
                        {
                            effectsArray[i].onExpire.Invoke(this);
                            effectsArray[i].Remove();
                            if (showDamagePopups)
                                ShowStatusEffectFlyText(effectsArray[i], " - ");
                        }
                        else if (!temp.ContainsKey(effectsArray[i].data.statusName))
                        {
                            temp.Add(effectsArray[i].data.statusName, effectsArray[i]);
                        }
                    }
                    effects = new Dictionary<string, StatusEffect>(temp);
                    effectsArray = effects.Values.ToArray();
                }
            }
            else
            {
                health = 1;
                dead = false;
            }
        }
        if (health > currentMaxHealth)
        {
            health = currentMaxHealth;
        }

        RecalculateCurrentShield();
        //HealthBarUserInterface();
    }

    public void AddShield(long value, string identifier, bool showPopup = false)
    {
        if (!currentShields.ContainsKey(identifier))
        {
            currentShields.Add(new Shield(identifier, value));

            if (showDamagePopups && showPopup)
                ShowDamageFlyText(new Damage(value, false, true, DamageType.magical, ElementalAspect.unaspected, PhysicalAspect.none, DamageApplicationType.normal, Utilities.InsertSpaceBeforeCapitals(identifier)));
        }
        else
        {
            currentShields.RemoveKey(identifier);
            currentShields.Add(new Shield(identifier, value));
        }

        RecalculateCurrentShield();
    }

    public void RemoveShield(string identifier)
    {
        if (currentShields.ContainsKey(identifier))
        {
            currentShields.RemoveKey(identifier);
        }
        else
        {
            return;
        }

        RecalculateCurrentShield();
    }

    private void RecalculateCurrentShield()
    {
        long result = 0;

        if (currentShields.Count > 0)
        {
            currentShields.Sort((x, y) => y.value.CompareTo(x.value));
            for (int i = 0; i < currentShields.Count; i++)
            {
                result += currentShields[i].value;
            }
        }

        shield = result;

        HealthBarUserInterface();
    }

    private void HealthBarUserInterface()
    {
        // USER INTERFACE
        if (healthBar != null)
        {
            healthBar.value = health;
            healthBar.maxValue = currentMaxHealth;
        }
        if (nameplateHealthBar != null)
        {
            nameplateHealthBar.maxValue = currentMaxHealth;
            nameplateHealthBar.value = health;

            if (nameplateHealthBarGroup != null)
            {
                if (showNameplateHealthBar)
                {
                    if (showOnlyBelowMaxHealth && health >= currentMaxHealth)
                    {
                        nameplateHealthBarGroup.alpha = 0f;
                    }
                    else
                    {
                        nameplateHealthBarGroup.alpha = 1f;
                    }
                }
                else
                {
                    nameplateHealthBarGroup.alpha = 0f;
                }
            }
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
                    string result = healthPercentage.ToString("F1") + "%";

                    if (health > 0)
                    {
                        if (result == "0%" || result == "0.0%" || result == "0.00%" || result == "00.0%" || result == "00.00%" || result == "0,0%" || result == "0,00%" || result == "00,0%" || result == "00,00%")
                        {
                            result = "0.1%";
                        }
                    }

                    healthBarText.text = result;
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
            if (shieldBarParty != null)
            {
                shieldBarParty.value = health + shield;
                shieldBarParty.maxValue = currentMaxHealth;
            }
            if (overShieldBarParty != null)
            {
                overShieldBarParty.value = health + shield;
                overShieldBarParty.minValue = currentMaxHealth;
                overShieldBarParty.maxValue = currentMaxHealth + currentMaxHealth;
            }
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

    public void AddDamageOutputModifier(float value, string identifier)
    {
        if (!damageOutputModifiers.ContainsKey(identifier))
        {
            damageOutputModifiers.Add(identifier, value);
        }
        else
        {
            return;
        }

        RecalculateDamageOutputModifiers();
    }

    public void RemoveDamageOutputModifier(string identifier)
    {
        if (damageOutputModifiers.ContainsKey(identifier))
        {
            damageOutputModifiers.Remove(identifier);
        }
        else
        {
            return;
        }

        RecalculateDamageOutputModifiers();
    }

    private void RecalculateDamageOutputModifiers()
    {
        float result = 1f;

        List<float> mList = new List<float>(damageOutputModifiers.Values.ToArray());

        if (damageOutputModifiers.Count > 0)
        {
            mList.Sort();
            for (int i = 0; i < damageOutputModifiers.Count; i++)
            {
                result *= mList[i];
            }
        }

        currentDamageOutputMultiplier = result;
    }

    public void AddEnmityGenerationModifier(float value, string identifier)
    {
        if (!canGainEnmity)
            return;

        if (!enmityGenerationModifiers.ContainsKey(identifier))
        {
            enmityGenerationModifiers.Add(identifier, value);
        }
        else
        {
            return;
        }

        RecalculateEnmityGenerationModifier();
    }

    public void RemoveEnmityGenerationModifier(string identifier)
    {
        if (!canGainEnmity)
            return;

        if (enmityGenerationModifiers.ContainsKey(identifier))
        {
            enmityGenerationModifiers.Remove(identifier);
        }
        else
        {
            return;
        }

        RecalculateEnmityGenerationModifier();
    }

    private void RecalculateEnmityGenerationModifier()
    {
        if (!canGainEnmity)
        {
            enmityGenerationModifier = 0f;
            return;
        }

        float result = 1f;

        List<float> mList = new List<float>(enmityGenerationModifiers.Values.ToArray());

        if (enmityGenerationModifiers.Count > 0)
        {
            mList.Sort();
            for (int i = 0; i < enmityGenerationModifiers.Count; i++)
            {
                result *= mList[i];
            }
        }

        enmityGenerationModifier = result;
    }

    public void AddDamageModifier(float value, string identifier, bool poison)
    {
        bool update = false;

        if (poison)
        {
            if (!poisonDamageModifiers.ContainsKey(identifier))
            {
                poisonDamageModifiers.Add(identifier, value);
                update = true;
            }
        }

        if (!update)
        {
            return;
        }

        RecalculateCurrentDamageModifiers(poison);
    }

    public void RemoveDamageModifier(string identifier, bool poison)
    {
        bool update = false;

        if (poison)
        {
            if (poisonDamageModifiers.ContainsKey(identifier))
            {
                poisonDamageModifiers.Remove(identifier);
                update = true;
            }
        }

        if (!update)
        {
            return;
        }

        RecalculateCurrentDamageModifiers(poison);
    }

    public void RecalculateCurrentDamageModifiers(bool poison)
    {
        if (poison)
        {
            float result = 1f;

            List<float> mList = new List<float>(poisonDamageModifiers.Values.ToArray());

            if (poisonDamageModifiers.Count > 0)
            {
                mList.Sort();
                for (int i = 0; i < poisonDamageModifiers.Count; i++)
                {
                    result *= mList[i];
                }
            }

            poisonDamageModifier = result;
        }
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
        float result = defaultMaxHealth;

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

        if (health > currentMaxHealth)
        {
            health = currentMaxHealth;
        }

        if (healthBar != null)
        {
            healthBar.value = health;
            healthBar.maxValue = currentMaxHealth;
        }
        if (healthBarParty != null)
        {
            healthBarParty.value = health;
            healthBarParty.maxValue = currentMaxHealth;
            if (shieldBarParty != null)
            {
                shieldBarParty.value = health + shield;
                shieldBarParty.maxValue = currentMaxHealth;
            }
            if (overShieldBarParty != null)
            {
                overShieldBarParty.value = health + shield;
                overShieldBarParty.minValue = currentMaxHealth;
                overShieldBarParty.maxValue = currentMaxHealth + currentMaxHealth;
            }
        }

        HealthBarUserInterface();
    }

    public void AddEnmity(long value, CharacterState character)
    {
        if (!canGainEnmity)
            return;

        if (value < 0)
            return;

        if (character == null)
            return;

        if (enmity.ContainsKey(character))
        {
            enmity[character] += value;
        }
        else
        {
            enmity.Add(character, value);
        }
#if UNITY_EDITOR
        m_enmity.Clear();
        for (int i = 0; i < enmity.Count; i++)
        {
            m_enmity.Add(new Enmity(enmity.Keys.ToArray()[i], enmity.Values.ToArray()[i]));
        }
#endif
    }

    public void SetEnmity(long value, CharacterState character)
    {
        if (!canGainEnmity)
            return;

        if (value < 0)
            return;

        if (character == null)
            return;

        if (enmity.ContainsKey(character))
        {
            enmity[character] = value;

            if (enmity[character] <= 0)
            {
                ResetEnmity(character);
            }
        }
        else
        {
            enmity.Add(character, value);
        }
#if UNITY_EDITOR
        m_enmity.Clear();
        for (int i = 0; i < enmity.Count; i++)
        {
            m_enmity.Add(new Enmity(enmity.Keys.ToArray()[i], enmity.Values.ToArray()[i]));
        }
#endif
    }

    public void RemoveEnmity(long value, CharacterState character)
    {
        if (!canGainEnmity)
            return;

        if (value < 0)
            return;

        if (character == null)
            return;

        if (enmity.ContainsKey(character))
        {
            enmity[character] -= value;

            if (enmity[character] <= 0)
            {
                ResetEnmity(character);
            }
        }
#if UNITY_EDITOR
        m_enmity.Clear();
        for (int i = 0; i < enmity.Count; i++)
        {
            m_enmity.Add(new Enmity(enmity.Keys.ToArray()[i], enmity.Values.ToArray()[i]));
        }
#endif
    }

    public void ResetEnmity(CharacterState character)
    {
        if (!canGainEnmity)
            return;

        if (character == null)
            return;

        if (enmity.ContainsKey(character))
        {
            enmity.Remove(character);
        }
#if UNITY_EDITOR
        m_enmity.Clear();
        for (int i = 0; i < enmity.Count; i++)
        {
            m_enmity.Add(new Enmity(enmity.Keys.ToArray()[i], enmity.Values.ToArray()[i]));
        }
#endif
    }

    public void AddEffect(StatusEffectData data, bool self = false, int tag = 0, int stacks = 0)
    {
        AddEffect(data, null, self, tag, stacks);
    }

    public void AddEffect(StatusEffectData data, Damage? damage, bool self = false, int tag = 0, int stacks = 0)
    {
        if (data.statusEffect == null)
            return;

        if (!gameObject.activeSelf)
            return;

        if (health <= 0 || dead)
            return;

        string name = string.Empty;
        bool refreshed = false;

        if (data.refreshStatusEffects != null && data.refreshStatusEffects.Count > 0)
        {
            for (int i = 0; i < data.refreshStatusEffects.Count; i++)
            {
                name = data.refreshStatusEffects[i].statusName;

                if (effects.ContainsKey(name))
                {
                    if (!data.refreshStatusEffects[i].unique)
                        effects[name].Refresh(stacks, 0, -1);
                    refreshed = true;
                }
            }
        }

        name = data.statusName;

        if (tag > 0)
        {
            name = $"{data.statusName}_{tag}";
        }

        if (effects.ContainsKey(name))
        {
            if (!data.unique)
                effects[name].Refresh(stacks);
            refreshed = true;
        }

        if (refreshed)
            return;

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
        StatusEffect effect = newStatusEffect.GetComponent<StatusEffect>();

        if (damage != null)
        {
            effect.damage = new Damage((Damage)damage);
        }

        AddEffect(effect, damage, self, tag, stacks);
    }

    public void AddEffect(string name, bool self = false, int tag = 0, int stacks = 0)
    {
        AddEffect(name, null, self, tag);
    }

    public void AddEffect(string name, Damage? damage, bool self = false, int tag = 0, int stacks = 0)
    {
        if (!gameObject.activeSelf)
            return;

        if (health <= 0 || dead)
            return;

        string m_name = string.Empty;
        bool refreshed = false;

        if (effects[name].data.refreshStatusEffects != null && effects[name].data.refreshStatusEffects.Count > 0)
        {
            for (int i = 0; i < effects[name].data.refreshStatusEffects.Count; i++)
            {
                m_name = effects[name].data.refreshStatusEffects[i].statusName;

                if (effects.ContainsKey(m_name))
                {
                    if (!effects[m_name].data.refreshStatusEffects[i].unique)
                        effects[m_name].Refresh(stacks, 0, -1);
                    refreshed = true;
                }
            }
        }

        if (tag > 0)
        {
            name = $"{name}_{tag}";
        }

        if (effects.ContainsKey(name))
        {
            if (!effects[name].data.unique)
                effects[name].Refresh(stacks);
            refreshed = true;
        }

        if (refreshed)
            return;

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
                StatusEffect effect = newStatusEffect.GetComponent<StatusEffect>();

                if (damage != null)
                {
                    effect.damage = new Damage((Damage)damage);
                }

                AddEffect(effect, damage, self, tag);
            }
        }
    }

    public void AddEffect(StatusEffect effect, bool self = false, int tag = 0, int stacks = 0)
    {
        AddEffect(effect, null, self, tag);
    }

    public void AddEffect(StatusEffect effect, Damage? damage, bool self = false, int tag = 0, int stacks = 0)
    {
        //if (effect.data.name.Contains("Cleaned"))
        //    Debug.LogError("Cleaned detected");

        if (!gameObject.activeSelf)
            return;

        if (health <= 0 || dead)
            return;

        if (damage != null)
        {
            effect.damage = new Damage((Damage)damage);
        }

        string name = string.Empty;
        bool refreshed = false;

        if (effect.data.refreshStatusEffects != null && effect.data.refreshStatusEffects.Count > 0)
        {
            for (int i = 0; i < effect.data.refreshStatusEffects.Count; i++)
            {
                name = effect.data.refreshStatusEffects[i].statusName;

                if (effects.ContainsKey(name))
                {
                    if (!effect.data.refreshStatusEffects[i].unique)
                        effects[name].Refresh(stacks, 0, -1);
                    refreshed = true;
                }
            }
        }

        name = effect.data.statusName;

        if (tag > 0)
        {
            name = $"{effect.data.statusName}_{tag}";
        }

        if (effects.ContainsKey(name))
        {
            if (!effect.data.unique)
                effects[name].Refresh(stacks);
            refreshed = true;
        }

        if (refreshed)
            return;

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

        if (showDamagePopups)
        {
            ShowStatusEffectFlyText(effect, stacks + effect.data.appliedStacks, " + ");
        }

        if (invulnerable)
        {
            return;
        }

        effect.uniqueTag = tag;
        effect.stacks = stacks;
        effects.Add(name, effect);
#if UNITY_EDITOR
        m_effects.Add(new StatusEffectPair(name, effect));
#endif
        effectsArray = effects.Values.ToArray();

        if (effect.data.instantCasts)
        {
            if (!instantCasts.Contains(effect))
            {
                instantCasts.Add(effect);
                instantCasts.Sort((x, y) => x.sortOrder.CompareTo(y.sortOrder));
                onInstantCastsChanged.Invoke(instantCasts);
            }
        }

        if (showStatusEffectsWhenTargeted && targetStatusEffectHolderPrefab != null && targetStatusEffectHolderParent != null && targetStatusEffectIconParent == null)
        {
            GameObject newStatusEffectHolder = Instantiate(targetStatusEffectHolderPrefab, targetStatusEffectHolderParent);
            if (newStatusEffectHolder.TryGetComponent(out HudElement result))
            {
                result.characterState = this;
            }
            if (newStatusEffectHolder.TryGetComponent(out CanvasGroup group))
            {
                targetStatusEffectIconGroup = group;
                targetStatusEffectIconGroup.alpha = 0f;
            }
            newStatusEffectHolder.name = newStatusEffectHolder.name.Replace("(Clone)", "").Replace("Target_", $"{characterName.Replace(" ","_").Replace('#', '_')}_");
            targetStatusEffectIconParent = newStatusEffectHolder.transform;
        }
        else if (targetStatusEffectIconParent == null)
        {
            targetStatusEffectIconParent = GameObject.Find("Temp_RectTransform").transform;
        }

        if (effect.data.negative)
        {
            if (self)
            {
                effect.Initialize(statusEffectNegativeIconParent, statusEffectIconParentParty, targetStatusEffectIconParent, ownStatusEffectColor);
            }
            else
            {
                effect.Initialize(statusEffectNegativeIconParent, statusEffectIconParentParty, targetStatusEffectIconParent, otherStatusEffectColor);
            }
        }
        else
        {
            if (self)
            {
                effect.Initialize(statusEffectPositiveIconParent, statusEffectIconParentParty, targetStatusEffectIconParent, ownStatusEffectColor);
            }
            else
            {
                effect.Initialize(statusEffectPositiveIconParent, statusEffectIconParentParty, targetStatusEffectIconParent, otherStatusEffectColor);
            }
        }
        effect.onApplication.Invoke(this);

        if (partyList != null)
            partyList.UpdatePrioritySorting();
    }

    public void RemoveEffect(StatusEffectData data, bool expired, int tag = 0, int stacks = 0)
    {
        RemoveEffect(data.statusName, expired, tag, stacks);
    }

    public void RemoveEffect(StatusEffect effect, bool expired, int tag = 0, int stacks = 0)
    {
        RemoveEffect(effect.data.statusName, expired, tag, stacks);
    }

    public void RemoveEffect(string name, bool expired, int tag = 0, int stacks = 0)
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
                RemoveEffect(name, expired, i + 1, stacks);
            }
            return;
        }

        if (!effects.ContainsKey(name))
            return;

        StatusEffect temp = effects[name];

        if (showDamagePopups && (effects[name].stacks <= 1 || effects[name].stacks <= stacks))
        {
            ShowStatusEffectFlyText(temp, " - ");
        }
        else
        {
            ShowStatusEffectFlyText(temp, 1, " - ");
        }

        if (invulnerable)
            return;

        if (effects[name].stacks <= 1 || effects[name].stacks <= stacks)
        {
            if (expired)
                effects[name].onExpire.Invoke(this);
            else
                effects[name].onCleanse.Invoke(this);

            effects.Remove(name);
#if UNITY_EDITOR
            for (int i = 0; i < m_effects.Count; i++)
            {
                if (m_effects[i].name == name)
                {
                    m_effects.RemoveAt(i);
                }
            }
#endif
            effectsArray = effects.Values.ToArray();

            if (temp.data.instantCasts)
            {
                if (instantCasts.Contains(temp))
                {
                    instantCasts.Remove(temp);
                    instantCasts.Sort((x, y) => x.sortOrder.CompareTo(y.sortOrder));
                    onInstantCastsChanged.Invoke(instantCasts);
                }
            }

            if (temp.data.refreshStatusEffects != null && temp.data.refreshStatusEffects.Count > 0)
            {
                for (int i = 0; i < temp.data.refreshStatusEffects.Count; i++)
                {
                    string m_name = temp.data.refreshStatusEffects[i].statusName;

                    if (effects.ContainsKey(m_name))
                    {
                        Debug.Log($"Removed sub status {m_name} with a index {i} of {temp.data.refreshStatusEffects.Count} total from {temp.data.statusName}");
                        RemoveEffect(m_name, expired, tag, stacks);
                    }
                }
            }

            temp.Remove();
        }
        else
        {
            if (stacks > 1)
            {
                effects[name].stacks -= stacks;
            }
            else
            {
                effects[name].stacks -= 1;
            }
            effects[name].onReduce.Invoke(this);
        }

        if (partyList != null)
            partyList.UpdatePrioritySorting();
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

    public void ShowStatusEffectFlyText(StatusEffect statusEffect, int stacks, string prefix)
    {
        ShowStatusEffectFlyText(statusEffect.data, stacks, prefix);
    }

    public void ShowStatusEffectFlyText(StatusEffect statusEffect, string prefix)
    {
        ShowStatusEffectFlyText(statusEffect.data, statusEffect.stacks, prefix);
    }

    public void ShowStatusEffectFlyText(StatusEffectData data, int stacks, string prefix)
    {
        if (data.hidden) 
            return;

        // Hard coded the Short (@s) and Long (@l) that are used to distinguish between few of the same debuffs,
        // also the '#' character which is used for non capitalised letter sequences. This needs a better implementation.
        string result = $"{prefix}{Utilities.InsertSpaceBeforeCapitals(data.statusName).Replace("@s","").Replace("@l","").Replace("#"," ").Replace("@gd", "").Replace("@gs", "")}";

        Color color = neutralPopupColor;

        if (prefix.Contains('+'))
        {
            if (data.negative)
            {
                color = negativePopupColor;
            }
            else
            {
                color = positivePopupColor;
            }
        }

        if (invulnerable)
        {
            result = $"{result} Has No Effect";
            color = neutralPopupColor;
        }

        Sprite icon;
        int iconIndex = stacks - 1;
        if (data.icons != null && data.icons.Count > 0 && data.icons.Count >= iconIndex && iconIndex > -1)
        {
            icon = data.icons[iconIndex];
        }
        else
        {
            icon = data.hudElement.transform.GetChild(0).GetComponent<Image>().sprite;
        }

        ShowFlyText(new FlyText(result, color, icon, string.Empty));
    }

    public void ShowDamageFlyText(Damage damage)
    {
        string result = string.Empty;

        Damage finalDamage = new Damage(damage);

        if (!damage.ignoreDamageReductions)
        {
            // Apply damage reduction
            finalDamage = new Damage(damage, (long)Math.Round(damage.value * currentDamageReduction));
        }

        if (showElementalAspect && finalDamage.elementalAspect != ElementalAspect.none && finalDamage.elementalAspect != ElementalAspect.unaspected)
        {
            if (finalDamage.value < 0 || finalDamage.value > 0)
            {
                result = $"<sprite=\"damage_types\" name=\"{(int)finalDamage.type}\"><sprite=\"damage_types\" name=\"{finalDamage.elementalAspect.ToString("g")}\">{Mathf.Abs(finalDamage.value)}";
            }
            else
            {
                result = $"<sprite=\"damage_types\" name=\"{(int)finalDamage.type}\"><sprite=\"damage_types\" name=\"{finalDamage.elementalAspect.ToString("g")}\">{finalDamage.value}";
            }
        } 
        else
        {
            if (finalDamage.value < 0 || finalDamage.value > 0)
            {
                result = $"<sprite=\"damage_types\" name=\"{(int)finalDamage.type}\">{Mathf.Abs(finalDamage.value)}";
            }
            else
            {
                result = $"<sprite=\"damage_types\" name=\"{(int)finalDamage.type}\">{finalDamage.value}";
            }
        }

        string source = finalDamage.name;

        Color color = positivePopupColor;

        if (finalDamage.negative || finalDamage.value < 0)
        {
            color = negativePopupColor;
        }
        else if (!finalDamage.negative)
        {
            if (finalDamage.value < 0 || finalDamage.value > 0)
            {
                result = $"<sprite=\"damage_types\" name=\"0\">{Mathf.Abs(finalDamage.value)}";
            }
            else
            {
                result = $"<sprite=\"damage_types\" name=\"0\">{finalDamage.value}";
            }
        }

        if (invulnerable)
        {
            result = "Invulnerable";
            color = neutralPopupColor;
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

        /*if (characterNameTextGroup != null)
        {
            if (!hideNameplate && showCharacterName)
            {
                characterNameTextGroup.alpha = 1f;
            }
            else
            {
                characterNameTextGroup.alpha = 0f;
            }
        }*/
        if (nameplateCharacterNameTextGroup != null)
        {
            if (showCharacterName)
            {
                nameplateCharacterNameTextGroup.alpha = 1f;
            }
            else
            {
                nameplateCharacterNameTextGroup.alpha = 0f;
            }
        }
        if (characterNameTextGroupParty != null)
        {
            if (!hidePartyName && showPartyCharacterName)
            {
                characterNameTextGroupParty.alpha = 1f;
            }
            else
            {
                characterNameTextGroupParty.alpha = 0f;
            }
        }
    }

    [System.Serializable]
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

    [System.Serializable]
    public struct Shield
    {
        public string key;
        public long value;

        public Shield(string key, long value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
