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
    public Damage damage;
    public float visualDelay = 0f;
    public float damageApplicationDelay = 0.25f;
    public bool failWipes = false;
    public bool cleaves = true;
    public bool shared = false;
    public bool enumeration = false;
    public int playersRequired = 0;
    public List<CharacterState> players = new List<CharacterState>();
    public List<StatusEffectData> effects = new List<StatusEffectData>();
    public UnityEvent<CharacterState> onHit;
    public UnityEvent<CharacterState> onFail;
    public UnityEvent<CharacterState> onFinish;

    private bool inProgress = false;

    void Awake()
    {
        m_collider = GetComponent<Collider>();

        if (failWipes)
            onFail.AddListener((CharacterState characterState) => { FightTimeline.Instance.WipeParty(); });
    }

    void OnEnable()
    {
        if (m_collider != null && visualDelay > 0f)
        {
            m_collider.enabled = false;
            Utilities.FunctionTimer.Create(() => m_collider.enabled = true, visualDelay, $"{damageName}_{gameObject}_visual_delay", false, true);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.transform.parent.TryGetComponent(out CharacterState playerState))
            {
                if (!cleaves && owner == null)
                    return;
                if (!cleaves && owner != null && playerState != owner)
                    return;

                players.Add(playerState);
                if (!inProgress)
                    StartCoroutine(StartDamageTrigger());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.transform.parent.TryGetComponent(out CharacterState playerState))
            {
                if (!cleaves && owner == null)
                    return;
                if (!cleaves && owner != null && playerState != owner)
                    return;

                players.Remove(playerState);
            }
        }
    }

    private IEnumerator StartDamageTrigger()
    {
        inProgress = true;
        yield return new WaitForSeconds(damageApplicationDelay);
        TriggerDamage();
        inProgress = false;
    }

    public void TriggerDamage()
    {
        Damage damagePerPlayer = shared ? new Damage(damage, Mathf.RoundToInt(damage.value / players.Count)) : damage;
        bool failed = false;
        bool kill = false;

        if (enumeration && playersRequired > 0)
        {
            if (players.Count != playersRequired)
            {
                damagePerPlayer = new Damage(damagePerPlayer, -999999);
                failed = true;
                kill = true;
            }
        }

        if (damage.value <= -999999)
        {
            damagePerPlayer = new Damage(damagePerPlayer, -999999);
            kill = true;
        }

        for (int i = 0; i < players.Count; i++)
        {
            players[i].ModifyHealth(damagePerPlayer, kill);
            if (effects.Count > 0)
            {
                for (int k = 0; k < effects.Count; k++)
                {
                    players[i].AddEffect(effects[k]);
                }
            }
            onHit.Invoke(players[i]);
            if (failed)
                onFail.Invoke(players[i]);
        }

        onFinish.Invoke(owner);
    }

    public void OnDestroy()
    {
        if (failWipes)
            onFail.RemoveListener((CharacterState characterState) => { FightTimeline.Instance.WipeParty(); });
    }
}
