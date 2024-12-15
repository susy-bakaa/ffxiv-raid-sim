using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatusEffectData;
using static ActionController;
using static TriggerRandomMechanic;
using UnityEngine.UIElements;
using static CharacterState;

public class DebuffsMechanic : FightMechanic
{
    public List<StatusEffectInfo> effects = new List<StatusEffectInfo>();
    public List<IndexMapping> indexMapping = new List<IndexMapping>();
    public List<RoleMapping> roleMapping = new List<RoleMapping>();
    public int fightTimelineEventRandomResultId = -1;
    public bool cleansEffect = false;
    public bool ignoreRoles = false;
    public bool useIndexMapping = false;
    public bool useRoleMapping = false;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        CharacterState state = null;
        int r = FightTimeline.Instance.GetRandomEventResult(fightTimelineEventRandomResultId);

        if (actionInfo.source != null)
        {
            state = actionInfo.source;
        } 
        else if (actionInfo.target != null)
        {
            state = actionInfo.target;
        }

        if (log)
            Debug.Log($"{gameObject.name} character state is {state.gameObject.name}");

        if (effects != null && effects.Count > 0)
        {
            if (!cleansEffect)
            {
                bool flag = false;

                if (useIndexMapping)
                {
                    for (int i = 0; i < indexMapping.Count; i++)
                    {
                        if (indexMapping[i].previousIndex == r)
                        {
                            r = indexMapping[i].nextIndex;
                            break;
                        }
                    }
                    flag = true;
                }
                if (useRoleMapping)
                {
                    for (int i = 0; i < roleMapping.Count; i++)
                    {
                        if (roleMapping[i].role == state.role)
                        {
                            r += roleMapping[i].indexOffset;
                            if (log)
                                Debug.Log($"{gameObject.name} roleMapping role is of index {i} and is {roleMapping[i].role}, r is {r}");
                            break;
                        }
                    }
                    flag = true;
                }

                if (flag)
                {
                    if (!ignoreRoles)
                    {
                        if (effects[r].data.assignedRoles.Contains(state.role))
                        {
                            if (log)
                            {
                                Debug.Log($"{gameObject.name} effects[r] {effects[r].name} contains {state.gameObject.name} role {state.role}\nAdding effect...");
                            }
                            state.AddEffect(effects[r].data, state, false, effects[r].tag, effects[r].stacks);
                        }
                    }
                    else
                    {
                        state.AddEffect(effects[r].data, state, false, effects[r].tag, effects[r].stacks);
                    }
                }
                else
                {
                    for (int i = 0; i < effects.Count; i++)
                    {
                        if (!ignoreRoles)
                        {
                            if (!effects[i].data.assignedRoles.Contains(state.role))
                                continue;
                        }

                        state.AddEffect(effects[i].data, state, false, effects[i].tag, effects[i].stacks);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public struct RoleMapping
    {
        public string name;
        public Role role;
        public int indexOffset;

        public RoleMapping(string name, Role role, int indexOffset)
        {
            this.name = name;
            this.role = role;
            this.indexOffset = indexOffset;
        }
    }
}
