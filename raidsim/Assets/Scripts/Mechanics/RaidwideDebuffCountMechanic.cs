using System.Collections.Generic;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;
using static dev.susybaka.raidsim.UI.PartyList;

namespace dev.susybaka.raidsim.Mechanics
{
    public class RaidwideDebuffCountMechanic : FightMechanic
    {
        public PartyList party;
        public List<StatusEffectCountPair> effectCountPairs = new List<StatusEffectCountPair>();

        private void Awake()
        {
            if (party == null)
            {
                party = FightTimeline.Instance.partyList;
            }
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!mechanicEnabled)
                return;

            foreach (PartyMember member in party.members)
            {
                for (int i = 0; i < effectCountPairs.Count; i++)
                {
                    if (member.characterState.HasEffect(effectCountPairs[i].effect.data.statusName, effectCountPairs[i].effect.tag))
                    {
                        int results = 0;
                        for (int j = 0; j < effectCountPairs[i].allowedEffects.Count; j++)
                        {
                            if (member.characterState.HasEffect(effectCountPairs[i].allowedEffects[j].data.statusName, effectCountPairs[i].allowedEffects[j].tag))
                            {
                                results++;
                            }
                        }
                        if (results < effectCountPairs[i].requiredAmount)
                        {
                            member.characterState.ModifyHealth(new Damage(100, true, true, Damage.DamageType.unique, Damage.ElementalAspect.unaspected, Damage.PhysicalAspect.none, Damage.DamageApplicationType.percentageFromMax, mechanicName));
                        }
                    }
                }
            }
        }
#if UNITY_EDITOR
        public void OnValidate()
        {
            for (int i = 0; i < effectCountPairs.Count; i++)
            {
                if (effectCountPairs[i].effect.data != null)
                {
                    StatusEffectCountPair pair = effectCountPairs[i];
                    pair.name = effectCountPairs[i].effect.name;
                    effectCountPairs[i] = pair;
                }
            }
        }
#endif
        [System.Serializable]
        public struct StatusEffectCountPair
        {
            public string name;
            public StatusEffectInfo effect;
            public List<StatusEffectInfo> allowedEffects;
            public int requiredAmount;

            public StatusEffectCountPair(string name, StatusEffectInfo effect, List<StatusEffectInfo> allowedEffects, int requiredAmount)
            {
                this.name = name;
                this.effect = effect;
                this.allowedEffects = allowedEffects;
                this.requiredAmount = requiredAmount;
            }
        }
    }
}