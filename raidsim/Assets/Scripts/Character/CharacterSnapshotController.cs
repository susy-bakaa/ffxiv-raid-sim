// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using dev.susybaka.raidsim.Core;
using UnityEngine;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Characters
{
    public class CharacterSnapshotController : MonoBehaviour
    {
        public CharacterState targetCharacterState;
        [Min(1)] public int maxSnapshots = 16;

        private bool initialized = false;
        private CharacterSnapshot[] snapshots;
        private int head;
        private int count;

        private void Awake()
        {
            snapshots = new CharacterSnapshot[maxSnapshots];
        }

        private void OnEnable()
        {
            if (FightTimeline.Instance != null)
                FightTimeline.Instance.onServerTick.AddListener(HandleSnapshotTick);
        }

        private void OnDisable()
        {
            if (FightTimeline.Instance != null)
                FightTimeline.Instance.onServerTick.RemoveListener(HandleSnapshotTick);
        }

        public void Initialize(CharacterState initialState)
        {
            targetCharacterState = initialState;
            gameObject.name = $"{targetCharacterState.gameObject.name}_SnapshotController";
            initialized = true;
        }

        private void HandleSnapshotTick(float snapshotTime)
        {
            if (!initialized)
                return;

            RecordSnapshot(snapshotTime);

            float queryTime = snapshotTime - FightTimeline.Instance.effectiveLookback;
            CharacterSnapshot snapshot = GetSnapshotAtOrBefore(queryTime);

            transform.SetPositionAndRotation(snapshot.position, snapshot.rotation);
        }

        public void RecordSnapshot(float time)
        {
            if (!initialized)
                return;

            snapshots[head] = new CharacterSnapshot(time, targetCharacterState);

            head = (head + 1) % snapshots.Length;
            count = Mathf.Min(count + 1, snapshots.Length);
        }

        public CharacterSnapshot GetSnapshotAtOrBefore(float time)
        {
            if (count == 0 || !initialized)
            {
                return new CharacterSnapshot(time, targetCharacterState);
            }

            for (int i = 0; i < count; i++)
            {
                int index = (head - 1 - i + snapshots.Length) % snapshots.Length;
                CharacterSnapshot snapshot = snapshots[index];

                if (snapshot.time <= time)
                    return snapshot;
            }

            int oldestIndex = (head - count + snapshots.Length) % snapshots.Length;
            return snapshots[oldestIndex];
        }
    }
}