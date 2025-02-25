using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOnStart : MonoBehaviour
{
    CanvasGroup group;

    public float delay = 1f;
    private float wasDelay = -1;
    public float duration = 1f;

    private int id = 0;
    private Coroutine ieFadeDelay;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 1f;
        id = Random.Range(1000, 10000);
    }

    void Start()
    {
        if (delay > 0f)
        {
            if (ieFadeDelay == null && gameObject.scene.isLoaded && gameObject.activeSelf)
            {
                ieFadeDelay = StartCoroutine(IE_FadeDelay(new WaitForSecondsRealtime(delay), false));
            }
        }
        else
        {
            group.LeanAlpha(0f, duration);
        }

        if (wasDelay < 0)
            wasDelay = delay;
        else
            delay = wasDelay;
    }

    public void FadeToTransition(float delay)
    {
        group.LeanAlpha(1f, duration).setOnComplete(() =>
        {
            if (delay >= 0f)
                this.delay = delay;
            Start();
        });
    }

    public void FadeToBlack(float delay)
    {
        if (delay > 0f)
        {
            if (ieFadeDelay == null && gameObject.scene.isLoaded && gameObject.activeSelf)
            {
                ieFadeDelay = StartCoroutine(IE_FadeDelay(new WaitForSecondsRealtime(delay), true));
            }
        }
        else
        {
            group.LeanAlpha(1f, duration);
        }
    }

    private IEnumerator IE_FadeDelay(WaitForSecondsRealtime wait, bool fadeOut)
    {
        yield return wait;
        if (fadeOut)
        {
            group.LeanAlpha(1f, duration);
        }
        else
        {
            group.LeanAlpha(0f, duration);
        }
        ieFadeDelay = null;
    }
}