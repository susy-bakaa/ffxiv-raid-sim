// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using NaughtyAttributes;
#endif
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class RepeatMechanic : FightMechanic
    {
        public int repeatCount = 3;
        public float waitDuration = 1.0f;
        public FightMechanic mechanicToTrigger;
        public bool skipLastWait = false;
        public UnityEvent<ActionInfo> onFinish;

        private Coroutine ieRepeat;

#if UNITY_EDITOR
        [Button("Start Mechanic")]
        public void StartMechanic()
        {
            TriggerMechanic(new ActionInfo());
        }
#endif

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (CanTrigger(actionInfo) && ieRepeat == null)
            {
                ieRepeat = StartCoroutine(IE_Repeat(actionInfo));
            }
        }

        public override void InterruptMechanic(ActionInfo actionInfo)
        {
            StopAllCoroutines();
            ieRepeat = null;
        }

        private IEnumerator IE_Repeat(ActionInfo actionInfo)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                if (mechanicToTrigger != null)
                {
                    mechanicToTrigger.TriggerMechanic();
                }
                if ((i < repeatCount - 1) || !skipLastWait)
                {
                    yield return new WaitForSeconds(waitDuration);
                }
            }
            onFinish.Invoke(actionInfo);
            ieRepeat = null;
        }
    }
}