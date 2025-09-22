// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using dev.susybaka.raidsim.Characters;

namespace dev.susybaka.raidsim.StatusEffects
{
    public class SurecastBuff : StatusEffect
    {
        public override void OnApplication(CharacterState state)
        {
            state.knockbackResistant.SetFlag(data.statusName, true);
        }

        public override void OnExpire(CharacterState state)
        {
            state.knockbackResistant.SetFlag(data.statusName, false);
            base.OnExpire(state);
        }

        public override void OnCleanse(CharacterState state)
        {
            state.knockbackResistant.SetFlag(data.statusName, false);
            base.OnCleanse(state);
        }
    }
}