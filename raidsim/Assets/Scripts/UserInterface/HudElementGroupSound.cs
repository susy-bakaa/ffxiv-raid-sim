using UnityEngine;
using dev.susybaka.Shared.Attributes;
using dev.susybaka.Shared.Audio;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(HudElementGroup))]
    public class HudElementGroupSound : MonoBehaviour
    {
        AudioManager audioManager;
        HudElementGroup hudElementGroup;

        [SerializeField][SoundName] private string hoverSound = "ui_hover";
        [SerializeField][SoundName] private string confirmSound = "ui_confirm";
        [SerializeField][Range(0f, 1f)] private float audioVolume = 1f;

        public bool limitEvents = false;
        public float eventCooldown = 0.3f;

        private float timer = 0f;
        private bool eventsAvailable = true;
        private bool countdown = false;

        private void Start()
        {
            audioManager = AudioManager.Instance;
            hudElementGroup = GetComponent<HudElementGroup>();

            hudElementGroup.onPointerEnter.AddListener(OnPointerEnter);
            hudElementGroup.onPointerExit.AddListener(OnPointerExit);
            hudElementGroup.onPointerClick.AddListener(OnPointerClick);

            timer = eventCooldown;
        }

        private void Update()
        {
            if (audioManager == null)
                return;

            if (limitEvents && countdown)
            {
                timer -= Time.unscaledDeltaTime;
                if (timer <= 0f)
                {
                    countdown = false;
                    eventsAvailable = true;
                }
            }
        }

        private void OnPointerEnter(HudElementEventInfo eventInfo)
        {
            if (audioManager == null)
                return;

            if (limitEvents && eventInfo.element.restrictsAudio)
            {
                countdown = false;
                timer = eventCooldown;
            }

            if (!eventsAvailable)
                return;

            if (eventInfo.element.playHoverAudio)
            {
                audioManager.Play(hoverSound, audioVolume);
            }

            if (limitEvents && eventInfo.element.restrictsAudio)
                eventsAvailable = false;
        }

        private void OnPointerExit(HudElementEventInfo eventInfo)
        {
            if (audioManager == null)
                return;

            if (limitEvents && eventInfo.element.restrictsAudio)
            {
                countdown = true;
            }
        }

        private void OnPointerClick(HudElementEventInfo eventInfo)
        {
            if (audioManager == null)
                return;

            if (eventInfo.element.playClickAudio)
                audioManager.Play(confirmSound, audioVolume);
        }

        public void PlayConfirmSound()
        {
            if (audioManager == null)
                return;

            audioManager.Play(confirmSound, audioVolume);
        }
    }
}