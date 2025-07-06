using UnityEngine;
using dev.susybaka.Shared.Audio;
using dev.susybaka.Shared.Editor;
using System.Collections;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(HudWindow))]
    public class HudWindowAudio : MonoBehaviour
    {
        HudWindow hudWindow;
        AudioManager audioManager;

        [SerializeField][SoundName] private string openSound = "ui_open";
        [SerializeField][SoundName] private string closeSound = "ui_close";

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
                audioManager.Play(openSound);

            windowOpen = true;
        }

        private void OnClose()
        {
            if (!windowOpen)
                return;

            if (audioManager == null)
                return;

            if (!string.IsNullOrEmpty(closeSound))
                audioManager.Play(closeSound);

            windowOpen = false;
        }
    }
}