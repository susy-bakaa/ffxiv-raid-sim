// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;
using static dev.susybaka.raidsim.UI.PartyList;

namespace dev.susybaka.raidsim.Mechanics
{
    public class AutomarkerDebuffMechanic : FightMechanic
    {
        public PartyList targetParty;
        public bool autoObtainParty = false;
        public StatusEffectInfo targetEffect;
        public int usedSignMarkerIndex = 0;
        public float initialDelay = 0f;
        public float markingDelay = 0.25f;
        public bool clearAutomatically = false;
        public float clearDelay = -1f;

        private Coroutine ieClearMarkersDelay;
        private Coroutine ieAssignMarkersDelay;
        private Coroutine ieAssignMarkers;

        private void Start()
        {
            if (!FightTimeline.Instance.useAutomarker)
            {
                mechanicEnabled = false;
                return;
            }

            if (autoObtainParty && targetParty == null)
            {
                targetParty = FightTimeline.Instance.partyList;
            }
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (!FightTimeline.Instance.useAutomarker)
            {
                mechanicEnabled = false;
                return;
            }

            List<PartyMember> amTargets = new List<PartyMember>();

            if (targetEffect.data != null)
            {
                amTargets = targetParty.GetPrioritySortedList(targetEffect.data);
            }
            else
            {
                // Implement other cases of AM?
            }

            if (initialDelay <= 0f)
            {
                if (ieAssignMarkers == null)
                {
                    ieAssignMarkers = StartCoroutine(IE_AssignMarkers(amTargets, new WaitForSeconds(markingDelay)));
                }
            }
            else
            {
                if (ieAssignMarkersDelay == null)
                {
                    ieAssignMarkersDelay = StartCoroutine(IE_AssignMarkersDelay(amTargets, new WaitForSeconds(initialDelay)));
                }
            }

            if (clearAutomatically && clearDelay > 0f)
            {
                if (ieClearMarkersDelay == null)
                {
                    ieClearMarkersDelay = StartCoroutine(IE_ClearMarkersDelay(amTargets, new WaitForSeconds(clearDelay)));
                }
            }
            else
            {
                ClearMarkers(amTargets);
            }
        }

        public override void InterruptMechanic(ActionInfo actionInfo)
        {
            base.InterruptMechanic(actionInfo);

            if (ieClearMarkersDelay != null)
            {
                StopCoroutine(ieClearMarkersDelay);
                ieClearMarkersDelay = null;
            }
        }

        private IEnumerator IE_AssignMarkersDelay(List<PartyMember> amTargets, WaitForSeconds wait)
        {
            yield return wait;
            if (ieAssignMarkers != null)
            {
                StopCoroutine(ieAssignMarkers);
                ieAssignMarkers = null;
            }
            ieAssignMarkers = StartCoroutine(IE_AssignMarkers(amTargets, new WaitForSeconds(markingDelay)));
            ieAssignMarkersDelay = null;
        }

        private IEnumerator IE_ClearMarkersDelay(List<PartyMember> amTargets, WaitForSeconds wait)
        {
            yield return wait;
            ClearMarkers(amTargets);
            ieClearMarkersDelay = null;
        }

        private IEnumerator IE_AssignMarkers(List<PartyMember> amTargets, WaitForSeconds wait)
        {
            for (int i = 0; i < amTargets.Count; i++)
            {
                if (amTargets[i].characterState.showSignMarkers && amTargets[i].characterState.signMarkers != null && amTargets[i].characterState.signMarkers.Count > 0)
                {
                    if (amTargets[i].characterState.signMarkers.Count > usedSignMarkerIndex)
                    {
                        amTargets[i].characterState.signMarkers[usedSignMarkerIndex].AssignMarker(i);
                    }
                }
                yield return wait;
            }
        }

        private void ClearMarkers(List<PartyMember> amTargets)
        {
            for (int i = 0; i < amTargets.Count; i++)
            {
                if (amTargets[i].characterState.showSignMarkers && amTargets[i].characterState.signMarkers != null && amTargets[i].characterState.signMarkers.Count > 0)
                {
                    amTargets[i].characterState.signMarkers[usedSignMarkerIndex].ResetMarker();
                }
            }
        }
    }
}