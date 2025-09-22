// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.Animations
{
    public class ModelHandler : MonoBehaviour
    {
        public int currentModelIndex = 0;
        private int originalModelIndex = -1;
        public List<GameObject> models = new List<GameObject>();
        public UnityEvent<Animator> onCharacterModelSwapped;

        private CharacterState characterState;
        private Animator activeAnimator;
        private AnimatorController activeAnimatorController;
        private Dictionary<string, int> hashedParameters = new Dictionary<string, int>();

        private void Awake()
        {
            transform.TryGetComponentInParents(out characterState);

            originalModelIndex = currentModelIndex;

            models = new List<GameObject>();

            for (int i = 0; i < transform.childCount; i++)
            {
                models.Add(transform.GetChild(i).gameObject);
            }

            if (FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.AddListener(ResetModelState);
            }

            hashedParameters = new Dictionary<string, int>();
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < models.Count; i++)
            {
                if (models[i] == null)
                    continue;

                models[i].SetActive(i == currentModelIndex);
                if (models[i].activeSelf)
                {
                    models[i].TryGetComponent(out activeAnimator);
                    if (activeAnimator != null)
                    {
                        activeAnimator.TryGetComponent(out activeAnimatorController);
                    }
                }
            }
        }

        public void ResetModelState()
        {
            if (characterState != null)
                ResetCharacterModel();
            else
                ResetModel();
        }

        public void ResetCharacterModel()
        {
            for (int i = 0; i < models.Count; i++)
            {
                if (models[i] == null)
                    continue;

                models[i].SetActive(i == originalModelIndex);
                if (models[i].activeSelf)
                {
                    models[i].TryGetComponent(out activeAnimator);
                    if (activeAnimator != null)
                    {
                        activeAnimator.TryGetComponent(out activeAnimatorController);
                    }
                }
            }

            if (activeAnimator != null)
            {
                SetAnimator(activeAnimator);
                onCharacterModelSwapped.Invoke(activeAnimator);
            }
        }

        public void ResetModel()
        {
            for (int i = 0; i < models.Count; i++)
            {
                models[i].SetActive(i == originalModelIndex);
                if (models[i].activeSelf)
                {
                    models[i].TryGetComponent(out activeAnimator);
                    if (activeAnimator != null)
                    {
                        activeAnimator.TryGetComponent(out activeAnimatorController);
                    }
                }
            }
        }

        public void UpdateModels()
        {
            models.Clear();
            Debug.Log($"UpdateModels {transform.childCount}");
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                models.Add(child.gameObject);
            }

            for (int i = 0; i < models.Count; i++)
            {
                models[i].SetActive(i == currentModelIndex);
                if (models[i].activeSelf)
                {
                    models[i].TryGetComponent(out activeAnimator);
                    if (activeAnimator != null)
                    {
                        activeAnimator.TryGetComponent(out activeAnimatorController);
                    }
                }
            }

            if (activeAnimator != null)
            {
                SetAnimator(activeAnimator);
                onCharacterModelSwapped.Invoke(activeAnimator);
            }
        }

        public void SwitchActiveCharacterModel(int index)
        {
            if (index < 0 || index >= models.Count)
                return;

            if (characterState == null)
                return;

            for (int i = 0; i < models.Count; i++)
            {
                models[i].SetActive(i == index);
                if (models[i].activeSelf)
                {
                    models[i].TryGetComponent(out activeAnimator);
                    if (activeAnimator != null)
                    {
                        activeAnimator.TryGetComponent(out activeAnimatorController);
                    }
                }
            }

            if (activeAnimator != null)
            {
                SetAnimator(activeAnimator);
                onCharacterModelSwapped.Invoke(activeAnimator);
            }
        }

        public void SwitchActiveModel(int index)
        {
            if (index < 0 || index >= models.Count)
                return;

            for (int i = 0; i < models.Count; i++)
            {
                models[i].SetActive(i == index);
            }
        }

        public void SetAnimator(Animator animator)
        {
            if (characterState == null)
                return;

            characterState.SetAnimator(animator);
        }

        public void SetTrigger(string trigger)
        {
            if (activeAnimator == null && activeAnimatorController == null)
                return;

            if (!hashedParameters.TryGetValue(trigger, out int hash))
            {
                hash = Animator.StringToHash(trigger);
                hashedParameters.Add(trigger, hash);
            }

            if (activeAnimatorController != null)
            {
                activeAnimatorController.SetTrigger(hash);
            }
            else
            {
                activeAnimator.SetTrigger(hash);
            }
        }

        public void SetBooleanTrue(string boolean)
        {
            if (activeAnimator == null && activeAnimatorController == null)
                return;

            if (!hashedParameters.TryGetValue(boolean, out int hash))
            {
                hash = Animator.StringToHash(boolean);
                hashedParameters.Add(boolean, hash);
            }

            if (activeAnimatorController != null)
            {
                activeAnimatorController.SetBool(hash, true);
            }
            else
            {
                activeAnimator.SetBool(hash, true);
            }
        }

        public void SetBooleanFalse(string boolean)
        {
            if (activeAnimator == null)
                return;

            if (!hashedParameters.TryGetValue(boolean, out int hash))
            {
                hash = Animator.StringToHash(boolean);
                hashedParameters.Add(boolean, hash);
            }

            if (activeAnimatorController != null)
            {
                activeAnimatorController.SetBool(hash, false);
            }
            else
            {
                activeAnimator.SetBool(hash, false);
            }
        }
    }
}