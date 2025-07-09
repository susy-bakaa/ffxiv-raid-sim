using UnityEngine;

namespace dev.susybaka.Shared.Audio
{
    public class AudioManagerLayer : MonoBehaviour
    {
        private AudioManager manager;

        private void Awake()
        {
            if (FindObjectOfType<AudioManager>() != null)
                manager = FindObjectOfType<AudioManager>();
        }

        public void Play(string sound)
        {
            if (manager == null)
                return;

            manager.Play(sound);
        }

        public void StopPlaying(string sound)
        {
            if (manager == null)
                return;

            manager.StopPlaying(sound);
        }

        public void StopPlayingAll()
        {
            if (manager == null)
                return;

            manager.StopPlayingAll();
        }
    }
}