using UnityEngine;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Targeting;
using dev.susybaka.raidsim.UI;

namespace dev.susybaka.raidsim.StatusEffects
{
    public class SetBossControlledState : MonoBehaviour
    {
        private PartyList party;
        private StatusEffect statusEffect;
        private TargetNode previousTarget;

        private enum TargetType
        {
            Random,
            Nearest,
            Furthest,
            LowestHealth,
            HighestHealth
        }

        [SerializeField] private bool log = false;
        [SerializeField] private bool setCharacterTarget = false;
        [SerializeField] private bool targetFriendlies = true;
        [SerializeField] private TargetType targetType = TargetType.Random;

        private string id = "SetBossControlled";

        private void Awake()
        {
            statusEffect = GetComponent<StatusEffect>();

            if (statusEffect != null)
            {
                id = $"{id}_{statusEffect.data.name}";
            }
        }

        public void SetTrue(CharacterState state)
        {
            party = state.partyList;

            if (state.bossController != null)
            {
                state.bossController.hasControl.SetFlag(id, true);

                if (log)
                    Debug.Log($"[SetBossControlledState ({gameObject.name})] Setting {state.GetCharacterName()} ({state.gameObject.name}) to boss controlled = true");

                if (setCharacterTarget)
                    SetTarget(state);
            }
        }

        public void SetFalse(CharacterState state) 
        {
            party = state.partyList;

            if (state.bossController != null)
            {
                state.bossController.hasControl.RemoveFlag(id);

                if (log)
                    Debug.Log($"[SetBossControlledState ({gameObject.name})] Setting {state.GetCharacterName()} ({state.gameObject.name}) to boss controlled = false");

                if (setCharacterTarget && state.targetController != null)
                    state.targetController.SetTarget(previousTarget);
            }
        }

        public void SetTarget(CharacterState state)
        {
            if (party != null)
            {
                CharacterState target = null;
                if (targetFriendlies)
                {
                    switch (targetType)
                    {
                        case TargetType.Nearest:
                            target = party.GetNearestMemberToMember(state);
                            break;
                        case TargetType.Furthest:
                            target = party.GetFurthestMemberFromMember(state);
                            break;
                        case TargetType.LowestHealth:
                            target = party.GetLowestHealthMemberToMember(state);
                            break;
                        case TargetType.HighestHealth:
                            target = party.GetHighestHealthMemberToMember(state);
                            break;
                        default:
                            target = party.GetRandomMemberExcluding(state, id);
                            break;
                    }
                }
                else
                {
                    // Implement targeting logic for enemies
                }

                if (target.targetController != null)
                {
                    previousTarget = state.targetController.currentTarget;

                    if (state.targetController != null)
                    {
                        state.targetController.SetTarget(target.targetController.self);
                    }
                    if (target != null)
                    {
                        state.bossController.SetTarget(target.targetController.self);
                        if (log)
                            Debug.Log($"[SetBossControlledState ({gameObject.name})] Setting {state.GetCharacterName()} ({state.gameObject.name}) targeting to {target.GetCharacterName()} ({target.gameObject.name}).");
                    }
                }
            }
        }
    }
}