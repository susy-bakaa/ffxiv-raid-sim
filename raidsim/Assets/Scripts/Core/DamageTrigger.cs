using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class DamageTrigger : MonoBehaviour
{
    public enum DamageApplicationType { normal, percentage, percentageFromMax, set }

    public string damageName = "Unnamed Damage";
    public CharacterState owner;
    public int damage = -1000;
    public DamageApplicationType applicationType = DamageApplicationType.normal;
    public float delay = 0.25f;
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
        if (failWipes)
            onFail.AddListener((CharacterState characterState) => { FightTimeline.Instance.WipeParty(); });
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
        yield return new WaitForSeconds(delay);
        TriggerDamage();
        inProgress = false;
    }

    public void TriggerDamage()
    {
        int damagePerPlayer = shared ? Mathf.RoundToInt(damage / players.Count) : damage;
        bool failed = false;
        bool kill = false;

        if (enumeration && playersRequired > 0)
        {
            if (players.Count != playersRequired)
            {
                damagePerPlayer = -999999;
                failed = true;
                kill = true;
            }
        }

        if (damage <= -999999)
        {
            damagePerPlayer = -999999;
            kill = true;
        }

        for (int i = 0; i < players.Count; i++)
        {
            float percentage = 0f;

            switch (applicationType)
            {
                default:
                {
                    if (damagePerPlayer > 0)
                        players[i].ModifyHealth(damagePerPlayer, kill);
                    break;
                }
                case DamageApplicationType.percentage:
                {
                    percentage = Mathf.Abs(damagePerPlayer) / 100f;

                    if (percentage > 1f)
                        percentage = 1f;
                    else if (percentage < 0f)
                        percentage = 0f;

                    if (percentage > 0f)
                        players[i].RemoveHealth(damagePerPlayer / 100.0f, false, kill);
                    break;
                }
                case DamageApplicationType.percentageFromMax:
                {
                    percentage = Mathf.Abs(damagePerPlayer) / 100f;

                    if (percentage > 1f)
                        percentage = 1f;
                    else if (percentage < 0f)
                        percentage = 0f;

                    if (percentage > 0f)
                        players[i].RemoveHealth(damagePerPlayer / 100.0f, true, kill);
                    break;
                }
                case DamageApplicationType.set:
                {
                    int damageAbs = Mathf.Abs(damage);
                    if (damageAbs > 0)
                        players[i].SetHealth(damageAbs, kill);
                    break;
                }
            }
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
