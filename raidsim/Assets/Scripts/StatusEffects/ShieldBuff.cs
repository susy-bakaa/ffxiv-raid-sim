using dev.susybaka.raidsim.Characters;
using static dev.susybaka.raidsim.Characters.CharacterState;

namespace dev.susybaka.raidsim.StatusEffects
{
    public class ShieldBuff : StatusEffect
    {
        public bool allowAutoShield = true;
        public Shield shield;

        private void Start()
        {
            if (allowAutoShield)
            {
                if (!string.IsNullOrEmpty(damage.name) && damage.value != 0)
                    shield = new Shield(damage.name.Replace(" ", ""), damage.value);
            }
        }

        public override void OnApplication(CharacterState state)
        {
            if (allowAutoShield)
            {
                if (!string.IsNullOrEmpty(damage.name) && damage.value != 0)
                    shield = new Shield(damage.name.Replace(" ", ""), damage.value);
            }

            state.AddShield(shield.value, shield.key, damage.source, true);
            base.OnApplication(state);
        }

        public override void OnExpire(CharacterState state)
        {
            state.RemoveShield(shield.key);
            base.OnExpire(state);
        }

        public override void OnCleanse(CharacterState state)
        {
            state.RemoveShield(shield.key);
            base.OnCleanse(state);
        }
    }
}