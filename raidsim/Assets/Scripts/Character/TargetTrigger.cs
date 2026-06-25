// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.Shared;
using UnityEngine;

namespace dev.susybaka.raidsim.Targeting
{
    public class TargetTrigger : MonoBehaviour
    {
        TargetNode node;

        Dictionary<GameObject, TargetController> players;

        private void Awake()
        {
            node = transform.parent.GetComponent<TargetNode>();
            players = new Dictionary<GameObject, TargetController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (players.TryGetValue(other.gameObject, out TargetController cachedController))
                {
                    if (node != null)
                    {
                        node.AddNodeToController(cachedController.self);
                    }
                }
                else
                {
                    TargetController controller = null;
                    CharacterSnapshotController snapshots = null;

                    if (other.transform.TryGetComponentInParents(true, out controller) || other.transform.TryGetComponent(out snapshots))
                    {
                        if (controller == null && snapshots != null)
                            controller = snapshots.targetCharacterState.targetController;

                        if (controller == null)
                            return;

                        if (node != null)
                        {
                            players.TryAdd(other.gameObject, controller);
                            node.AddNodeToController(controller.self);
                        }
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (players.TryGetValue(other.gameObject, out TargetController cachedController))
                {
                    if (node != null)
                    {
                        node.RemoveNodeFromController(cachedController.self);
                    }
                }
                else
                {
                    TargetController controller = null;
                    CharacterSnapshotController snapshots = null;

                    if (other.transform.TryGetComponentInParents(true, out controller) || other.transform.TryGetComponent(out snapshots))
                    {
                        if (controller == null && snapshots != null)
                            controller = snapshots.targetCharacterState.targetController;

                        if (controller == null)
                            return;

                        if (node != null)
                        {
                            players.TryAdd(other.gameObject, controller);
                            node.RemoveNodeFromController(controller.self);
                        }
                    }
                }
            }
        }
    }
}