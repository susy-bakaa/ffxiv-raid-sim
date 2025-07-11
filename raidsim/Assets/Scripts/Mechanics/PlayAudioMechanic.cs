using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.Shared.Attributes;
using dev.susybaka.Shared.Audio;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class PlayAudioMechanic : FightMechanic
    {
        AudioManager audioManager;

        [Header("Play Audio Mechanic")]
        [SerializeField][SoundName] private string audioToPlay = "<None>";
        [SerializeField] private float overrideVolume = 1f;
        [SerializeField] private Transform location;
        [SerializeField] private bool useParent = false;
        [SerializeField][ShowIf("useParent")] private Transform parent;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(audioToPlay))
            {
                audioToPlay = "<None>";
            }
        }
#endif

        private void Awake()
        {
            audioManager = AudioManager.Instance;

            if (location == null && audioManager != null)
                location = audioManager.transform;

            if (useParent && parent == null)
                parent = location;
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (audioManager == null)
                return;

            if (!useParent)
                audioManager.PlayAt(audioToPlay, location, overrideVolume);
            else
                audioManager.PlayAt(audioToPlay, location.position, parent, overrideVolume);
        }
    }
}