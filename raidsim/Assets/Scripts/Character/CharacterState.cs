using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static GlobalData;
using static GlobalData.Damage;
using static GlobalData.Flag;
using static PartyList;

public class CharacterState : MonoBehaviour
{
    [Hidden]
    public PlayerController playerController;
    [Hidden]
    public AIController aiController;
    [Hidden]
    public BossController bossController;
    [Hidden]
    public TargetController targetController;
    [Hidden]
    public ActionController actionController;
    [Hidden]
    public Transform dashKnockbackPivot;

    #region Stat Variables
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
    #endregion

    [Header("States")]
    public Flag invulnerable = new Flag("invulnerable", AggregateLogic.AnyTrue);
    private Flag wasInvulnerable;
    public Flag uncontrollable = new Flag("uncontrollable", AggregateLogic.AnyTrue);
    private Flag wasUncontrollable;
    public Flag untargetable = new Flag("untargetable", AggregateLogic.AnyTrue);
    private Flag wasUntargetable;
    public Flag bound = new Flag("bound", AggregateLogic.AnyTrue);
    private Flag wasBound;
    public Flag stunned = new Flag("stunned", AggregateLogic.AnyTrue);
    private Flag wasStunned;
    public Flag knockbackResistant = new Flag("knockbackResistant", AggregateLogic.AnyTrue);
    private Flag wasKnockbackResistant;
    public Flag silenced = new Flag("silenced", AggregateLogic.AnyTrue);
    private Flag wasSilenced;
    public Flag pacificied = new Flag("pacificied", AggregateLogic.AnyTrue);
    private Flag wasPacified;
    public Flag amnesia = new Flag("amnesia", AggregateLogic.AnyTrue);
    private Flag wasAmnesia;
    public Flag canDoActions = new Flag("canDoActions", new List<FlagValue> { new FlagValue("base", true) });
    private Flag wasCanDoActions;
    public Flag canDie = new Flag("canDie", new List<FlagValue> { new FlagValue("base", true) }, AggregateLogic.AnyTrue);
    private Flag wasCanDie;
    public bool dead = false;
    public bool still = false;
    public bool disabled = false;
    private bool wasDisabled = false;
    public bool canGainEnmity = true;
    private bool preventDamage = false;

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
    public UnityEvent<CharacterState> onModifyHealth;

    [Header("Config")]
    public CharacterState mirror;
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
    public Sector sector = Sector.N;
    public bool isAggressive = true;
    private bool wasIsAggressive;
    public bool hideNameplate = false;
    private bool wasHideNameplate;
    public bool hidePartyName = false;
    private bool wasHidePartyName;
    public bool hidePartyListEntry = false;
    private bool wasHidePartyListEntry;

    #region User Interface Variables
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
    [Header("Personal - Signs")]
    public bool showSignMarkers = true;
    public bool showDeadMarker = true;
    public CanvasGroup signMarkersGroup;
    public List<SignMarker> signMarkers = new List<SignMarker>();
    public GameObject deadMarker;

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

    [Header("Party - Status Popups")]
    public bool showStatusPopups = true;
    public GameObject statusPopupPrefab;
    public Transform statusPopupParent;
    public Color statusPopupColor = Color.blue;
    public float statusPopupTextDelay = 0.5f;
    public Transform statusPopupPivot;
    private Queue<FlyText> statusPopupTexts = new Queue<FlyText>();
    private Coroutine statusPopupCoroutine;

    [Header("Target - StatusEffects")]
    public bool showStatusEffectsWhenTargeted = true;
    public GameObject targetStatusEffectHolderPrefab;
    public Transform targetStatusEffectHolderParent;
    public CanvasGroup targetStatusEffectIconGroup;
    public Transform targetStatusEffectIconParent;

    [Header("Target - Damage Popups")]
    public bool showTargetDamagePopups = true;
    public GameObject targetDamagePopupPrefab;
    public Transform targetDamagePopupParent;
    public Color targetDamagePopupColor = Color.white;
    public CharacterState showOnlyFromCharacter;

    private Coroutine ieUpdatePartyList;
    private Coroutine ieStartSetupDelayed;
    private GameObject dfUpdatePartyList;
    #endregion

#if UNITY_EDITOR
    [Header("Editor")]
    public StatusEffectData.StatusEffectInfo inflictedEffect;
    public Damage inflictedDamage;
    [Button("Inflict Status Effect")]
    public void InflictStatusEffectButton()
    {
        AddEffect(inflictedEffect.data, this, true, inflictedEffect.tag, inflictedEffect.stacks);
    }
    [Button("Cleanse Status Effect")]
    public void CleanseStatusEffectButton()
    {
        RemoveEffect(inflictedEffect.data, true, this, inflictedEffect.tag, inflictedEffect.stacks);
    }
    [Button("Inflict Damage")]
    public void InflictDamageButton()
    {
        ModifyHealth(inflictedDamage);
    }

    [Button("Print Flags")]
    public void PrintFlags()
    {
        string charName = string.IsNullOrEmpty(characterName) ? "Unknown" : characterName.Replace(" ", "_");
        Debug.Log($"[CharacterState.{charName} ({gameObject.name})] invulnerable: {invulnerable.value} values.Count: {invulnerable.values.Count}");
        Debug.Log($"[CharacterState.{charName} ({gameObject.name})] uncontrollable: {uncontrollable.value} values.Count: {uncontrollable.values.Count}");
        Debug.Log($"[CharacterState.{charName} ({gameObject.name})] untargetable: {untargetable.value} values.Count: {untargetable.values.Count}");
        Debug.Log($"[CharacterState.{charName} ({gameObject.name})] bound: {bound.value} values.Count: {bound.values.Count}");
        Debug.Log($"[CharacterState.{charName} ({gameObject.name})] stunned: {stunned.value} values.Count: {stunned.values.Count}");
        Debug.Log($"[CharacterState.{charName} ({gameObject.name})] knockbackResistant: {knockbackResistant.value} values.Count: {knockbackResistant.values.Count}");
        Debug.Log($"[CharacterState.{charName} ({gameObject.name})] silenced: {silenced.value} values.Count: {silenced.values.Count}");
        Debug.Log($"[CharacterState.{charName} ({gameObject.name})] pacificied: {pacificied.value} values.Count: {pacificied.values.Count}");
        Debug.Log($"[CharacterState.{charName} ({gameObject.name})] amnesia: {amnesia.value} values.Count: {amnesia.values.Count}");
        Debug.Log($"[CharacterState.{charName} ({gameObject.name})] canDoActions: {canDoActions.value} values.Count: {canDoActions.values.Count}");
        Debug.Log($"[CharacterState.{charName} ({gameObject.name})] canDie: {canDie.value} values.Count: {canDie.values.Count}");
    }
#endif

    #region Custom Setters and Getters
    public string GetCharacterName()
    {
        int index = characterName.IndexOf('#');
        if (index >= 0) // Check if '#' exists in the string
        {
            return characterName.Substring(0, index);
        }
        return characterName; // Return the full string if no '#' is found
    }

    public void UpdateCharacterName()
    {
        if (!gameObject.activeSelf)
            return;

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

    public void SetAnimator(Animator animator)
    {
        if (bossController != null)
            bossController.SetAnimator(animator);
        if (playerController != null)
            playerController.SetAnimator(animator);
        if (aiController != null)
            aiController.SetAnimator(animator);
        if (actionController != null)
            actionController.SetAnimator(animator);
    }
    #endregion

    #region BuiltIn Unity Functions
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        aiController = GetComponent<AIController>();
        bossController = GetComponent<BossController>();
        targetController = GetComponent<TargetController>();
        actionController = GetComponent<ActionController>();
        TaggedObject[] taggedObjects = transform.Find("Pivot")?.GetComponentsInChildren<TaggedObject>();

        if (taggedObjects != null && taggedObjects.Length > 0)
        {
            foreach (TaggedObject tagged in taggedObjects)
            {
                if (tagged.m_tag == "dashPivot")
                {
                    dashKnockbackPivot = tagged.transform;
                    break;
                }
            }
        }

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

        wasDisabled = disabled;
        wasHideNameplate = hideNameplate;
        wasHidePartyName = hidePartyName;
        wasHidePartyListEntry = hidePartyListEntry;
        wasIsAggressive = isAggressive;
        currentMaxHealth = health;
        defaultMaxHealth = health;
        defaultSpeed = currentSpeed;
        dead = false;
        invulnerable.ForceUpdate();
        wasInvulnerable = new Flag(invulnerable);
        uncontrollable.ForceUpdate();
        wasUncontrollable = new Flag(uncontrollable);
        untargetable.ForceUpdate();
        wasUntargetable = new Flag(untargetable);
        bound.ForceUpdate();
        wasBound = new Flag(bound);
        stunned.ForceUpdate();
        wasStunned = new Flag(stunned);
        knockbackResistant.ForceUpdate();
        wasKnockbackResistant = new Flag(knockbackResistant);
        silenced.ForceUpdate();
        wasSilenced = new Flag(silenced);
        pacificied.ForceUpdate();
        wasPacified = new Flag(pacificied);
        amnesia.ForceUpdate();
        wasAmnesia = new Flag(amnesia);
        canDoActions.ForceUpdate();
        wasCanDoActions = new Flag(canDoActions);
        canDie.ForceUpdate();
        wasCanDie = new Flag(canDie);
        enmity = new Dictionary<CharacterState, long>();
        preventDamage = false;

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
        if (signMarkersGroup != null)
        {
            signMarkers = new List<SignMarker>();
            signMarkers.AddRange(signMarkersGroup.GetComponentsInChildren<SignMarker>());

            if (showSignMarkers)
            {
                signMarkersGroup.alpha = 1f;
            }
            else
            {
                signMarkersGroup.alpha = 0f;
            }
        }

        if (statusPopupPivot == null)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.name.ToLower().Contains("pivot"))
                {
                    statusPopupPivot = child;
                }
            }
        }
    }

    void Start()
    {
        if (ieStartSetupDelayed == null)
        {
            ieStartSetupDelayed = StartCoroutine(IE_StartSetupDelayed(new WaitForSecondsRealtime(0.1f), new WaitForSecondsRealtime(0.2f)));
        }
    }

    void OnEnable()
    {
        if (partyList != null && ieUpdatePartyList == null)
        {
            ieUpdatePartyList = StartCoroutine(IE_PartyListUpdate(new WaitForSecondsRealtime(0.2f)));
        }
    }

    void OnDisable()
    {
        if (partyList != null && gameObject.scene.isLoaded) // ieUpdatePartyList == null &&
        {
            if (dfUpdatePartyList == null)
                dfUpdatePartyList = new GameObject($"{gameObject.name}_OnDisable_PartyListUpdate", typeof(DelayedFunction));

            if (dfUpdatePartyList.TryGetComponent(out DelayedFunction df))
            {
                df.Trigger(() => { partyList.UpdatePartyList(); }, new WaitForSecondsRealtime(0.2f));
            }
            //ieUpdatePartyList = StartCoroutine(IE_PartyListUpdate(new WaitForSecondsRealtime(1f)));
        }
    }

    private IEnumerator IE_PartyListUpdate(WaitForSecondsRealtime wait)
    {
        yield return wait;
        if (this == null || !gameObject.scene.isLoaded)
            yield break;
        partyList.UpdatePartyList();
        ieUpdatePartyList = null;
    }

    private IEnumerator IE_StartSetupDelayed(WaitForSecondsRealtime wait1, WaitForSecondsRealtime wait2)
    {
        yield return wait1;
        if (this == null || !gameObject.scene.isLoaded)
            yield break;
        onSpawn?.Invoke();
        yield return wait2;
        if (disabled)
            gameObject.SetActive(false);
        ieStartSetupDelayed = null;
    }

    void Update()
    {
        if (!gameObject.activeSelf)
            return;

        if (Mathf.Abs(transform.position.y) >= GlobalVariables.worldBounds.y || Mathf.Abs(transform.position.x) >= GlobalVariables.worldBounds.x || Mathf.Abs(transform.position.z) >= GlobalVariables.worldBounds.z)
        {
            ModifyHealth(new Damage(100, true, true, DamageType.unique, ElementalAspect.unaspected, PhysicalAspect.none, DamageApplicationType.percentageFromMax, "Out of bounds"));
            transform.position = new Vector3(0f, 1f, 0f);
        }

        if (mirror != null)
        {
            health = mirror.health;
            defaultMaxHealth = mirror.defaultMaxHealth;
            currentMaxHealth = mirror.currentMaxHealth;
            maxHealthModifiers = mirror.maxHealthModifiers;
            currentShields = mirror.currentShields;
            shield = mirror.shield;
            //currentSpeed = mirror.currentSpeed;
            //speedModifiers = mirror.speedModifiers;
            //speed = mirror.speed;
            currentDamageOutputMultiplier = mirror.currentDamageOutputMultiplier;
            damageOutputModifiers = mirror.damageOutputModifiers;
            currentDamageReduction = mirror.currentDamageReduction;
            damageReduction = mirror.damageReduction;
            magicalTypeDamageModifiers = mirror.magicalTypeDamageModifiers;
            magicalTypeDamageModifier = mirror.magicalTypeDamageModifier;
            physicalTypeDamageModifiers = mirror.physicalTypeDamageModifiers;
            physicalTypeDamageModifier = mirror.physicalTypeDamageModifier;
            uniqueTypeDamageModifiers = mirror.uniqueTypeDamageModifiers;
            uniqueTypeDamageModifier = mirror.uniqueTypeDamageModifier;
            unaspectedElementDamageModifiers = mirror.unaspectedElementDamageModifiers;
            unaspectedElementDamageModifier = mirror.unaspectedElementDamageModifier;
            fireElementDamageModifiers = mirror.fireElementDamageModifiers;
            fireElementDamageModifier = mirror.fireElementDamageModifier;
            iceElementDamageModifiers = mirror.iceElementDamageModifiers;
            iceElementDamageModifier = mirror.iceElementDamageModifier;
            lightningElementDamageModifiers = mirror.lightningElementDamageModifiers;
            lightningElementDamageModifier = mirror.lightningElementDamageModifier;
            waterElementDamageModifiers = mirror.waterElementDamageModifiers;
            waterElementDamageModifier = mirror.waterElementDamageModifier;
            windElementDamageModifiers = mirror.windElementDamageModifiers;
            windElementDamageModifier = mirror.windElementDamageModifier;
            earthElementDamageModifiers = mirror.earthElementDamageModifiers;
            earthElementDamageModifier = mirror.earthElementDamageModifier;
            darkElementDamageModifiers = mirror.darkElementDamageModifiers;
            darkElementDamageModifier = mirror.darkElementDamageModifier;
            lightElementDamageModifiers = mirror.lightElementDamageModifiers;
            lightElementDamageModifier = mirror.lightElementDamageModifier;
            slashingElementDamageModifiers = mirror.slashingElementDamageModifiers;
            slashingElementDamageModifier = mirror.slashingElementDamageModifier;
            piercingElementDamageModifiers = mirror.piercingElementDamageModifiers;
            piercingElementDamageModifier = mirror.piercingElementDamageModifier;
            bluntElementDamageModifiers = mirror.bluntElementDamageModifiers;
            bluntElementDamageModifier = mirror.bluntElementDamageModifier;
            poisonDamageModifiers = mirror.poisonDamageModifiers;
            poisonDamageModifier = mirror.poisonDamageModifier;
            enmityGenerationModifiers = mirror.enmityGenerationModifiers;
            enmityGenerationModifier = mirror.enmityGenerationModifier;
            enmity = mirror.enmity;
            effects = mirror.effects;
            effectsArray = mirror.effectsArray;
#if UNITY_EDITOR
            m_enmity = mirror.m_enmity;
            m_effects = mirror.m_effects;
#endif
            instantCasts = mirror.instantCasts;
            if (canDie.value && !invulnerable.value)
                dead = mirror.dead;
            else
                dead = false;
            disabled = mirror.disabled;
            mirror.sector = sector;
        }

        statusEffectUpdateTimer += Time.deltaTime;

        if (mirror == null)
        {
            if (effects.Count > 0 && effectsArray != null)
            {
                // Simulate FFXIV server ticks by updating status effects every 3 seconds (By default).
                if (statusEffectUpdateTimer >= statusEffectUpdateInterval)
                {
                    statusEffectUpdateTimer = 0f;
                    for (int i = 0; i < effectsArray.Length; i++)
                    {
                        if (effectsArray[i] == null)
                            continue;

                        effectsArray[i].onTick.Invoke(this);
                    }
                }
                // Update the status effect durations every frame.
                for (int i = effectsArray.Length - 1; i >= 0; i--)
                {
                    if (i >= effectsArray.Length)
                        continue;

                    if (effectsArray[i].data.unaffectedByTimeScale)
                    {
                        effectsArray[i].duration -= Time.deltaTime;
                    }
                    else
                    {
                        effectsArray[i].duration -= FightTimeline.deltaTime;
                    }
                    effectsArray[i].onUpdate.Invoke(this);
                    if (effectsArray[i]?.duration <= 0f)
                    {
                        RemoveEffect(effectsArray[i], true, this, effectsArray[i].uniqueTag, effectsArray[i].stacks);
                    }
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
        if (signMarkersGroup != null)
        {
            if (showSignMarkers)
            {
                signMarkersGroup.alpha = 1f;
            }
            else
            {
                signMarkersGroup.alpha = 0f;
            }
        }
    }

    void OnDestroy()
    {
        if (targetStatusEffectIconParent != null)
            Destroy(targetStatusEffectIconParent.gameObject, 0.1f);
        StopAllCoroutines();
    }
    #endregion

    #region Toggles
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

    public void ToggleTargetable(bool state)
    {
        if (state)
        {
            untargetable.RemoveFlag("toggleTargetable");
        }
        else
        {
            untargetable.SetFlag("toggleTargetable", true);
        }
    }

    public void ToggleNameplate(bool state)
    {
        hideNameplate = !state;
    }

    public void TogglePartyListEntry(bool state)
    {
        hidePartyListEntry = !state;
    }

    public void ResetState()
    {
        maxHealthModifiers.Clear();
        speedModifiers.Clear();
        speed.Clear();
        damageOutputModifiers.Clear();
        damageReduction.Clear();
        magicalTypeDamageModifiers.Clear();
        physicalTypeDamageModifiers.Clear();
        uniqueTypeDamageModifiers.Clear();
        unaspectedElementDamageModifiers.Clear();
        fireElementDamageModifiers.Clear();
        iceElementDamageModifiers.Clear();
        lightningElementDamageModifiers.Clear();
        waterElementDamageModifiers.Clear();
        windElementDamageModifiers.Clear();
        earthElementDamageModifiers.Clear();
        darkElementDamageModifiers.Clear();
        lightElementDamageModifiers.Clear();
        slashingElementDamageModifiers.Clear();
        piercingElementDamageModifiers.Clear();
        bluntElementDamageModifiers.Clear();
        poisonDamageModifiers.Clear();
        enmityGenerationModifiers.Clear();
        enmity.Clear();

        // Make the character invulnerable and unable to die here for a split second so any status effects that kill on expiry don't kill them
        invulnerable.SetFlag("resetState", true);
        canDie.SetFlag("resetState", false);
        preventDamage = true;

        if (effectsArray != null)
        {
            for (int i = 0; i < effectsArray.Length; i++)
            {
                if (effectsArray[i] == null)
                    continue;
                effectsArray[i].onCleanse.Invoke(this);
                effectsArray[i].onExpire.Invoke(this);
            }
        }

        effects.Clear();
        effectsArray = null;
        instantCasts.Clear();
        currentShields.Clear();

#if UNITY_EDITOR
        m_enmity.Clear();
        m_effects.Clear();
        
#endif

        currentMaxHealth = defaultMaxHealth;
        health = defaultMaxHealth;
        shield = 0;
        currentSpeed = defaultSpeed;
        currentDamageOutputMultiplier = 1f;
        currentDamageReduction = 1f;
        magicalTypeDamageModifier = 1f;
        physicalTypeDamageModifier = 1f;
        uniqueTypeDamageModifier = 1f;
        unaspectedElementDamageModifier = 1f;
        fireElementDamageModifier = 1f;
        iceElementDamageModifier = 1f;
        lightningElementDamageModifier = 1f;
        waterElementDamageModifier = 1f;
        windElementDamageModifier = 1f;
        earthElementDamageModifier = 1f;
        darkElementDamageModifier = 1f;
        lightElementDamageModifier = 1f;
        slashingElementDamageModifier = 1f;
        piercingElementDamageModifier = 1f;
        bluntElementDamageModifier = 1f;
        poisonDamageModifier = 1f;
        enmityGenerationModifier = 1f;

        invulnerable = new Flag(wasInvulnerable);
        uncontrollable = new Flag(wasUncontrollable);
        untargetable = new Flag(wasUntargetable);
        bound = new Flag(wasBound);
        stunned = new Flag(wasStunned);
        knockbackResistant = new Flag(wasKnockbackResistant);
        silenced = new Flag(wasSilenced);
        pacificied = new Flag(wasPacified);
        amnesia = new Flag(wasAmnesia);
        canDoActions = new Flag(wasCanDoActions);
        canDie = new Flag(wasCanDie);
        disabled = wasDisabled;
        dead = false;
        isAggressive = wasIsAggressive;
        hideNameplate = wasHideNameplate;
        hidePartyName = wasHidePartyName;
        hidePartyListEntry = wasHidePartyListEntry;

        if (aiController != null)
            aiController.ResetController();
        if (targetController != null)
            targetController.ResetController();
        if (bossController != null)
            bossController.ResetController();
        if (playerController != null)
            playerController.ResetController();
        if (actionController != null)
            actionController.ResetController();

        if (statusEffectParent != null)
        {
            foreach (Transform child in statusEffectParent)
            {
                Destroy(child.gameObject);
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

        OnEnable();
        Start();

        HealthBarUserInterface();
        UpdateCharacterName();

        Utilities.FunctionTimer.Create(this, () =>
        {
            preventDamage = false;
            dead = false;
            currentMaxHealth = defaultMaxHealth;
            health = defaultMaxHealth;
        }, 1f, $"CharacterState_{gameObject.name}_ResetState_Health_Delay", false, false);
    }
    #endregion

    #region Health
    public void ModifyHealth(Damage damage, bool kill = false, bool noFlyText = false)
    {
        if (!gameObject.activeSelf)
            return;

        if (preventDamage)
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

        if (m_damage.value > GlobalVariables.maximumDamage)
        {
            m_damage = new Damage(m_damage, 1);
        }

        switch (m_damage.applicationType)
        {
            default:
            {
                if (m_damage.value != 0)
                    ModifyHealthInternal(m_damage.value, kill, ignoreDamageReduction, ignoreDamageReduction);
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
                    RemoveHealth(m_damage.value / 100.0f, false, m_damage.negative, kill, ignoreDamageReduction, ignoreDamageReduction);
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

        if (!noFlyText && !showDamagePopups && !showTargetDamagePopups)
            noFlyText = true;

        if (!noFlyText && m_damage.value != 0)
            ShowDamageFlyText(flyTextDamage);

        if (m_damage.source != null)
        {
            onModifyHealth.Invoke(m_damage.source);
        }
    }

    private void SetHealth(long value, bool negative, bool kill = false, bool ignoreDamageReduction = false, bool ignoreShields = false)
    {
        if (preventDamage)
            return;

        if (kill)
        {
            if (negative)
            {
                ModifyHealthInternal(0, kill, ignoreDamageReduction, ignoreShields);
            }
            else
            {
                ModifyHealthInternal(currentMaxHealth, false, ignoreDamageReduction, ignoreShields);
            }
        }
        else
        {
            if (negative)
            {
                ModifyHealthInternal(-1 * (health - value), kill, ignoreDamageReduction, ignoreShields);
            }
            else
            {
                ModifyHealthInternal(Math.Abs(health - value), kill, ignoreDamageReduction, ignoreShields);
            }
        }
    }

    private void RemoveHealth(float percentage, bool fromMax, bool negative, bool kill = false, bool ignoreDamageReduction = false, bool ignoreShields = false)
    {
        if (preventDamage)
            return;

        if (kill)
        {
            ModifyHealthInternal(0, kill, ignoreDamageReduction, ignoreShields);
        }
        else
        {
            long damage = fromMax ? (long)Math.Round(currentMaxHealth * percentage) : (long)Math.Round(health * percentage);
            if (negative)
            {
                ModifyHealthInternal(-1 * damage, kill, ignoreDamageReduction, ignoreShields);
            }
            else
            {
                ModifyHealthInternal(Math.Abs(damage), kill, ignoreDamageReduction, ignoreShields);
            }
        }
    }

    private void ModifyHealthInternal(long value, bool kill = false, bool ignoreDamageReduction = false, bool ignoreShields = false)
    {
        if (preventDamage)
        {
            kill = false;
            return;
        }

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

        if (!invulnerable.value && !preventDamage)
        {
            if (FightTimeline.Instance.log)
                Debug.Log($"[CharacterState.{gameObject.name}] raw damage modified from health | value {value}, kill {kill}, ignoreDamageReduction {ignoreDamageReduction}, ignoreShields {ignoreShields}");

            // Check if shields should be ignored
            if (ignoreShields || kill)
            {
                // Directly reduce health if ignoring shields
                health += (int)remainingDamage;
            }
            else
            {
                // Shield reduction logic if not ignoring shields
                if (currentShields.Count > 0 && remainingDamage < 0 && !kill && !ignoreShields)
                {
                    if (FightTimeline.Instance.log)
                        Debug.Log($"[CharacterState.{gameObject.name}] shields");
                    for (int i = currentShields.Count - 1; i >= 0; i--)
                    {
                        if (remainingDamage >= 0)
                        {
                            break;
                        }

                        if (currentShields[i].value > Mathf.Abs(remainingDamage))
                        {
                            if (FightTimeline.Instance.log)
                                Debug.Log($"[CharacterState.{gameObject.name}] shield ({currentShields[i].key}, {currentShields[i].value}) was bigger than damage ({remainingDamage})");
                            currentShields[i] = new Shield(currentShields[i].key, currentShields[i].value + remainingDamage);
                            remainingDamage = 0;
                        }
                        else
                        {
                            if (FightTimeline.Instance.log)
                                Debug.Log($"[CharacterState.{gameObject.name}] damage was bigger than shield ({currentShields[i].key}, {currentShields[i].value}) damage ({remainingDamage})");
                            remainingDamage += currentShields[i].value;
                            RemoveEffect(currentShields[i].key, false, this);
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

        if (health <= 0 && !invulnerable.value && !preventDamage)
        {
            if (canDie.value)
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
                            if (showDamagePopups || showTargetDamagePopups)
                                ShowStatusEffectFlyText(effectsArray[i], " - ");
                            if (showStatusPopups)
                                ShowStatusEffectFlyTextWorldspace(effectsArray[i].data, effectsArray[i].stacks, " - ");
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
        if ((health > currentMaxHealth) || preventDamage)
        {
            health = currentMaxHealth;
        }

        RecalculateCurrentShield();
        //HealthBarUserInterface();
    }

    public void AddShield(long value, string identifier, CharacterState character, bool showPopup = false)
    {
        if (!currentShields.ContainsKey(identifier))
        {
            currentShields.Add(new Shield(identifier, value));

            if ((showDamagePopups || showTargetDamagePopups) && showPopup)
                ShowDamageFlyText(new Damage(value, false, true, DamageType.magical, ElementalAspect.unaspected, PhysicalAspect.none, DamageApplicationType.normal, character, Utilities.InsertSpaceBeforeCapitals(identifier)));
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
        if (deadMarker != null)
        {
            if (showDeadMarker && dead)
            {
                deadMarker.SetActive(true);
            } 
            else if (!showDeadMarker || !dead)
            {
                deadMarker.SetActive(false);
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
    #endregion

    #region Stats
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

    public void ModifyDamageModifier(float value, string identifier, DamageType damageType, ElementalAspect elementalAspect = ElementalAspect.none, PhysicalAspect physicalAspect = PhysicalAspect.none)
    {
        bool update = false;

        switch (damageType)
        {
            case DamageType.magical:
            {
                if (magicalTypeDamageModifiers.ContainsKey(identifier))
                {
                    magicalTypeDamageModifiers[identifier] = value;
                }
                else
                {
                    magicalTypeDamageModifiers.Add(identifier, value);
                }
                update = true;
                break;
            }
            case DamageType.physical:
            {
                if (physicalTypeDamageModifiers.ContainsKey(identifier))
                {
                    physicalTypeDamageModifiers[identifier] = value;
                }
                else
                {
                    physicalTypeDamageModifiers.Add(identifier, value);
                }
                update = true;
                break;
            }
            case DamageType.unique:
            {
                if (uniqueTypeDamageModifiers.ContainsKey(identifier))
                {
                    uniqueTypeDamageModifiers[identifier] = value;
                }
                else
                {
                    uniqueTypeDamageModifiers.Add(identifier, value);
                }
                update = true;
                break;
            }
        }
        switch (elementalAspect)
        {
            case ElementalAspect.unaspected:
            {
                if (unaspectedElementDamageModifiers.ContainsKey(identifier))
                {
                    unaspectedElementDamageModifiers[identifier] = value;
                }
                else
                {
                    unaspectedElementDamageModifiers.Add(identifier, value);
                }
                update = true;
                break;
            }
            case ElementalAspect.fire:
            {
                if (fireElementDamageModifiers.ContainsKey(identifier))
                {
                    fireElementDamageModifiers[identifier] = value;
                }
                else
                {
                    fireElementDamageModifiers.Add(identifier, value);
                }
                update = true;
                break;
            }
            case ElementalAspect.ice:
            {
                if (iceElementDamageModifiers.ContainsKey(identifier))
                {
                    iceElementDamageModifiers[identifier] = value;
                }
                else
                {
                    iceElementDamageModifiers.Add(identifier, value);
                }
                update = true;
                break;
            }
            case ElementalAspect.lightning:
            {
                if (lightningElementDamageModifiers.ContainsKey(identifier))
                {
                    lightningElementDamageModifiers[identifier] = value;
                }
                else
                {
                    lightningElementDamageModifiers.Add(identifier, value);
                }
                update = true;
                break;
            }
            case ElementalAspect.water:
            {
                if (waterElementDamageModifiers.ContainsKey(identifier))
                {
                    waterElementDamageModifiers[identifier] = value;
                }
                else
                {
                    waterElementDamageModifiers.Add(identifier, value);
                }
                update = true;
                break;
            }
            case ElementalAspect.wind:
            {
                if (windElementDamageModifiers.ContainsKey(identifier))
                {
                    windElementDamageModifiers[identifier] = value;
                }
                else
                {
                    windElementDamageModifiers.Add(identifier, value);
                }
                update = true;
                break;
            }
            case ElementalAspect.earth:
            {
                if (earthElementDamageModifiers.ContainsKey(identifier))
                {
                    earthElementDamageModifiers[identifier] = value;
                }
                else
                {
                    earthElementDamageModifiers.Add(identifier, value);
                }
                update = true;
                break;
            }
            case ElementalAspect.dark:
            {
                if (darkElementDamageModifiers.ContainsKey(identifier))
                {
                    darkElementDamageModifiers[identifier] = value;
                }
                else
                {
                    darkElementDamageModifiers.Add(identifier, value);
                }
                update = true;
                break;
            }
            case ElementalAspect.light:
            {
                if (lightElementDamageModifiers.ContainsKey(identifier))
                {
                    lightElementDamageModifiers[identifier] = value;
                }
                else
                {
                    lightElementDamageModifiers.Add(identifier, value);
                }
                update = true;
                break;
            }
        }
        switch (physicalAspect)
        {
            case PhysicalAspect.slashing:
            {
                if (slashingElementDamageModifiers.ContainsKey(identifier))
                {
                    slashingElementDamageModifiers[identifier] = value;
                }
                else
                {
                    slashingElementDamageModifiers.Add(identifier, value);
                }
                update = true;
                break;
            }
            case PhysicalAspect.piercing:
            {
                if (piercingElementDamageModifiers.ContainsKey(identifier))
                {
                    piercingElementDamageModifiers[identifier] = value;
                }
                else
                {
                    piercingElementDamageModifiers.Add(identifier, value);
                }
                update = true;
                break;
            }
            case PhysicalAspect.blunt:
            {
                if (bluntElementDamageModifiers.ContainsKey(identifier))
                {
                    bluntElementDamageModifiers[identifier] = value;
                }
                else
                {
                    bluntElementDamageModifiers.Add(identifier, value);
                }
                update = true;
                break;
            }
        }

        if (update)
        {
            RecalculateCurrentDamageModifiers(damageType, elementalAspect, physicalAspect);
        }
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
    #endregion

    #region Enmity
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
    #endregion

    #region Status Effects
    public void AddEffect(StatusEffectData data, CharacterState character, bool self = false, int tag = 0, int stacks = 0, float duration = -1)
    {
        AddEffect(data, null, character, self, tag, stacks, duration);
    }

    public void AddEffect(StatusEffectData data, Damage? damage, CharacterState character, bool self = false, int tag = 0, int stacks = 0, float duration = -1)
    {
        if (data.statusEffect == null)
            return;

        if (!gameObject.activeSelf)
            return;

        if (health <= 0 || dead)
            return;

        if (data.negative && invulnerable.value)
            return;

        if (mirror != null)
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
                        effects[name].Refresh(stacks, 0, duration);
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
            float val = duration;
            if (duration < 0)
                val = 0;

            if (!data.unique)
                effects[name].Refresh(stacks, 0, val);
            refreshed = true;
        }

        if (showStatusPopups && refreshed)
        {
            ShowStatusEffectFlyTextWorldspace(data, stacks + data.appliedStacks, " + ");
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

        AddEffect(effect, damage, character, self, tag, stacks, duration);
    }

    /*public void AddEffect(string name, CharacterState character, bool self = false, int tag = 0, int stacks = 0)
    {
        AddEffect(name, null, character, self, tag);
    }

    public void AddEffect(string name, Damage? damage, CharacterState character, bool self = false, int tag = 0, int stacks = 0)
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

        if (showStatusPopups && refreshed)
        {
            ShowStatusEffectFlyTextWorldspace(effects[name].data, stacks + effects[name].data.appliedStacks, " + ");
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
                        if (FightTimeline.Instance.allAvailableStatusEffects[i].incompatableStatusEffects.Contains(effectsArray[k].data) || (FightTimeline.Instance.allAvailableStatusEffects[i].negative && invulnerable.value))
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

                AddEffect(effect, damage, character, self, tag);
            }
        }
    }*/

    public void AddEffect(StatusEffect effect, CharacterState character, bool self = false, int tag = 0, int stacks = 0, float duration = -1)
    {
        AddEffect(effect, null, character, self, tag, stacks, duration);
    }

    public void AddEffect(StatusEffect effect, Damage? damage, CharacterState character, bool self = false, int tag = 0, int stacks = 0, float duration = -1)
    {
        //if (effect.data.name.Contains("Cleaned"))
        //    Debug.LogError("Cleaned detected");

        if (!gameObject.activeSelf)
            return;

        if (health <= 0 || dead)
            return;

        if (mirror != null)
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
                        effects[name].Refresh(stacks, 0, duration);
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
            float val = duration;
            if (duration < 0)
                val = 0;
            if (!effect.data.unique)
                effects[name].Refresh(stacks, 0, val);
            refreshed = true;
        }

        if (showStatusPopups)
        {
            ShowStatusEffectFlyTextWorldspace(effect.data, stacks + effect.data.appliedStacks, " + ");
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

        if (showDamagePopups || showTargetDamagePopups)
        {
            ShowStatusEffectFlyText(effect, stacks + effect.data.appliedStacks, " + ", character);
        }

        // Only prevent negative effects
        if (effect.data.negative && invulnerable.value)
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
                effect.Initialize(this,statusEffectNegativeIconParent, statusEffectIconParentParty, targetStatusEffectIconParent, ownStatusEffectColor);
            }
            else
            {
                effect.Initialize(this, statusEffectNegativeIconParent, statusEffectIconParentParty, targetStatusEffectIconParent, otherStatusEffectColor);
            }
        }
        else
        {
            if (self)
            {
                effect.Initialize(this, statusEffectPositiveIconParent, statusEffectIconParentParty, targetStatusEffectIconParent, ownStatusEffectColor);
            }
            else
            {
                effect.Initialize(this, statusEffectPositiveIconParent, statusEffectIconParentParty, targetStatusEffectIconParent, otherStatusEffectColor);
            }
        }
        if (duration > 0)
        {
            effect.duration = duration;
        }
        effect.onApplication.Invoke(this);

        if (partyList != null)
            partyList.UpdatePrioritySorting();
    }

    public void RemoveEffect(StatusEffectData data, bool expired, CharacterState character, int tag = 0, int stacks = 0)
    {
        RemoveEffect(data.statusName, expired, character, tag, stacks);
    }

    public void RemoveEffect(StatusEffect effect, bool expired, CharacterState character, int tag = 0, int stacks = 0)
    {
        RemoveEffect(effect.data.statusName, expired, character, tag, stacks);
    }

    public void RemoveEffect(string name, bool expired, CharacterState character, int tag = 0, int stacks = 0)
    {
        if (!gameObject.activeSelf)
            return;

        if (mirror != null)
            return;

        if (tag > 0)
        {
            name = $"{name}_{tag}";
        }
        if (tag < 0)
        {
            for (int i = 0; i < (tag * -1); i++)
            {
                RemoveEffect(name, expired, character, i + 1, stacks);
            }
            return;
        }

        if (!effects.ContainsKey(name))
            return;

        StatusEffect temp = effects[name];

        if ((showDamagePopups || showTargetDamagePopups) && (effects[name].stacks <= 1 || effects[name].stacks <= stacks))
        {
            ShowStatusEffectFlyText(temp, " - ", character);
        }
        else if (showDamagePopups || showTargetDamagePopups)
        {
            ShowStatusEffectFlyText(temp, 1, " - ", character);
        }
        if (showStatusPopups && (effects[name].stacks <= 1 || effects[name].stacks <= stacks))
        {
            ShowStatusEffectFlyTextWorldspace(temp.data, temp.stacks, " - ");
        }
        else if (showStatusPopups)
        {
            ShowStatusEffectFlyTextWorldspace(temp.data, 1, " - ");
        }

        // Not sure why being invulnerable would prevent buffs and debuffs from clearing?
        // Was causing issues with Titan Gaols, so for now it's gone at least.
        //if (invulnerable.value)
        //    return;

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
                        if (FightTimeline.Instance.log)
                            Debug.Log($"Removed sub status {m_name} with a index {i} of {temp.data.refreshStatusEffects.Count} total from {temp.data.statusName}");
                        RemoveEffect(m_name, expired, character, tag, stacks);
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
        if (string.IsNullOrEmpty(name))
            return false;

        if (tag > 0)
        {
            name = $"{name}_{tag}";
        }

        return effects.ContainsKey(name);
    }

    public bool HasAnyVersionOfEffect(string name)
    {
        if (effectsArray != null && effectsArray.Length > 0)
        {
            for (int i = 0; i < effectsArray.Length; i++)
            {
                if (effectsArray[i].data.statusName == name)
                {
                    return true;
                }
            }
        }

        return false; 
    }

    public StatusEffect[] GetEffects()
    {
        return effectsArray;
    }

    public StatusEffect GetEffect(string name)
    {
        if (effects.ContainsKey(name))
        {
            return effects[name];
        }
        return null;
    }
    #endregion

    #region FlyTexts
    public void ShowStatusEffectFlyText(StatusEffect statusEffect, int stacks, string prefix, CharacterState character = null)
    {
        ShowStatusEffectFlyText(statusEffect.data, stacks, prefix, character);
    }

    public void ShowStatusEffectFlyText(StatusEffect statusEffect, string prefix, CharacterState character = null)
    {
        ShowStatusEffectFlyText(statusEffect.data, statusEffect.stacks, prefix, character);
    }

    public void ShowStatusEffectFlyText(StatusEffectData data, int stacks, string prefix, CharacterState character = null)
    {
        ShowStatusEffectFlyTextInternal(data, stacks, prefix, true, character);
    }

    public void ShowStatusEffectFlyTextWorldspace(StatusEffectData data, int stacks, string prefix, CharacterState character = null)
    {
        ShowStatusEffectFlyTextInternal(data, stacks, prefix, false, character);
    }

    public void ShowStatusEffectFlyTextInternal(StatusEffectData data, int stacks, string prefix, bool feed, CharacterState character)
    {
        if (data.hidden)
            return;

        if (mirror != null)
            return;

        // Hard coded the Short (@s) and Long (@l) that are used to distinguish between few of the same debuffs,
        // also the '#' character which is used for non capitalised letter sequences. This needs a better implementation.
        string result = $"{prefix}{Utilities.InsertSpaceBeforeCapitals(data.statusName).Replace("#", " ")}";

        // Remove anything after the '@' symbol
        int atIndex = result.IndexOf('@');
        if (atIndex >= 0)
        {
            result = result.Substring(0, atIndex);
        }

        Color color = neutralPopupColor;

        if (!feed)
        {
            color = statusPopupColor;
        }
        else if (prefix.Contains('+'))
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

        if (invulnerable.value && data.negative && prefix.Contains('+'))
        {
            result = $"{result} Has No Effect";
            if (feed)
                color = neutralPopupColor;
        }

        //Debug.Log($"data '{data.name}', stacks '{stacks}', prefix '{prefix}', feed '{feed}', character '{character?.characterName} ({character?.gameObject.name})'");

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

        if (feed)
            ShowFlyText(new FlyText(result, color, icon, string.Empty, character));
        else
            ShowFlyTextWorldspace(new FlyText(result, color, icon, string.Empty, character));
    }

    public void ShowDamageFlyText(Damage damage, bool showValue = true)
    {
        if (mirror != null)
            return;

        string result = string.Empty;

        Damage finalDamage = new Damage(damage);

        if (!damage.ignoreDamageReductions)
        {
            // Apply damage reduction
            finalDamage = new Damage(damage, (long)Math.Round(damage.value * currentDamageReduction));
        }

        string finalValue = string.Empty;

        if (showValue)
        {
            if (finalDamage.value < 0 || finalDamage.value > 0)
            {
                finalValue = Mathf.Abs(finalDamage.value).ToString();
            }
            else
            {
                finalValue = finalDamage.value.ToString();
            }
        }
        else
        {
            finalValue = string.Empty;
        }

        if ((invulnerable.value && finalDamage.negative) || (finalDamage.value == 0))
        {
            finalValue = "DODGE";
        }

        if (showElementalAspect && finalDamage.elementalAspect != ElementalAspect.none && finalDamage.elementalAspect != ElementalAspect.unaspected)
        {
            result = $"<sprite=\"damage_types\" name=\"{(int)finalDamage.type}\"><sprite=\"damage_types\" name=\"{finalDamage.elementalAspect.ToString("g")}\">{finalValue}";
        } 
        else
        {
            result = $"<sprite=\"damage_types\" name=\"{(int)finalDamage.type}\">{finalValue}";
        }

        string source = finalDamage.name;

        Color color = positivePopupColor;

        if (finalDamage.negative || finalDamage.value < 0)
        {
            color = negativePopupColor;
        }
        else if (!finalDamage.negative)
        {
            result = $"<sprite=\"damage_types\" name=\"0\">{finalValue}";
        }

        if ((invulnerable.value && finalDamage.negative) || (finalDamage.value == 0))
        {
            color = neutralPopupColor;
        }

        ShowFlyText(new FlyText(result, color, null, source, finalDamage.source));
    }

    /*public void ShowDamageFlyText(int value, string source)
    {
        string result = Mathf.Abs(value).ToString();

        Color color = positivePopupColor;

        if (value < 0)
        {
            color = negativePopupColor;
        }

        ShowFlyText(new FlyText(result, color, null, source));
    } */

    public void ShowFlyText(FlyText text)
    {
        if (!gameObject.activeSelf)
            return;

        if (mirror != null)
            return;

        if ((damagePopupPrefab == null && targetDamagePopupPrefab == null) || (damagePopupParent == null && targetDamagePopupParent == null))
            return;

        if (string.IsNullOrEmpty(text.content))
            return;

        popupTexts.Enqueue(text);

        if (popupCoroutine == null)
        {
            popupCoroutine = StartCoroutine(IE_ProcessFlyTextQueue());
        }
    }

    public void ShowFlyTextWorldspace(FlyText text)
    {
        if (!gameObject.activeSelf)
            return;

        if (mirror != null)
            return;

        if (statusPopupPrefab == null || statusPopupParent == null)
            return;

        if (string.IsNullOrEmpty(text.content))
            return;

        statusPopupTexts.Enqueue(text);

        if (statusPopupCoroutine == null)
        {
            statusPopupCoroutine = StartCoroutine(IE_ProcessWorldspaceFlyTextQueue());
        }
    }

    private IEnumerator IE_ProcessFlyTextQueue()
    {
        while (popupTexts.Count > 0)
        {
            FlyText text = popupTexts.Dequeue();

            if (damagePopupParent != null && damagePopupPrefab != null && showDamagePopups)
            {
                SetupPopupTextObject(text, damagePopupPrefab, damagePopupParent);
            }
            if (targetDamagePopupParent != null && targetDamagePopupPrefab != null && showTargetDamagePopups)
            {
                text = new FlyText(text, targetDamagePopupColor);
                if (showOnlyFromCharacter != null && text.character != null)
                {
                    if (text.character == showOnlyFromCharacter)
                    {
                        SetupPopupTextObject(text, targetDamagePopupPrefab, targetDamagePopupParent);
                    }
                } 
                else if (showOnlyFromCharacter == null)
                {
                    SetupPopupTextObject(text, targetDamagePopupPrefab, targetDamagePopupParent);
                }
            }

            yield return new WaitForSeconds(popupTextDelay);
        }

        popupCoroutine = null;
    }

    private void SetupPopupTextObject(FlyText text, GameObject prefab, Transform parent)
    {
        GameObject go = Instantiate(prefab, parent);
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
    }

    private IEnumerator IE_ProcessWorldspaceFlyTextQueue()
    {
        while (statusPopupTexts.Count > 0)
        {
            FlyText text = statusPopupTexts.Dequeue();

            GameObject go = Instantiate(statusPopupPrefab, statusPopupParent);
            TextMeshProUGUI tm = go.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            tm.text = text.content;
            tm.color = text.color;

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

            if (go.TryGetComponent(out HudElement e))
            {
                e.characterState = this;
                e.Initialize();
            }

            yield return new WaitForSeconds(statusPopupTextDelay);
        }

        statusPopupCoroutine = null;
    }
    #endregion

    #region Data Structs
    [System.Serializable]
    public struct FlyText
    {
        public string content;
        public Color color;
        public Sprite icon;
        public string source;
        public CharacterState character;

        public FlyText(string content, Color color, Sprite icon, string source, CharacterState character)
        {
            this.content = content;
            this.color = color;
            this.icon = icon;
            this.source = source;
            this.character = character;
        }

        public FlyText(FlyText copy, Color color)
        {
            content = copy.content;
            this.color = color;
            icon = copy.icon;
            source = copy.source;
            character = copy.character;
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
    #endregion
}
