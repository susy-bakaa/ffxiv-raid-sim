using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeCanvasGroup : MonoBehaviour
{
    private CanvasGroup group;
    [SerializeField] private float fadeDuration = 0.2f;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
    }

    public void FadeToggle()
    {
        if (group.alpha < 1f)
        {
            FadeIn();
        }
        else
        {
            FadeOut();
        }
    }

    public void FadeIn()
    {
        group.LeanAlpha(1f, fadeDuration);
    }

    public void FadeOut()
    {
        group.LeanAlpha(0f, fadeDuration);
    }
}
