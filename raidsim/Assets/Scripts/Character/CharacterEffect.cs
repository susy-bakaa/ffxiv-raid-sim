// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using NaughtyAttributes;
#endif
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Visuals;

namespace dev.susybaka.raidsim.Characters
{
    public class CharacterEffect : MonoBehaviour
    {
        public bool visible = false;
        private bool wasVisible;
        public bool enableOnStart = false;
        private bool wasEnabledOnStart;
        public bool toggleObject = true;
        public float fadeTime = 0.33f;
        public bool log = false;

        private SimpleShaderFade[] shaderFade;
        private bool hasShaderFade = false;
        private SimpleParticleEffect[] particleEffects;
        private bool hasParticleEffects = false;
        private Coroutine ieDisableEffect;
        private GameObject targetObject;
        private Coroutine ieOnReset;
        private bool reset = false;
        private bool hasStarted = false;

#if UNITY_EDITOR
        [Button("Toggle Effect")]
        public void ToggleEffectButton()
        {
            ToggleEffect();
        }
#endif

        private void Awake()
        {
            shaderFade = transform.GetComponentsInChildren<SimpleShaderFade>();
            particleEffects = transform.GetComponentsInChildren<SimpleParticleEffect>();
            targetObject = transform.GetChild(0).gameObject;
            if (shaderFade != null && shaderFade.Length > 0)
                hasShaderFade = true;
            else
                hasShaderFade = false;
            if (particleEffects != null && particleEffects.Length > 0)
                hasParticleEffects = true;
            else
                hasParticleEffects = false;

            wasVisible = visible;
            wasEnabledOnStart = enableOnStart;
            hasStarted = false;
        }

        private void Start()
        {
            if (enableOnStart)
            {
                visible = false;
                EnableEffect();
            }
            else
            {
                visible = true;
                DisableEffect();
            }
        }

        private void OnEnable()
        {
            if (FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.AddListener(OnReset);
            }
        }

        private void OnDisable()
        {
            if (FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.RemoveListener(OnReset);
            }
        }

        private void OnDestroy()
        {
            if (FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.RemoveListener(OnReset);
            }
        }

        private void OnReset()
        {
            visible = wasVisible;
            enableOnStart = wasEnabledOnStart;
            reset = true;

            StopAllCoroutines();
            ieDisableEffect = null;
            ieOnReset = null;

            if (gameObject.activeSelf && gameObject.activeInHierarchy)
            {
                if (enableOnStart)
                {
                    Start();
                }
                else
                {
                    if (visible)
                        EnableEffect();
                    else
                        DisableEffect();
                }

                if (ieOnReset == null)
                    ieOnReset = StartCoroutine(IE_OnReset(new WaitForSecondsRealtime(1f)));
            }
        }

        private IEnumerator IE_OnReset(WaitForSecondsRealtime wait)
        {
            yield return wait;
            reset = false;
            ieOnReset = null;
        }

        public void ToggleEffect()
        {
            if (visible)
                DisableEffect();
            else
                EnableEffect();
        }

        public void EnableEffect()
        {
            if (visible && !reset)
                return;

            if (log)
                Debug.Log($"[CharacterEffect ({gameObject.name})] EnableEffect was called!");

            bool hasAnything = false;

            if (hasParticleEffects)
            {
                for (int i = 0; i < particleEffects.Length; i++)
                {
                    if (toggleObject)
                        particleEffects[i].gameObject.SetActive(true);
                    particleEffects[i].Play();
                    hasAnything = true;
                }
            }
            if (hasShaderFade)
            {
                for (int i = 0; i < shaderFade.Length; i++)
                {
                    if (toggleObject)
                        shaderFade[i].gameObject.SetActive(true);
                    if (hasStarted)
                        shaderFade[i].FadeIn(fadeTime);
                    else
                        shaderFade[i].FadeIn(0); // If it's the first time starting, we want to skip the fade in time to avoid all effects fading at the same time running out of tweens.
                    hasAnything = true;
                }
            }
            if (!hasAnything)
            {
                if (toggleObject)
                    targetObject.SetActive(true);
            }
            visible = true;
            hasStarted = true;
        }

        public void DisableEffect()
        {
            if (!visible && !reset)
                return;

            if (log)
                Debug.Log($"[CharacterEffect ({gameObject.name})] DisableEffect was called!");

            bool hasAnything = false;

            if (hasShaderFade)
            {
                for (int i = 0; i < shaderFade.Length; i++)
                {
                    if (hasStarted)
                        shaderFade[i].FadeOut(fadeTime);
                    else
                        shaderFade[i].FadeOut(0); // If it's the first time starting, we want to skip the fade in time to avoid all effects fading at the same time running out of tweens.
                }
                if (toggleObject && ieDisableEffect == null)
                {
                    WaitForSeconds wait = new WaitForSeconds(fadeTime + 0.1f);
                    ieDisableEffect = StartCoroutine(IE_DisableEffect(wait));
                }
                hasAnything = true;
            }
            if (hasParticleEffects)
            {
                for (int i = 0; i < particleEffects.Length; i++)
                {
                    particleEffects[i].Stop();
                }
                if (toggleObject && ieDisableEffect == null)
                {
                    WaitForSeconds wait = new WaitForSeconds(fadeTime + 0.1f);
                    ieDisableEffect = StartCoroutine(IE_DisableEffect(wait));
                }
                hasAnything = true;
            }
            if (!hasAnything)
            {
                if (toggleObject)
                    targetObject.SetActive(false);
            }
            visible = false;
            hasStarted = true;
        }

        private IEnumerator IE_DisableEffect(WaitForSeconds wait)
        {
            yield return wait;
            bool hasAnything = false;
            if (hasParticleEffects)
            {
                for (int i = 0; i < particleEffects.Length; i++)
                {
                    particleEffects[i].gameObject.SetActive(false);
                }
                hasAnything = true;
            }
            if (hasShaderFade)
            {
                for (int i = 0; i < shaderFade.Length; i++)
                {
                    shaderFade[i].gameObject.SetActive(false);
                }
                hasAnything = true;
            }
            if (!hasAnything)
            {
                targetObject.SetActive(false);
            }
            ieDisableEffect = null;
        }
    }
}