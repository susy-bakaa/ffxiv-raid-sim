using UnityEngine;
using dev.susybaka.raidsim.Characters;

namespace dev.susybaka.raidsim.StatusEffects
{
    public class DamageReductionDebuff : StatusEffect
    {
        [Header("Function")]
        public float damageOutputModifier = 0.7f;

        public override void OnApplication(CharacterState state)
        {
            base.OnApplication(state);
        }

        public override void OnExpire(CharacterState state)
        {
            base.OnExpire(state);
        }

        public override void OnCleanse(CharacterState state)
        {
            base.OnCleanse(state);
        }
    }
}