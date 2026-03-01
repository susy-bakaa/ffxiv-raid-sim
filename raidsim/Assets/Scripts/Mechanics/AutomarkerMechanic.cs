// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.UI.PartyList;

namespace dev.susybaka.raidsim.Mechanics
{
    public class AutomarkerMechanic : FightMechanic
    {
        [Header("Info")]
        public PartyList targetParty;
        public bool autoObtainParty = false;
        public bool skipPlayer = false;
        public bool markByCharacterEventResult = true;
        [ShowIf(nameof(markByCharacterEventResult)), Min(-1)] public int characterEventResultId = -1;
        public bool sortByCharacterEventResult = false;
        [ShowIf(nameof(sortByCharacterEventResult))] public int sortCharacterEventResultId = -1;
        public List<AutomarkerPool> automarkerPools;
        [Header("Visuals")]
        public float initialDelay = 0f;
        public float markingDelay = 0.25f;
        public bool clearAutomatically = false;
        public float clearDelay = -1f;

        private CharacterState player;
        private Coroutine ieClearMarkersDelay;
        private Coroutine ieAssignMarkersDelay;
        private Coroutine ieAssignMarkers;
        private List<AutomarkerPool> pools;

        private void Start()
        {
            if (autoObtainParty && targetParty == null)
            {
                targetParty = FightTimeline.Instance.partyList;
            }

            player = FightTimeline.Instance.player;

            CopyPools();
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (!FightTimeline.Instance.useAutomarker)
            {
                return;
            }

            if (autoObtainParty && targetParty == null)
            {
                targetParty = FightTimeline.Instance.partyList;
            }

            if (player == null)
            {
                player = FightTimeline.Instance.player;
            }

            List<PartyMember> amTargets = new List<PartyMember>(targetParty.members);
            CopyPools(); // Reset pools to initial state

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
            if (sortByCharacterEventResult && sortCharacterEventResultId > -1)
            {
                amTargets = amTargets.OrderBy(member =>
                {
                    if (member.characterState != null && member.characterState.TryGetCharacterEventResult(sortCharacterEventResultId, out int sr))
                    {
                        return sr;
                    }
                    return int.MinValue + Random.Range(0, 10001); // Randomize order for characters without the result, but keep them after those with the result
                }).ToList();
            }

            for (int i = 0; i < amTargets.Count; i++)
            {
                if (markByCharacterEventResult && characterEventResultId > -1)
                {
                    Debug.Log($"[AutomarkerMechanic ({gameObject.name})] Processing character '{amTargets[i].name}' for automarker assignment based on character event result with ID {characterEventResultId}.");

                    if (amTargets[i].characterState == null)
                    {
                        if (log)
                            Debug.LogWarning($"[AutomarkerMechanic ({gameObject.name})] Character '{amTargets[i].name}' does not have a CharacterState component. Skipping automarker assignment.");
                        continue;
                    }

                    if (skipPlayer && amTargets[i].characterState == player)
                    {
                        if (log)
                            Debug.Log($"[AutomarkerMechanic ({gameObject.name})] Skipping player character '{amTargets[i].name}' for automarker assignment.");
                        continue;
                    }

                    if (amTargets[i].characterState.TryGetCharacterEventResult(characterEventResultId, out int result))
                    {
                        if (amTargets[i].characterState.showSignMarkers && amTargets[i].characterState.signMarkers != null && amTargets[i].characterState.signMarkers.Count > 0)
                        {
                            AutomarkerPoolEntry marker = GetAutomarkerFromPool(pools[result]);

                            if (marker.usedSignMarkerIndex < 0 || marker.maxUses < 0)
                            {
                                Debug.LogWarning($"[AutomarkerMechanic ({gameObject.name})] No available markers in pool '{pools[result].name}' for character '{amTargets[i].name}' with result {result}.");
                                continue;
                            }

                            if (amTargets[i].characterState.signMarkers.Count > marker.usedSignMarkerIndex)
                            {
                                amTargets[i].characterState.signMarkers[marker.usedSignMarkerIndex].AssignMarker(automarkerPools[result].entries[marker.index].maxUses - marker.maxUses - 1);
                            }
                        }
                    }
                    else if (log)
                    {
                        Debug.LogWarning($"[AutomarkerMechanic ({gameObject.name})] Character '{amTargets[i].name}' does not have a character event result with ID {characterEventResultId}.");
                    }

                    yield return wait;
                }
                else
                {
                    // Implement other marking logic here if not using character event results
                }
            }
        }

        private void ClearMarkers(List<PartyMember> amTargets)
        {
            for (int i = 0; i < amTargets.Count; i++)
            {
                if (amTargets[i].characterState.showSignMarkers && amTargets[i].characterState.signMarkers != null && amTargets[i].characterState.signMarkers.Count > 0)
                {
                    if (automarkerPools != null && automarkerPools.Count > 0)
                    {
                        foreach (var pool in automarkerPools)
                        {
                            if (pool.entries != null && pool.entries.Count > 0)
                            {
                                foreach (var entry in pool.entries)
                                {
                                    if (entry.usedSignMarkerIndex >= 0 && amTargets[i].characterState.signMarkers.Count > entry.usedSignMarkerIndex)
                                    {
                                        amTargets[i].characterState.signMarkers[entry.usedSignMarkerIndex].ResetMarker();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private AutomarkerPoolEntry GetAutomarkerFromPool(AutomarkerPool pool)
        {
            if (pool.entries == null || pool.entries.Count < 1)
                return new AutomarkerPoolEntry { name = string.Empty, usedSignMarkerIndex = -1, maxUses = -1 };

            for (int i = 0; i < pool.entries.Count; i++)
            {
                AutomarkerPoolEntry entry = pool.entries[i];

                if (entry.maxUses > 0)
                {
                    entry.maxUses--;
                    pool.entries[i] = entry; // Update the entry in the pool after decrementing maxUses
                    return entry;
                }
            }
            return new AutomarkerPoolEntry { name = string.Empty, usedSignMarkerIndex = -1, maxUses = -1 };
        }

        private void CopyPools()
        {
            pools = automarkerPools.Select(pool => new AutomarkerPool
            {
                name = pool.name,
                entries = new List<AutomarkerPoolEntry>(pool.entries) // Copy the list too
            }).ToList();

            foreach (var pool in pools)
            {
                for (int i = 0; i < pool.entries.Count; i++)
                {
                    AutomarkerPoolEntry entry = pool.entries[i];
                    entry.index = i;
                    pool.entries[i] = entry;
                }
            }
        }

        [System.Serializable]
        public struct AutomarkerPool
        {
            public string name;
            public List<AutomarkerPoolEntry> entries;
        }

        [System.Serializable]
        public struct AutomarkerPoolEntry
        {
            public string name;
            [Min(0)] public int index;
            [Min(0)] public int usedSignMarkerIndex;
            [Min(1)] public int maxUses;
        }
    }
}