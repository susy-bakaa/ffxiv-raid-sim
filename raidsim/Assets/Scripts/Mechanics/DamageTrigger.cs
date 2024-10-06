using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using static ActionController;
using static GlobalStructs;
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
            Utilities.FunctionTimer.Create(() => m_collider.enabled = true, visualDelay, $"{id}_{damageName}_{gameObject}_{GetHashCode()}_visual_delay", false, true);
        }
        if (triggerDelay > 0f)
        {
            Utilities.FunctionTimer.Create(() => {
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
                Utilities.FunctionTimer.Create(() => inProgress = false, cooldown, $"{id}_{damageName}_{gameObject}_{GetHashCode()}_trigger_cooldown", false, true);
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
            Utilities.FunctionTimer.Create(() => inProgress = false, cooldown, $"{id}_{damageName}_{gameObject}_{GetHashCode()}_trigger_cooldown", false, true);
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

                        if (topEnmity && owner.partyList != null)
                        {
                            CharacterState highestEnmityMember = owner.partyList.GetHighestEnmityMember(players[i]);
                            long highestEnmity = 0;
                            highestEnmityMember.enmity.TryGetValue(players[i], out highestEnmity);
                            owner.ResetEnmity(players[i]);
                            enmity = highestEnmity;
                            Debug.Log($"topEnmity {highestEnmityMember.characterName} highestEnmity {highestEnmity} damageTrigger.enmity {enmity}");
                        }

                        long damageEnmity = Math.Abs(data.enmity);
                        damageEnmity += Math.Abs(Mathf.RoundToInt(data.damage.value * data.damageEnmityMultiplier));
                        enmity += damageEnmity;
                        Debug.Log($"enmity {enmity} damageTrigger.enmity {enmity}");
                        owner.AddEnmity(enmity, players[i]);
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
