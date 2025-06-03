using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using static ActionController;
using static GlobalData;
using static UnityEngine.ParticleSystem;
using Random = UnityEngine.Random;

public class DamageTrigger : MonoBehaviour
{
    Collider m_collider;
    public CharacterActionData data;

    public string damageName = string.Empty;
    public bool inverted = false;
    public bool log = false;
    public CharacterState owner;
    public bool autoAssignOwner = false;
    [Tag]
    public string ableToHitTag = "Player";
    public string canHitCharacterName = string.Empty;
    public Damage damage;
    public AnimationCurve damageFalloff = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(1f, 1f)
    );
    public long enmity = 0;
    public bool increaseEnmity = false;
    public bool topEnmity = false;
    public bool initializeOnStart = true;
    public bool self = false;
    public bool dealsDamage = true;
    public bool passDamage = false;
    public bool proximityBased = false;
    public bool isAShield = false;
    public bool cleaves = true;
    public bool cancelsMovement = false;
    public bool ignoresOwner = false;
    public bool ignoreSnapshot = false;
    public bool playerActivated = true;
    public bool updateLive = false;
    public bool shared = false;
    public bool enumeration = false;
    public bool requireOwner = false;
    public bool resetOnReload = false;
    public bool resetOwner = false;
    public float visualDelay = 0f;
    public float triggerDelay = 0f;
    public float triggerDelayVariance = 0f;
    public float damageApplicationDelay = 0.25f;
    public float cooldown = 10f;
    public int playersRequired = 0;
    public float damageMultiplierPerMissingPlayer = 1f;
    public List<CharacterState> currentPlayers = new List<CharacterState>();
    public List<StatusEffectData> appliedEffects = new List<StatusEffectData>();
    public List<StatusEffectData> appliedEffectsOnFail = new List<StatusEffectData>();
    public UnityEvent<CharacterState> onHit;
    public UnityEvent<CharacterState> onFail;
    public UnityEvent<CharacterState> onFinish;
    public UnityEvent onStart;
    public UnityEvent onSpawn;
    public UnityEvent<CharacterState> onInitialize;
    public UnityEvent<CharacterCollection> onTrigger;

    private int id = 0;
    private bool inProgress = false;
    private bool initialized = false;
    private bool colliderWaDisabled = false;

#if UNITY_EDITOR
    public int dummy = 0;
    [Button("Initialize")]
    public void InitializeButton()
    {
        Initialize();
    }
#endif

    void Awake()
    {
        m_collider = GetComponent<Collider>();
        id = Random.Range(1000,10000);

        if (m_collider != null)
        {
            if (m_collider.enabled)
                colliderWaDisabled = false;
            else
                colliderWaDisabled = true;
        }
        else
        {
            colliderWaDisabled = true;
        }
    }

    void Start()
    {
        onSpawn.Invoke();

        if (initializeOnStart && !initialized)
            Initialize();

        if (resetOnReload && FightTimeline.Instance != null)
            FightTimeline.Instance.onReset.AddListener(ResetTrigger);
    }

    void OnDisable()
    {
        initialized = false;
        currentPlayers.Clear();
        inProgress = false;
    }

    public void Initialize(float delay = 0f)
    {
        if (autoAssignOwner)
        {
            owner = transform.GetComponentInParent<CharacterState>();
        }

        if (owner == null && requireOwner)
        {
            return;
        }

        if (delay > 0f)
        {
            visualDelay = delay;
            triggerDelay = delay + 0.1f;
        }

        if (m_collider != null && visualDelay > 0f)
        {
            m_collider.enabled = false;
            Utilities.FunctionTimer.Create(this, () => m_collider.enabled = true, visualDelay, $"{id}_{damageName}_{gameObject.name}_visual_delay", false, true);
        }
        if (triggerDelay > 0f)
        {
            if (triggerDelayVariance > 0f)
                triggerDelay += Random.Range(0f, triggerDelayVariance);
            Utilities.FunctionTimer.Create(this, () => {
                if (!inProgress)
                    StartDamageTrigger();
            }, triggerDelay, $"{id}_{damageName}_{gameObject.name}_trigger_delay", false, true);
        }
        else if (!playerActivated)
        {
            if (!inProgress)
                StartDamageTrigger();
        }

        onInitialize.Invoke(owner);

        initialized = true;
    }

    public void ResetOwner()
    {
        owner = null;
    }

    public void ResetTrigger()
    {
        inProgress = false;
        StopAllCoroutines();
        Utilities.FunctionTimer.StopTimer($"{id}_{damageName}_{gameObject.name}_trigger_cooldown");
        Utilities.FunctionTimer.StopTimer($"{id}_{damageName}_{gameObject.name}_trigger_delay");
        Utilities.FunctionTimer.StopTimer($"{id}_{damageName}_{gameObject.name}_visual_delay");
        currentPlayers.Clear();
        if (resetOwner)
            owner = null;
        if (m_collider != null)
        {
            if (colliderWaDisabled)
                m_collider.enabled = false;
            else
                m_collider.enabled = true;
        }
    }

    public void Activate(bool playerActivated)
    {
        this.playerActivated = playerActivated;
        if (!playerActivated && !inProgress)
            StartDamageTrigger();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!inverted)
        {
            if (other.CompareTag(ableToHitTag))
            {
                if (other.transform.TryGetComponentInParents(true, out CharacterState playerState))
                {
                    if (!string.IsNullOrEmpty(canHitCharacterName))
                    {
                        if (!playerState.characterName.ToLower().Contains(canHitCharacterName.ToLower()))
                            return;
                    }

                    if (currentPlayers.Contains(playerState))
                        return;
                    if (playerState == owner && ignoresOwner)
                        return;
                    if (!cleaves && owner == null)
                        return;
                    if (!cleaves && owner != null && playerState != owner)
                        return;

                    currentPlayers.Add(playerState);

                    if (!inProgress && playerActivated && initialized)
                        StartDamageTrigger();
                }
            }
        }
        else
        {
            if (other.CompareTag(ableToHitTag))
            {
                if (other.transform.TryGetComponentInParents(true, out CharacterState playerState))
                {
                    if (!string.IsNullOrEmpty(canHitCharacterName))
                    {
                        if (!playerState.characterName.ToLower().Contains(canHitCharacterName.ToLower()))
                            return;
                    }
                    if (!currentPlayers.Contains(playerState))
                        return;
                    if (playerState == owner && ignoresOwner)
                        return;
                    if (!cleaves && owner == null)
                        return;
                    if (!cleaves && owner != null && playerState != owner)
                        return;

                    currentPlayers.Remove(playerState);
                }
            }
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (updateLive)
        {
            if (other == null)
                return;

            if (!inverted)
            {
                if (other.CompareTag(ableToHitTag) && !inProgress && initialized)
                {
                    if (other.transform.TryGetComponentInParents(true, out CharacterState playerState))
                    {
                        if (!string.IsNullOrEmpty(canHitCharacterName))
                        {
                            if (!playerState.characterName.ToLower().Contains(canHitCharacterName.ToLower()))
                                return;
                        }

                        if (!updateLive && currentPlayers.Contains(playerState))
                            return;
                        if (playerState == owner && ignoresOwner)
                            return;
                        if (!cleaves && owner == null)
                            return;
                        if (!cleaves && owner != null && playerState != owner)
                            return;

                        if (!currentPlayers.Contains(playerState))
                            currentPlayers.Add(playerState);

                        if (playerActivated)
                            StartDamageTrigger();
                    }
                }
                else
                {
                    if (other.CompareTag(ableToHitTag))
                    {
                        if (other.transform.TryGetComponentInParents(true, out CharacterState playerState))
                        {
                            if (!string.IsNullOrEmpty(canHitCharacterName))
                            {
                                if (!playerState.characterName.ToLower().Contains(canHitCharacterName.ToLower()))
                                    return;
                            }
                            if (!updateLive && !currentPlayers.Contains(playerState))
                                return;
                            if (playerState == owner && ignoresOwner)
                                return;
                            if (!cleaves && owner == null)
                                return;
                            if (!cleaves && owner != null && playerState != owner)
                                return;

                            if (currentPlayers.Contains(playerState))
                                currentPlayers.Remove(playerState);
                        }
                    }
                }
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (!inverted)
        {
            if (other.CompareTag(ableToHitTag))
            {
                if (other.transform.TryGetComponentInParents(true, out CharacterState playerState))
                {
                    if (!string.IsNullOrEmpty(canHitCharacterName))
                    {
                        if (!playerState.characterName.ToLower().Contains(canHitCharacterName.ToLower()))
                            return;
                    }

                    if (!currentPlayers.Contains(playerState))
                        return;
                    if (playerState == owner && ignoresOwner)
                        return;
                    if (!cleaves && owner == null)
                        return;
                    if (!cleaves && owner != null && playerState != owner)
                        return;

                    currentPlayers.Remove(playerState);
                }
            }
        }
        else
        {
            if (other.CompareTag(ableToHitTag))
            {
                if (other.transform.TryGetComponentInParents(true, out CharacterState playerState))
                {
                    if (!string.IsNullOrEmpty(canHitCharacterName))
                    {
                        if (!playerState.characterName.ToLower().Contains(canHitCharacterName.ToLower()))
                            return;
                    }
                    if (currentPlayers.Contains(playerState))
                        return;
                    if (playerState == owner && ignoresOwner)
                        return;
                    if (!cleaves && owner == null)
                        return;
                    if (!cleaves && owner != null && playerState != owner)
                        return;

                    currentPlayers.Add(playerState);

                    if (!inProgress && playerActivated && initialized)
                        StartDamageTrigger();
                }
            }
        }
    }

    private void StartDamageTrigger()
    {
        if (log)
            Debug.Log($"[DamageTrigger ({gameObject.name})] StartDamageTrigger (inProgress {inProgress})");
        if (damageApplicationDelay > 0)
        {
            StartCoroutine(IE_StartDamageTrigger(currentPlayers.ToArray()));
        }
        else
        {
            TriggerDamage(currentPlayers.ToArray());
            if (cooldown > 0f)
            {
                inProgress = true;
                Utilities.FunctionTimer.Create(this, () => inProgress = false, cooldown, $"{id}_{damageName}_{gameObject.name}_trigger_cooldown", false, true);
            }
        }
        onStart.Invoke();
    }

    private IEnumerator IE_StartDamageTrigger(CharacterState[] players)
    {
        inProgress = true;
        yield return new WaitForSeconds(damageApplicationDelay);
        if (log)
            Debug.Log($"[DamageTrigger ({gameObject.name})] IE_StartDamageTrigger (inProgress {inProgress})");
        if (ignoreSnapshot)
        {
            List<CharacterState> candidates = new List<CharacterState>();
            currentPlayers.AddRange(players);
            foreach (CharacterState player in currentPlayers)
            {
                if (!candidates.Contains(player))
                {
                    candidates.Add(player);
                }
            }
            TriggerDamage(candidates.ToArray());
        }
        else
        {
            TriggerDamage(players);
        }
        if (cooldown > 0f)
        {
            Utilities.FunctionTimer.Create(this, () => inProgress = false, cooldown, $"{id}_{damageName}_{gameObject.name}_trigger_cooldown", false, true);
        }
        else
        {
            inProgress = false;
        }
    }

    public void TriggerDamage(CharacterState[] players)
    {
        if (!initialized)
            return;

        if (log)
            Debug.Log($"[DamageTrigger ({gameObject.name})] TriggerDamage (inProgress {inProgress}, players {players.Length})");

        foreach (CharacterState player in players)
        {
            if (log)
                Debug.Log($"[DamageTrigger ({gameObject.name})] --> player {player.gameObject.name}");
        }

        string newName = damage.name;

        if (!string.IsNullOrEmpty(damageName))
        {
            newName = damageName;
        }

        // Kinda hacky way to remove the hidden healer from damage triggers, because enumerations kept getting messed up if they were sometimes hit
        players = players.Where(p => !p.characterName.ToLower().Contains("hidden") &&
                                     (string.IsNullOrEmpty(canHitCharacterName) ||
                                      p.characterName.ToLower().Contains(canHitCharacterName.ToLower()))).ToArray();

        Damage damagePerPlayer;

        if (damage.value == 0 || !dealsDamage)
        {
            damagePerPlayer = new Damage(damage, 0, newName);
        }
        else
        {
            damagePerPlayer = (shared && players.Length > 0) ? new Damage(damage, Mathf.RoundToInt(damage.value / players.Length), newName) : new Damage(damage, newName);
        }

        bool failed = false;
        bool kill = false;

        if (playersRequired > 0)
        {
            if (enumeration)
            {
                if (players.Length != playersRequired)
                {
                    damagePerPlayer = new Damage(damagePerPlayer, -999999);
                    failed = true;
                    kill = true;
                }
            }
            else
            {
                if (players.Length < playersRequired)
                {
                    failed = true;

                    if (damageMultiplierPerMissingPlayer != 1f)
                    {
                        float damageMultiplier = damageMultiplierPerMissingPlayer - 1.0f;
                        damagePerPlayer = new Damage(damagePerPlayer, (long)Math.Round(damagePerPlayer.value * (1.0f + (damageMultiplier * (playersRequired - players.Length)))));
                    }
                }
            }
        }

        if (damage.value <= -GlobalVariables.maximumDamage)
        {
            damagePerPlayer = new Damage(damagePerPlayer, -1);
            kill = true;
        }

        if (players.Length > 0)
        {
            if (increaseEnmity && owner != null && data != null)
            {
                // First, collect all enemies from the players' enmity lists
                HashSet<CharacterState> enemiesToAdd = new HashSet<CharacterState>();

                // Iterate over the players list to gather the enemies they have enmity for
                foreach (CharacterState player in players)
                {
                    if (ignoresOwner)
                    {
                        if (player == owner)
                        {
                            continue;
                        }
                    }

                    foreach (CharacterState enemy in player.enmity.Keys)
                    {
                        // Add the enemy to the set if it's not already there
                        if (!enemiesToAdd.Contains(enemy))
                        {
                            enemiesToAdd.Add(enemy);
                        }
                    }
                }

                // Now, iterate over the gathered enemies and add enmity for each one to the owner
                foreach (CharacterState enemy in enemiesToAdd)
                {
                    // If the owner does not already have enmity for this enemy, add them
                    if (!owner.enmity.ContainsKey(enemy))
                    {
                        owner.enmity[enemy] = 0; // Initialize with 0 enmity
                    }

                    // Calculate the enmity to add based on damage dealt or healing done
                    long enmityToAdd = Mathf.RoundToInt((damagePerPlayer.value * data.damageEnmityMultiplier) * owner.enmityGenerationModifier);

                    // Set the enmity for the owner (you can use AddEnmity or SetEnmity depending on your logic)
                    long currentEnmity = owner.enmity[enemy];
                    currentEnmity += enmityToAdd;

                    owner.SetEnmity(currentEnmity, enemy);
                }
            }

            for (int i = 0; i < players.Length; i++)
            {
                if (ignoresOwner)
                {
                    if (players[i] == owner)
                    {
                        continue;
                    }
                }

                if (cancelsMovement)
                {
                    if (players[i].aiController != null && players[i].canDie.value)
                    {
                        players[i].aiController.freezeMovement = true;
                    } 
                    else if (players[i].playerController != null && players[i].canDie.value)
                    {
                        players[i].playerController.freezeMovement = true;
                    }
                }

                if (dealsDamage)
                {
                    if (damagePerPlayer.value != 0 && !proximityBased)
                    {
                        players[i].ModifyHealth(damagePerPlayer, kill);
                    } 
                    else if (damagePerPlayer.value != 0 && proximityBased)
                    {
                        float distance = Vector3.Distance(players[i].transform.position, transform.position);

                        // Sample the curve at the current distance to get the multiplier
                        float damageMultiplier = damageFalloff.Evaluate(distance);

                        // Calculate the final damage based on the multiplier
                        Damage finalDamagePerPlayer = new Damage(damagePerPlayer, Mathf.RoundToInt(damagePerPlayer.value * damageMultiplier));

                        players[i].ModifyHealth(finalDamagePerPlayer, kill);
                    }

                    if (increaseEnmity && owner != null && data != null)
                    {
                        enmity = 0;

                        if (!data.isHeal && !data.isShield && players[i].gameObject.activeInHierarchy)
                        {
                            if (topEnmity && owner.partyList != null)
                            {
                                CharacterState highestEnmityMember = owner.partyList.GetHighestEnmityMember(players[i]);
                                long highestEnmity = 0;
                                highestEnmityMember.enmity.TryGetValue(players[i], out highestEnmity);
                                owner.ResetEnmity(players[i]);
                                enmity = highestEnmity;
                            }

                            long damageEnmity = Math.Abs(data.enmity);
                            damageEnmity += Math.Abs(Mathf.RoundToInt(data.damage.value * data.damageEnmityMultiplier * owner.enmityGenerationModifier));
                            enmity += damageEnmity;
                            owner.AddEnmity(enmity, players[i]);
                        }
                        else if (players[i].gameObject.activeInHierarchy)
                        {
                            long healingEnmity = Math.Abs(data.enmity);
                            healingEnmity += Math.Abs(Mathf.RoundToInt(damagePerPlayer.value * data.damageEnmityMultiplier * owner.enmityGenerationModifier));
                            enmity += healingEnmity;

                            // Create a list of keys to avoid modifying the dictionary while iterating
                            var keys = new List<CharacterState>(owner.enmity.Keys);

                            if (keys.Count > 0)
                            {
                                // Loop through all enemies the owner has enmity towards
                                foreach (var targetPlayer in keys)
                                {
                                    // Reset current enmity from the enmity dictionary for each iteration
                                    long currentEnmity = owner.enmity[targetPlayer];

                                    // Calculate the healing enmity for this player based on damage healed
                                    long healingEnmityForPlayer = Mathf.RoundToInt((damagePerPlayer.value * data.damageEnmityMultiplier) * owner.enmityGenerationModifier);

                                    // Add the healing enmity to the current enmity value
                                    currentEnmity += healingEnmityForPlayer;

                                    // Ensure the enmity is correctly updated per player and does not carry over to the next iteration
                                    owner.enmity[targetPlayer] = currentEnmity;  // Update the dictionary with the new value

                                    // Update the enmity for this player
                                    owner.SetEnmity(currentEnmity, targetPlayer);
                                }
                            }

                            // Optionally, add the enmity for the character itself (owner) based on healing
                            //owner.AddEnmity(enmity, owner);
                        }
                    }

                    // Needs a fix or new implementation, but not important since a shield should never be applied directly
                    /*if (!isAShield)
                    {
                        players[i].ModifyHealth(damagePerPlayer, kill);
                    }
                    else
                    {
                        players[i].AddShield(damagePerPlayer.value, damage.name);
                    }*/
                }

                if (appliedEffects.Count > 0)
                {
                    for (int k = 0; k < appliedEffects.Count; k++)
                    {
                        if (passDamage)
                        {
                            players[i].AddEffect(appliedEffects[k], damage, damage.source, self);
                        }
                        else
                        {
                            players[i].AddEffect(appliedEffects[k], damage.source, self);
                        }
                    }
                }
                if (failed && appliedEffectsOnFail.Count > 0)
                {
                    for (int k = 0; k < appliedEffectsOnFail.Count; k++)
                    {
                        if (passDamage)
                        {
                            players[i].AddEffect(appliedEffectsOnFail[k], damage, damage.source, self);
                        }
                        else
                        {
                            players[i].AddEffect(appliedEffectsOnFail[k], damage.source, self);
                        }
                    }
                }
                if (players != null && players[i] != null)
                    onHit.Invoke(players[i]);
                if (failed)
                    onFail.Invoke(players[i]);
            }
        }
        else
        {
            if (failed)
                onFail.Invoke(null);
        }

        onFinish.Invoke(owner);
        onTrigger.Invoke(new CharacterCollection(players));
    }
}
