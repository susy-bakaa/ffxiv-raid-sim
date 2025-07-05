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
        private Coroutine ieDisableEffect;
        private GameObject targetObject;
        private Coroutine ieOnReset;
        private bool reset = false;

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
            targetObject = transform.GetChild(0).gameObject;
            if (shaderFade != null && shaderFade.Length > 0)
                hasShaderFade = true;
            else
                hasShaderFade = false;

            wasVisible = visible;
            wasEnabledOnStart = enableOnStart;
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

            if (hasShaderFade)
            {
                for (int i = 0; i < shaderFade.Length; i++)
                {
                    if (toggleObject)
                        shaderFade[i].gameObject.SetActive(true);
                    shaderFade[i].FadeIn(fadeTime);
                }
            }
            else
            {
                if (toggleObject)
                    targetObject.SetActive(true);
            }
            visible = true;
        }

        public void DisableEffect()
        {
            if (!visible && !reset)
                return;

            if (log)
                Debug.Log($"[CharacterEffect ({gameObject.name})] DisableEffect was called!");

            if (hasShaderFade)
            {
                for (int i = 0; i < shaderFade.Length; i++)
                {
                    shaderFade[i].FadeOut(fadeTime);
                }
                if (toggleObject && ieDisableEffect == null)
                {
                    WaitForSeconds wait = new WaitForSeconds(fadeTime + 0.1f);
                    ieDisableEffect = StartCoroutine(IE_DisableEffect(wait));
                }
            }
            else
            {
                if (toggleObject)
                    targetObject.SetActive(false);
            }
            visible = false;
        }

        private IEnumerator IE_DisableEffect(WaitForSeconds wait)
        {
            yield return wait;
            if (hasShaderFade)
            {
                for (int i = 0; i < shaderFade.Length; i++)
                {
                    shaderFade[i].gameObject.SetActive(false);
                }
            }
            else
            {
                targetObject.SetActive(false);
            }
            ieDisableEffect = null;
        }
    }
}