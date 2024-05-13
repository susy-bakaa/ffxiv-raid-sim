using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class DamageTrigger : MonoBehaviour
{
    public CharacterState owner;
    public int damage = -1000;
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

        if (enumeration && playersRequired > 0)
        {
            if (players.Count != playersRequired)
            {
                damagePerPlayer = -999999;
                failed = true;
            }
        }

        for (int i = 0; i < players.Count; i++)
        {
            players[i].ModifyHealth(damagePerPlayer);
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
    }

    public void OnDestroy()
    {
        if (failWipes)
            onFail.RemoveListener((CharacterState characterState) => { FightTimeline.Instance.WipeParty(); });
    }
}
