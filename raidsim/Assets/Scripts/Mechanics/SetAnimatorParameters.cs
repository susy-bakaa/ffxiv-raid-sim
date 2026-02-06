// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using System.Collections.Generic;
using dev.susybaka.raidsim.Characters;
using UnityEngine;

namespace dev.susybaka.raidsim.Mechanics
{
    public class SetAnimatorParameters : MonoBehaviour
    {
        public bool log = false;
        public float delay = -1f;
        public List<AnimatorParameterData> parameters;
        public Dictionary<string, int> parameterHashes = new Dictionary<string, int>();

        private void Awake()
        {
            if (log)
                Debug.Log($"[SetAnimatorParameters ({gameObject.name})] Caching parameter hashes for {parameters.Count} parameters.", gameObject);
            foreach (var param in parameters)
            {
                parameterHashes[param.name] = Animator.StringToHash(param.name);
            }
        }

        public void SetParameters(CharacterState character)
        {
            if (delay > 0f)
            {
                StartCoroutine(IE_SetParametersWithDelay(character, new WaitForSeconds(delay)));
            }
            else
            {
                SetParameters(character.modelHandler.GetCurrentAnimator());
            }
        }

        public void SetParameters(Animator animator)
        {
            if (log)
                Debug.Log($"[SetAnimatorParameters ({gameObject.name})] Setting {parameters.Count} parameters on animator {animator.gameObject.name}.", animator.gameObject);

            foreach (var param in parameters)
            {
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(parameterHashes[param.name], param.boolValue);
                        break;
                    case AnimatorControllerParameterType.Int:
                        animator.SetInteger(parameterHashes[param.name], param.intValue);
                        break;
                    case AnimatorControllerParameterType.Float:
                        animator.SetFloat(parameterHashes[param.name], param.floatValue);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        animator.SetTrigger(parameterHashes[param.name]);
                        break;
                }
            }
        }

        private IEnumerator IE_SetParametersWithDelay(CharacterState character, WaitForSeconds wait)
        {
            yield return wait;
            SetParameters(character.modelHandler.GetCurrentAnimator());
        }

        [System.Serializable]
        public struct AnimatorParameterData
        {
            public string name;
            public AnimatorControllerParameterType type;
            public bool boolValue;
            public int intValue;
            public float floatValue;
        }
    }
}