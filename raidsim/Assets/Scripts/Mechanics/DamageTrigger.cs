using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using static ActionController;
using static GlobalStructs;
using static UnityEngine.ParticleSystem;
using Random = UnityEngine.Random;

public class DamageTrigger : MonoBehaviour
{
    Collider m_collider;
    public CharacterActionData data;

    public string damageName = string.Empty;
    public bool inverted = false;
    public CharacterState owner;
    public bool autoAssignOwner = false;
    public string ableToHitTag = "Player";
    public Damage damage;
    public long enmity = 0;
    public bool increaseEnmity = false;
    public bool topEnmity = false;
    public bool initializeOnStart = true;
    public bool self = false;
    public bool dealsDamage = true;
    public bool passDamage = false;
    public bool isAShield = false;
    public bool cleaves = true;
    public bool ignoresOwner = false;
    public bool playerActivated = true;
    public bool shared = false;
    public bool enumeration = false;
    public float visualDelay = 0f;
    public float triggerDelay = 0f;
    public float damageApplicationDelay = 0.25f;
    public float cooldown = 10f;
    public int playersRequired = 0;
    public List<CharacterState> currentPlayers = new List<CharacterState>();
    public List<StatusEffectData> appliedEffects = new List<StatusEffectData>();
    public UnityEvent<CharacterState> onHit;
    public UnityEvent<CharacterState> onFail;
    public UnityEvent<CharacterState> onFinish;
    public UnityEvent onSpawn;
    public UnityEvent<CharacterCollection> onTrigger;

    private int id = 0;
    private bool inProgress = false;
    private bool initialized = false;

    void Awake()
    {
        m_collider = GetComponent<Collider>();
        id = Random.Range(1000,10000);
    }

    void Start()
    {
        onSpawn.Invoke();

        if (initializeOnStart && !initialized)
            Initialize();
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

        if (delay > 0f)
        {
            visualDelay = delay;
            triggerDelay = delay + 0.1f;
        }

        if (m_collider != null && visualDelay > 0f)
        {
            m_collider.enabled = false;
            Utilities.FunctionTimer.Create(this, () => m_collider.enabled = true, visualDelay, $"{id}_{damageName}_{gameObject}_{GetHashCode()}_visual_delay", false, true);
        }
        if (triggerDelay > 0f)
        {
            Utilities.FunctionTimer.Create(this, () => {
                if (!inProgress)
                    StartDamageTrigger();
            }, triggerDelay, $"{id}_{damageName}_{gameObject}_{GetHashCode()}_trigger_delay", false, true);
        }
        else if (!playerActivated)
        {
            if (!inProgress)
                StartDamageTrigger();
        }

        initialized = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!inverted)
        {
            if (other.CompareTag(ableToHitTag))
            {
                if (other.transform.parent.TryGetComponent(out CharacterState playerState))
                {
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
                if (other.transform.parent.TryGetComponent(out CharacterState playerState))
                {
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

    void OnTriggerExit(Collider other)
    {
        if (!inverted)
        {
            if (other.CompareTag(ableToHitTag))
            {
                if (other.transform.parent.TryGetComponent(out CharacterState playerState))
                {
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
                if (other.transform.parent.TryGetComponent(out CharacterState playerState))
                {
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
        if (damageApplicationDelay > 0)
        {
            StartCoroutine(IE_StartDamageTrigger(currentPlayers.ToArray()));
        }
        else
        {
            inProgress = true;
            TriggerDamage(currentPlayers.ToArray());
            if (cooldown > 0f)
            {
                Utilities.FunctionTimer.Create(this, () => inProgress = false, cooldown, $"{id}_{damageName}_{gameObject}_{GetHashCode()}_trigger_cooldown", false, true);
            }
            else
            {
                inProgress = false;
            }
        }
    }

    private IEnumerator IE_StartDamageTrigger(CharacterState[] players)
    {
        inProgress = true;
        yield return new WaitForSeconds(damageApplicationDelay);
        TriggerDamage(players);
        if (cooldown > 0f)
        {
            Utilities.FunctionTimer.Create(this, () => inProgress = false, cooldown, $"{id}_{damageName}_{gameObject}_{GetHashCode()}_trigger_cooldown", false, true);
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

        string newName = damage.name;

        if (!string.IsNullOrEmpty(damageName))
        {
            newName = damageName;
        }

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
                    //Debug.Log("DamageTrigger failed");
                    failed = true;
                    if (damagePerPlayer.value < 0)
                    {
                        //damagePerPlayer = new Damage(damagePerPlayer, -999999);
                        //kill = true;
                    }
                }
            }
        }

        if (damage.value <= -999999)
        {
            damagePerPlayer = new Damage(damagePerPlayer, -999999);
            kill = true;
        }

        if (players.Length > 0)
        {
            if (increaseEnmity && owner != null && data != null)
            {
                // First, collect all enemies from the players' enmity lists
                HashSet<CharacterState> enemiesToAdd = new HashSet<CharacterState>();

                // Iterate over the players list to gather the enemies they have enmity for
                foreach (var player in players)
                {
                    if (ignoresOwner)
                    {
                        if (player == owner)
                        {
                            continue;
                        }
                    }

                    foreach (var enemy in player.enmity.Keys)
                    {
                        // Add the enemy to the set if it's not already there
                        if (!enemiesToAdd.Contains(enemy))
                        {
                            enemiesToAdd.Add(enemy);
                        }
                    }
                }

                // Now, iterate over the gathered enemies and add enmity for each one to the owner
                foreach (var enemy in enemiesToAdd)
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

                if (dealsDamage)
                {
                    if (damagePerPlayer.value != 0)
                        players[i].ModifyHealth(damagePerPlayer, kill);

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

                                    // Debugging the individual healing enmity calculation
                                    Debug.Log($"Enmity for {targetPlayer.characterName} amounts to {healingEnmityForPlayer}. Current enmity: {currentEnmity}");

                                    // Add the healing enmity to the current enmity value
                                    currentEnmity += healingEnmityForPlayer;

                                    // Debugging the total enmity being set
                                    Debug.Log($"Total enmity for {targetPlayer.characterName} after addition: {currentEnmity}");

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
                            players[i].AddEffect(appliedEffects[k], damage, self);
                        }
                        else
                        {
                            players[i].AddEffect(appliedEffects[k], self);
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
