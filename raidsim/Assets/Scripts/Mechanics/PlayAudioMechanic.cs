// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.Shared.Attributes;
using dev.susybaka.Shared.Audio;
using static dev.susybaka.raidsim.Core.GlobalData;
using System.Collections;

namespace dev.susybaka.raidsim.Mechanics
{
    public class PlayAudioMechanic : FightMechanic
    {
        AudioManager audioManager;

        [Header("Play Audio Mechanic")]
        [SerializeField][SoundName] private string audioToPlay = "<None>";
        [SerializeField] private float overrideVolume = 1f;
        [SerializeField] private float delay = -1f;
        private float wasDelay = -1f;
        [SerializeField] private Transform location;
        [SerializeField] private bool useParent = false;
        [SerializeField][ShowIf("useParent")] private Transform parent;

        Coroutine iePlayAudioDelayed = null;

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
            iePlayAudioDelayed = null;
            wasDelay = delay;

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

            if (delay > 0f)
            {
                if (iePlayAudioDelayed == null)
                    iePlayAudioDelayed = StartCoroutine(IE_PlayAudioDelayed(new WaitForSeconds(delay)));
            }
            else
            {
                if (iePlayAudioDelayed != null)
                {
                    StopCoroutine(iePlayAudioDelayed);
                    iePlayAudioDelayed = null;
                    delay = wasDelay; // Reset delay if interrupted
                }
                PlayAudio();
            }
        }

        public void TriggerMechanic(float delay)
        {
            this.delay = delay;
            TriggerMechanic(new ActionInfo(null, null, null));
        }

        public override void InterruptMechanic(ActionInfo actionInfo)
        {
            base.InterruptMechanic(actionInfo);

            delay = wasDelay;
            if (iePlayAudioDelayed != null)
            {
                StopCoroutine(iePlayAudioDelayed);
                iePlayAudioDelayed = null;
            }
        }

        private IEnumerator IE_PlayAudioDelayed(WaitForSeconds wait)
        {
            yield return wait;
            PlayAudio();
            iePlayAudioDelayed = null;
            delay = wasDelay; // Reset delay after playing audio
        }

        private void PlayAudio()
        {
            if (!useParent)
                audioManager.PlayAt(audioToPlay, location, overrideVolume);
            else
                audioManager.PlayAt(audioToPlay, location.position, parent, overrideVolume);
        }
    }
}