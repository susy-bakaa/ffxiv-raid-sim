using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using static GlobalStructs;

public class DamageTrigger : MonoBehaviour
{
    Collider m_collider;

    public string damageName = string.Empty;
    public CharacterState owner;
    public bool autoAssignOwner = false;
    public Damage damage;
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
        if (initializeOnStart && !initialized)
            Initialize();
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
        if (other.CompareTag("Player"))
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

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
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

    private void StartDamageTrigger()
    {
        Debug.Log($"Players {currentPlayers.ToArray().Length}");

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
                    failed = true;
                    if (damagePerPlayer.value < 0)
                    {
                        damagePerPlayer = new Damage(damagePerPlayer, -999999);
                        kill = true;
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
                Debug.Log(players[i].characterName);

                if (ignoresOwner)
                {
                    if (players[i] == owner)
                    {
                        continue;
                    }
                }

                if (damagePerPlayer.value != 0 && dealsDamage)
                {
                    if (!isAShield)
                    {
                        players[i].ModifyHealth(damagePerPlayer, kill);
                    }
                    else
                    {
                        players[i].AddShield(damagePerPlayer.value, damage.name);
                    }
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
    }
}
