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
    public bool log = false;

    private SimpleShaderFade[] shaderFade;
    private bool hasShaderFade = false;
    private Coroutine disableEffect;
    private GameObject targetObject;

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
        if (!visible)
            return;

        if (log)
            Debug.Log($"[CharacterEffect ({gameObject.name})] DisableEffect was called!");

        if (hasShaderFade)
        {
            for (int i = 0; i < shaderFade.Length; i++)
            {
                shaderFade[i].FadeOut(fadeTime);
            }
            if (toggleObject && disableEffect == null)
            {
                WaitForSeconds wait = new WaitForSeconds(fadeTime + 0.1f);
                disableEffect = StartCoroutine(IE_DisableEffect(wait));
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
        disableEffect = null;
    }
}
