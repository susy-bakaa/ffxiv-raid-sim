using System.Collections;
using UnityEngine;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.UI
{
    public class FadeOnStart : MonoBehaviour
    {
        CanvasGroup group;

        public float delay = 1f;
        private float previousDelay = -1;
        private float wasDelay;
        public float duration = 1f;
        public bool repeatOnReset = false;
        private bool wasRepeatOnReset;

        private int id = 0;
        private Coroutine ieFadeDelay;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.alpha = 1f;
            id = Random.Range(1000, 10000);

            wasRepeatOnReset = repeatOnReset;
            wasDelay = delay;
        }

        private void Start()
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
                if (duration > 0f)
                    group.LeanAlpha(0f, duration);
                else
                    group.alpha = 0f;
            }

            if (previousDelay < 0)
                previousDelay = delay;
            else
                delay = previousDelay;
        }

        private void OnEnable()
        {
            if (repeatOnReset && FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.AddListener(OnReset);
            }
        }

        private void OnDisable()
        {
            if (repeatOnReset && FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.RemoveListener(OnReset);
            }
        }

        private void OnDestroy()
        {
            if (repeatOnReset && FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.RemoveListener(OnReset);
            }
        }

        private void OnReset()
        {
            repeatOnReset = wasRepeatOnReset;
            delay = wasDelay;
            if (repeatOnReset)
                Start();
        }

        public void FadeToTransition(float delay)
        {
            if (duration > 0f)
            {
                group.LeanAlpha(1f, duration).setOnComplete(() =>
                {
                    if (delay >= 0f)
                        this.delay = delay;
                    Start();
                });
            }
            else
            {
                group.alpha = 1f;
                if (delay >= 0f)
                    this.delay = delay;
                Start();
            }
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
                if (duration > 0f)
                    group.LeanAlpha(1f, duration);
                else
                    group.alpha = 1f;
            }
        }

        public void FadeToInvisible(float delay)
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
                if (duration > 0f)
                    group.LeanAlpha(0f, duration);
                else
                    group.alpha = 0f;
            }
        }

        private IEnumerator IE_FadeDelay(WaitForSecondsRealtime wait, bool fadeOut)
        {
            yield return wait;
            if (fadeOut)
            {
                if (duration > 0f)
                    group.LeanAlpha(1f, duration);
                else
                    group.alpha = 1f;
            }
            else
            {
                if (duration > 0f)
                    group.LeanAlpha(0f, duration);
                else
                    group.alpha = 0f;
            }
            ieFadeDelay = null;
        }
    }
}