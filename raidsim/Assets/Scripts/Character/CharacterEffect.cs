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
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.Core.GlobalData.Flag;

namespace dev.susybaka.raidsim.Characters
{
    public class CharacterEffect : MonoBehaviour
    {
        [SerializeField] private bool log = false;

        [SerializeField] private Flag isVisible = new Flag("isVisible", AggregateLogic.AnyTrue);
        private Flag wasIsVisible;
        private bool visible => isVisible.value;
        public bool enableOnStart = false;
        private bool wasEnabledOnStart;
        public bool toggleObject = true;
        [Min(0f)] public float fadeTime = 0.33f;
        [Min(0f)] public float fadeInDelay = 0f;
        [Min(0f)] public float fadeOutDelay = 0f;

        private SimpleShaderFade[] shaderFade;
        private bool hasShaderFade = false;
        private SimpleParticleEffect[] particleEffects;
        private bool hasParticleEffects = false;
        private Coroutine ieDisableEffect;
        private GameObject targetObject;
        private Coroutine ieOnReset;
        private bool reset = false;
        private bool hasStarted = false;
        private Coroutine ieFadeOutDelay;
        private Coroutine ieFadeInDelay;
        private WaitForSeconds waitFadeInDelay = null;
        private WaitForSeconds waitFadeOutDelay = null;

#if UNITY_EDITOR
        [Button("Toggle Effect")]
        public void ToggleEffectButton()
        {
            ToggleEffect("CharacterEffect_Default");
        }
        [Button("Update Delays")]
        public void UpdateDelaysButton()
        {
            waitFadeInDelay = new WaitForSeconds(fadeInDelay);
            waitFadeOutDelay = new WaitForSeconds(fadeOutDelay);
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

            waitFadeInDelay = new WaitForSeconds(fadeInDelay);
            waitFadeOutDelay = new WaitForSeconds(fadeOutDelay);
            //wasVisible = visible;
            wasIsVisible = new Flag(isVisible);
            wasEnabledOnStart = enableOnStart;
            hasStarted = false;
        }

        private void Start()
        {
            if (enableOnStart)
            {
                EnableEffect("CharacterEffect_Default");
            }
            else
            {
                DisableEffect("CharacterEffect_Default");
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
            //visible = wasVisible;
            isVisible = new Flag(wasIsVisible);
            enableOnStart = wasEnabledOnStart;
            reset = true;
            hasStarted = false;

            if (ieFadeInDelay != null || ieFadeOutDelay != null)
                DisableEffectInternal(false);

            StopAllCoroutines();

            ieDisableEffect = null;
            ieOnReset = null;
            ieFadeInDelay = null;
            ieFadeOutDelay = null;

            if (log)
                Debug.Log($"[CharacterEffect ({gameObject.name})] OnReset was called!");

            if (gameObject.activeSelf && gameObject.activeInHierarchy)
            {
                if (enableOnStart)
                {
                    Start();
                }
                else
                {
                    if (isVisible.value)
                        EnableEffect("CharacterEffect_Default");
                    else
                    {
                        if (log)
                            Debug.Log($"[CharacterEffect ({gameObject.name})] Effect disabled with reset!");

                        DisableEffect("CharacterEffect_Default");
                    }
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
            ToggleEffect("CharacterEffect_Default");
        }

        public void ToggleEffect(string key)
        {
            if (visible)
                DisableEffect(key);
            else
                EnableEffect(key);
        }

        public void EnableEffect()
        {
            EnableEffect("CharacterEffect_Default");
        }

        public void EnableEffect(string key)
        {
            bool wasStarted = hasStarted;
            hasStarted = true;

            if (isVisible.GetFlagValue(key))
                return;

            bool tempVisible = visible;

            isVisible.SetFlag(key, true);

            if (tempVisible && !reset)
                return;

            if (log)
                Debug.Log($"[CharacterEffect ({gameObject.name})] EnableEffect was called!");

            if (fadeInDelay > 0f)
            {
                if (ieFadeInDelay == null)
                {
                    if (ieFadeOutDelay != null)
                    {
                        StopCoroutine(ieFadeOutDelay);
                        ieFadeOutDelay = null;
                    }
                    ieFadeInDelay = StartCoroutine(IE_FadeInDelay(wasStarted));
                }
            }
            else
            {
                EnableEffectInternal(wasStarted);
            }
        }

        private IEnumerator IE_FadeInDelay(bool wasStarted)
        {
            yield return waitFadeInDelay;
            EnableEffectInternal(wasStarted);
            ieFadeInDelay = null;
        }

        private void EnableEffectInternal(bool wasStarted)
        {
            bool hasAnything = false;

            if (!visible)
                return;

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
                    if (wasStarted)
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
        }

        public void DisableEffect()
        {
            DisableEffect("CharacterEffect_Default");
        }

        public void DisableEffect(string key)
        {
            bool wasStarted = hasStarted;
            hasStarted = true;

            if (!visible && !reset)
                return;

            if (log)
                Debug.Log($"[CharacterEffect ({gameObject.name})] DisableEffect was called!");

            isVisible.RemoveFlag(key);

            if (visible && !reset)
                return;

            if (fadeOutDelay > 0f)
            {
                if (ieFadeOutDelay == null)
                {
                    if (ieFadeInDelay != null)
                    {
                        StopCoroutine(ieFadeInDelay);
                        ieFadeInDelay = null;
                    }
                    ieFadeOutDelay = StartCoroutine(IE_FadeOutDelay(wasStarted));
                }
            }
            else
            {
                DisableEffectInternal(wasStarted);
            }
        }

        private IEnumerator IE_FadeOutDelay(bool wasStarted)
        {
            yield return waitFadeOutDelay;
            DisableEffectInternal(wasStarted);
            ieFadeOutDelay = null;
        }

        private void DisableEffectInternal(bool wasStarted)
        {
            bool hasAnything = false;

            if (visible)
                return;

            if (hasShaderFade)
            {
                for (int i = 0; i < shaderFade.Length; i++)
                {
                    if (wasStarted)
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