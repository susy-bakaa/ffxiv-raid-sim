// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class DebuffMechanic : FightMechanic
    {
        public StatusEffectInfo effect;
        public bool applyToTarget = true;
        public bool cleans = false;
        public bool allowSubStatuses = false;
        [Tooltip("If true, the effect will be applied/removed without any popups or effects.")]
        public bool doSilently = false;

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (applyToTarget)
            {
                if (actionInfo.target != null)
                {
                    if (log)
                    {
                        Debug.Log($"[DebuffMechanic] '{effect.data.statusName}' applied to '{actionInfo.target.characterName}'");
                    }

                    if (!cleans)
                    {
                        for (int i = 0; i < effect.data.incompatableStatusEffects.Count; i++)
                        {
                            if (actionInfo.target.HasEffect(effect.data.incompatableStatusEffects[i].statusName))
                            {
                                return;
                            }
                        }
                        actionInfo.target.AddEffect(effect.data, actionInfo.source != null ? actionInfo.source : actionInfo.target, false, effect.tag, effect.stacks, -1, doSilently);
                    }
                    else
                    {
                        actionInfo.target.RemoveEffect(effect.data, false, actionInfo.source != null ? actionInfo.source : actionInfo.target, effect.tag, effect.stacks, doSilently);

                        if (allowSubStatuses)
                        {
                            if (effect.data.refreshStatusEffects != null && effect.data.refreshStatusEffects.Count > 0)
                            {
                                for (int i = 0; i < effect.data.refreshStatusEffects.Count; i++)
                                {
                                    if (actionInfo.target.HasEffect(effect.data.refreshStatusEffects[i].statusName))
                                    {
                                        actionInfo.target.RemoveEffect(effect.data.refreshStatusEffects[i], false, actionInfo.source != null ? actionInfo.source : actionInfo.target, effect.tag, effect.stacks, doSilently);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (actionInfo.source != null)
                {
                    if (!cleans)
                    {
                        for (int i = 0; i < effect.data.incompatableStatusEffects.Count; i++)
                        {
                            if (actionInfo.source.HasEffect(effect.data.incompatableStatusEffects[i].statusName))
                            {
                                return;
                            }
                        }
                        actionInfo.source.AddEffect(effect.data, actionInfo.source, false, effect.tag, effect.stacks);
                    }
                    else
                    {
                        actionInfo.source.RemoveEffect(effect.data, false, actionInfo.source, effect.tag, effect.stacks);

                        if (allowSubStatuses)
                        {
                            if (effect.data.refreshStatusEffects != null && effect.data.refreshStatusEffects.Count > 0)
                            {
                                for (int i = 0; i < effect.data.refreshStatusEffects.Count; i++)
                                {
                                    if (actionInfo.source.HasEffect(effect.data.refreshStatusEffects[i].statusName))
                                    {
                                        actionInfo.source.RemoveEffect(effect.data.refreshStatusEffects[i], false, actionInfo.source, effect.tag, effect.stacks);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}