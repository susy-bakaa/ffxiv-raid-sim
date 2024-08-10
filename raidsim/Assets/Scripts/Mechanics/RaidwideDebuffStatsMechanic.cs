using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using static ActionController;
using static GlobalStructs;
using static PartyList;
using static StatusEffectData;

public class RaidwideDebuffStatsMechanic : FightMechanic
{
    public PartyList party;
    public List<StatusEffectVitals> effectVitals = new List<StatusEffectVitals>();

    public UnityEvent<CharacterState> onFail;
    public UnityEvent<CharacterState> onSucceed;

    void Awake()
    {
        if (party == null)
        {
            party = FightTimeline.Instance.partyList;
        }
    }

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        base.TriggerMechanic(actionInfo);

        foreach (PartyMember member in party.members)
        {
            for (int i = 0; i < effectVitals.Count; i++)
            {
                if (member.characterState.HasEffect(effectVitals[i].effect.data.statusName, effectVitals[i].effect.tag))
                {
                    // Health Check
                    if (effectVitals[i].requiredHealth >= 0)
                    {
                        if (member.characterState.health < effectVitals[i].requiredHealth)
                        {
                            OnFail(member.characterState, effectVitals[i]);
                        }
                        else
                        {
                            OnSucceed(member.characterState, effectVitals[i]);
                        }
                    }
                    // Max Health Check
                    if (effectVitals[i].requiredMaxHealth >= 0)
                    {
                        if (member.characterState.currentMaxHealth < effectVitals[i].requiredMaxHealth)
                        {
                            OnFail(member.characterState, effectVitals[i]);
                        }
                        else
                        {
                            OnSucceed(member.characterState, effectVitals[i]);
                        }
                    }
                    // Has Most Health
                    if (effectVitals[i].hasMostHealth == true)
                    {
                        if (party.GetHighestHealthMember() != member.characterState)
                        {
                            OnFail(member.characterState, effectVitals[i]);
                        }
                        else
                        {
                            OnSucceed(member.characterState, effectVitals[i]);
                        }
                    }
                    // Has Least Health
                    if (effectVitals[i].hasLeastHealth == true)
                    {
                        if (party.GetLowestHealthMember() != member.characterState)
                        {
                            OnFail(member.characterState, effectVitals[i]);
                        }
                        else
                        {
                            OnSucceed(member.characterState, effectVitals[i]);
                        }
                    }
                }
            }
        }
    }

    public void OnFail(CharacterState character, StatusEffectVitals effectVitals)
    {
        if (effectVitals.damage.value != 0)
        {
            character.ModifyHealth(new Damage(effectVitals.damage, mechanicName));
        }
        onFail.Invoke(character);
    }

    public void OnSucceed(CharacterState character, StatusEffectVitals effectVitals)
    {
        onSucceed.Invoke(character);
    }
#if UNITY_EDITOR
    public void OnValidate()
    {
        for (int i = 0; i < effectVitals.Count; i++)
        {
            if (effectVitals[i].effect.data != null)
            {
                StatusEffectVitals pair = effectVitals[i];
                pair.name = effectVitals[i].effect.name;
                effectVitals[i] = pair;
            }
        }
    }
#endif
    [System.Serializable]
    public struct StatusEffectVitals
    {
        public string name;
        public StatusEffectInfo effect;
        public int requiredHealth;
        public int requiredMaxHealth;
        public bool hasMostHealth;
        public bool hasLeastHealth;
        public Damage damage;

        public StatusEffectVitals(string name, StatusEffectInfo effect, int requiredHealth, int requiredMaxHealth, bool hasMostHealth, bool hasLeastHealth, Damage damage)
        {
            this.name = name;
            this.effect = effect;
            this.requiredHealth = requiredHealth;
            this.requiredMaxHealth = requiredMaxHealth;
            this.hasMostHealth = hasMostHealth;
            this.hasLeastHealth = hasLeastHealth;
            this.damage = damage;
        }
    }
}
