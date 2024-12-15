using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using NaughtyAttributes;
#endif
using UnityEngine;

public class CharacterEffect : MonoBehaviour
{
    public bool visible = false;
    public bool enableOnStart = false;
    public bool toggleObject = true;
    public float fadeTime = 0.33f;

    private SimpleShaderFade shaderFade;
    private bool hasShaderFade = false;
    private Coroutine disableEffect;

#if UNITY_EDITOR
    [Button("Toggle Effect")]
    public void ToggleEffectButton()
    {
        ToggleEffect();
    }
#endif

    private void Awake()
    {
        shaderFade = transform.GetComponentInChildren<SimpleShaderFade>();
        if (shaderFade != null)
            hasShaderFade = true;
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

    public void ToggleEffect()
    {
        if (visible)
            DisableEffect();
        else
            EnableEffect();
    }

    public void EnableEffect()
    {
        if (visible)
            return;

        if (hasShaderFade)
        {
            if (toggleObject)
                shaderFade.gameObject.SetActive(true);
            shaderFade.FadeIn(fadeTime);
        }
        else
        {
            if (toggleObject)
                transform.GetChild(0).gameObject.SetActive(true);
        }
        visible = true;
    }

    public void DisableEffect()
    {
        if (!visible)
            return;

        if (hasShaderFade)
        {
            shaderFade.FadeOut(fadeTime);
            if (toggleObject && disableEffect == null)
            {
                WaitForSeconds wait = new WaitForSeconds(fadeTime + 0.1f);
                disableEffect = StartCoroutine(IE_DisableEffect(wait));
            }
        }
        else
        {
            if (toggleObject)
                transform.GetChild(0).gameObject.SetActive(false);
        }
        visible = false;
    }

    private IEnumerator IE_DisableEffect(WaitForSeconds wait)
    {
        yield return wait;
        if (hasShaderFade)
            shaderFade.gameObject.SetActive(false);
        disableEffect = null;
    }
}
