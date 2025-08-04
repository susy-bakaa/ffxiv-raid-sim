using UnityEngine;
using dev.susybaka.Shared.Attributes;
using dev.susybaka.Shared.Audio;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(HudWindow))]
    public class HudWindowAudio : MonoBehaviour
    {
        HudWindow hudWindow;
        AudioManager audioManager;

        [SerializeField][SoundName] private string openSound = "ui_open";
        [SerializeField][SoundName] private string closeSound = "ui_close";
        [SerializeField][Range(0f,1f)] private float audioVolume = 1f;

        private bool windowOpen = false;

        private void Awake()
        {
            hudWindow = GetComponent<HudWindow>();
            audioManager = AudioManager.Instance;

            hudWindow.onOpen.AddListener(OnOpen);
            hudWindow.onClose.AddListener(OnClose);
        }

        private void OnOpen()
        {
            if (windowOpen)
                return;

            if (audioManager == null)
                return;

            if (!string.IsNullOrEmpty(openSound))
                audioManager.Play(openSound, audioVolume);

            windowOpen = true;
        }

        private void OnClose()
        {
            if (!windowOpen)
                return;

            if (audioManager == null)
                return;

            if (!string.IsNullOrEmpty(closeSound))
                audioManager.Play(closeSound, audioVolume);

            windowOpen = false;
        }
    }
}