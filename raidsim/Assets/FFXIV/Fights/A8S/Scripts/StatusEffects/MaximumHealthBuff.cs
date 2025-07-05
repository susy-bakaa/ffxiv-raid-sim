using dev.susybaka.raidsim.Characters;

namespace dev.susybaka.raidsim.StatusEffects
{
    public class MaximumHealthBuff : StatusEffect
    {
        public float maxHealthModifier = 1.2f;

        public override void OnApplication(CharacterState state)
        {
            if (uniqueTag != 0)
                state.AddMaxHealth(maxHealthModifier, $"{data.statusName}_{uniqueTag}");
            else
                state.AddMaxHealth(maxHealthModifier, $"{data.statusName}");

            base.OnApplication(state);
        }

        public override void OnCleanse(CharacterState state)
        {
            if (uniqueTag != 0)
                state.RemoveMaxHealth($"{data.statusName}_{uniqueTag}");
            else
                state.RemoveMaxHealth($"{data.statusName}");

            base.OnCleanse(state);
        }

        public override void OnExpire(CharacterState state)
        {
            if (uniqueTag != 0)
                state.RemoveMaxHealth($"{data.statusName}_{uniqueTag}");
            else
                state.RemoveMaxHealth($"{data.statusName}");

            base.OnExpire(state);
        }
    }
}