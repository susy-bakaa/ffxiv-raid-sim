using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static ActionController;
using static StatusEffectData;

public class DebuffMechanic : FightMechanic
{
    public StatusEffectInfo effect;
    public bool applyToTarget = true;
    public bool cleans = false;
    public bool allowSubStatuses = false;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        base.TriggerMechanic(actionInfo);

        if (applyToTarget)
        {
            if (actionInfo.target != null)
            {
                if (!cleans)
                {
                    for (int i = 0; i < effect.data.incompatableStatusEffects.Count; i++)
                    {
                        if (actionInfo.target.HasEffect(effect.data.incompatableStatusEffects[i].statusName))
                        {
                            return;
                        }
                    }
                    actionInfo.target.AddEffect(effect.data, false, effect.tag, effect.stacks);
                }
                else
                {
                    actionInfo.target.RemoveEffect(effect.data, false, effect.tag, effect.stacks);

                    if (allowSubStatuses)
                    {
                        if (effect.data.refreshStatusEffects != null && effect.data.refreshStatusEffects.Count > 0)
                        {
                            for (int i = 0; i < effect.data.refreshStatusEffects.Count; i++)
                            {
                                if (actionInfo.target.HasEffect(effect.data.refreshStatusEffects[i].statusName))
                                {
                                    actionInfo.target.RemoveEffect(effect.data.refreshStatusEffects[i], false, effect.tag, effect.stacks);
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
                    actionInfo.source.AddEffect(effect.data, false, effect.tag, effect.stacks);
                }
                else
                {
                    actionInfo.source.RemoveEffect(effect.data, false, effect.tag, effect.stacks);

                    if (allowSubStatuses)
                    {
                        if (effect.data.refreshStatusEffects != null && effect.data.refreshStatusEffects.Count > 0)
                        {
                            for (int i = 0; i < effect.data.refreshStatusEffects.Count; i++)
                            {
                                if (actionInfo.source.HasEffect(effect.data.refreshStatusEffects[i].statusName))
                                {
                                    actionInfo.source.RemoveEffect(effect.data.refreshStatusEffects[i], false, effect.tag, effect.stacks);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
