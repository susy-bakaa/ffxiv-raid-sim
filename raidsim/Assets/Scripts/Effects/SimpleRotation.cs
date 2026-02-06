// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.UI;
using dev.susybaka.Shared;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.Visuals
{
    public class SimpleRotation : MonoBehaviour
    {
        public Vector3 rotation;
        public bool setRotation = false;
        public bool oneTime = false;
        public string faceTowardsName;
        public Transform faceTowards;
        public bool faceTowardsNearestCharacter = false;
        [ShowIf(nameof(faceTowardsNearestCharacter))] public PartyList party;

        private bool done = false;

        private void Awake()
        {
            done = false;

            if (faceTowardsNearestCharacter && party == null)
            {
                party = FightTimeline.Instance?.partyList;
            }

            if (!string.IsNullOrEmpty(faceTowardsName))
            {
                faceTowards = Utilities.FindAnyByName(faceTowardsName)?.transform;
            }
            else if (faceTowardsNearestCharacter && party != null)
            {
                faceTowards = party.GetNearestMember(transform.position).transform;
            }
        }

        private void Update()
        {
            if (oneTime && done)
                return;

            if (faceTowards == null)
            {
                if (!setRotation)
                {
                    if (rotation != Vector3.zero)
                    {
                        transform.Rotate(rotation * Time.deltaTime);
                    }
                }
                else
                {
                    transform.eulerAngles = rotation;
                }
            }
            else
            {
                transform.LookAt(faceTowards);
                transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
            }

            if (oneTime)
            {
                done = true;
            }
        }
    }
}