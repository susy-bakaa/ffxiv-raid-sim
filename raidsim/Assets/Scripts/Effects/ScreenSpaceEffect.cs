using dev.susybaka.raidsim.Core;
using UnityEngine;

namespace dev.susybaka.raidsim.Visuals
{
    public class ScreenSpaceEffect : MonoBehaviour
    {
        public bool log = false;

        [Header("Effects")]
        public GeometryTintEffect tintEffect;

        [Header("Options")]
        public float fadeTime = 0.5f;
        public float maxAlpha = 1f;
        public Color color = Color.clear;

        private LTDescr tween = null;
        private float originalAlpha = 0f;
        private Color originalColor;

        private void Awake()
        {
            if (tintEffect != null)
            {
                originalAlpha = tintEffect.alpha;
                originalColor = tintEffect.tintColor;
            }
        }

        private void OnEnable()
        {
            if (FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.AddListener(ResetEffect);
            }
        }

        private void OnDisable()
        {
            if (FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.RemoveListener(ResetEffect);
            }
        }

        public void FadeIn()
        {
            Fade(fadeTime, true);
        }

        public void FadeIn(float duration)
        {
            Fade(duration, true);
        }

        public void FadeOut()
        {
            Fade(fadeTime, false);
        }

        public void FadeOut(float duration)
        {
            Fade(duration, false);
        }

        private void Fade(float duration, bool state)
        {
            if (log)
                Debug.Log($"[ScreenSpaceEffect {gameObject.name}] Fading {(state ? "in" : "out")} over {duration} seconds.");

            if (tintEffect != null)
            {
                if (color != Color.clear)
                    tintEffect.tintColor = color;

                if (duration > 0f)
                {
                    float alpha = tintEffect.alpha;
                    if (tween != null)
                    {
                        if (log)
                            Debug.Log($"[ScreenSpaceEffect {gameObject.name}] Cancelling existing tween (id: {tween.id}) before starting new fade.\nalpha '{alpha}' originalAlpha '{originalAlpha}'");
                        LeanTween.cancel(tintEffect.gameObject, tween.id);
                    }
                    tween = LeanTween.value(tintEffect.gameObject, alpha, state ? maxAlpha : originalAlpha, duration).setOnUpdate(value => tintEffect.alpha = value).setOnComplete(() => { tween = null; if (log) { Debug.Log($"[ScreenSpaceEffect {gameObject.name}] Fade {(state ? "in" : "out")} completed over {duration} seconds."); } });
                }
                else
                {
                    tintEffect.alpha = state ? maxAlpha : originalAlpha;
                }
            }
        }

        public void SetColor(Color color)
        {
            if (tintEffect != null)
            {
                tintEffect.tintColor = color;
            }
        }

        public void ResetColor()
        {
            if (tintEffect != null)
            {
                tintEffect.tintColor = originalColor;
            }
        }

        public void ResetAlpha()
        {
            if (tintEffect != null)
            {
                tintEffect.alpha = originalAlpha;
            }
        }

        public void ResetEffect()
        {
            ResetColor();
            ResetAlpha();
        }
    }
}