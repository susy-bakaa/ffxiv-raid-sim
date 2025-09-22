// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.UI;
using dev.susybaka.raidsim.Visuals;

namespace dev.susybaka.raidsim.StatusEffects
{
    public class StoreLocationEffect : StatusEffect
    {
        public Vector3 location;
        public GameObject effectPrefab;
        public float fadeDuration = 0.5f;

        private GameObject spawnedEffectObject;
        private SimpleShaderFade shaderFade;
        private SortHotbar sortHotbar;

        private void Awake()
        {
            sortHotbar = FindObjectOfType<SortHotbar>();
        }

        public override void OnApplication(CharacterState state)
        {
            base.OnApplication(state);

            location = state.transform.position;

            if (effectPrefab != null)
            {
                spawnedEffectObject = Instantiate(effectPrefab, location, Quaternion.identity);
                spawnedEffectObject.transform.SetParent(GameObject.Find("Mechanics").transform);
                if (spawnedEffectObject.transform.GetChild(0).TryGetComponent(out SimpleShaderFade shaderFade))
                {
                    this.shaderFade = shaderFade;
                    shaderFade.FadeIn(fadeDuration);
                }
            }

            if (sortHotbar != null)
            {
                sortHotbar.UpdateSorting();
            }
        }

        public override void OnCleanse(CharacterState state)
        {
            base.OnCleanse(state);
            CleanObjects();
        }

        public override void OnExpire(CharacterState state)
        {
            base.OnExpire(state);
            CleanObjects();
        }

        private void CleanObjects()
        {
            if (spawnedEffectObject != null)
            {
                if (shaderFade != null)
                {
                    shaderFade.FadeOut(fadeDuration);
                }
                Destroy(spawnedEffectObject, fadeDuration);
            }

            if (sortHotbar != null)
            {
                sortHotbar.UpdateSorting();
            }
        }
    }
}