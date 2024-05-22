using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOnStart : MonoBehaviour
{
    CanvasGroup group;

    public float delay = 1f;
    public float duration = 1f;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 1f;
    }

    void Start()
    {
        if (delay > 0f)
        {
            Utilities.FunctionTimer.Create(() => group.LeanAlpha(0f, duration), delay);
        }
        else
        {
            group.LeanAlpha(0f, duration);
        }
    }

    public void FadeToBlack(float delay)
    {
        if (delay > 0f)
        {
            Utilities.FunctionTimer.Create(() => group.LeanAlpha(1f, duration), delay);
        }
        else
        {
            group.LeanAlpha(1f, duration);
        }
    }
}