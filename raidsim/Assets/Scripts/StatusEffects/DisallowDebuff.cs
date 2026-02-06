// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;

namespace dev.susybaka.raidsim.StatusEffects
{
    public class DisallowDebuff : StatusEffect
    {
        public bool disallowMovement = true;
        public bool disallowActions = true;
        public float updateInterval = 0.2f;
        public UnityEvent<CharacterState> onSuccess;
        public UnityEvent<CharacterState> onFail;

        private bool violated = false;
        private Vector3 lastPosition;
        private CharacterAction lastAction;
        private float timer = 0f;

        public override void OnApplication(CharacterState state)
        {
            base.OnApplication(state);

            lastPosition = state.transform.position;
            lastAction = state.actionController.LastAction;
        }

        public override void OnUpdate(CharacterState state)
        {
            base.OnUpdate(state);

            timer += Time.deltaTime;

            if (timer < updateInterval)
                return;
            else
                timer = 0f;

            if (disallowMovement)
            {
                if (state.transform.position != lastPosition)
                {
                    violated = true;
                    onFail?.Invoke(state);
                }
            }
            if (disallowActions)
            {
                if (lastAction != null && lastAction != state.actionController.LastAction)
                {
                    violated = true;
                    onFail?.Invoke(state);
                }
            }
        }

        public override void OnExpire(CharacterState state)
        {
            base.OnExpire(state);

            if (!violated)
                onSuccess?.Invoke(state);
        }

        public override void OnCleanse(CharacterState state)
        {
            base.OnCleanse(state);

            if (!violated)
                onSuccess?.Invoke(state);
        }
    }
}