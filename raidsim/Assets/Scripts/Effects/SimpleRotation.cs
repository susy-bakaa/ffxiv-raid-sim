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
        public bool onUpdate = true;
        public bool setRotation = false;
        public bool addRotation = false;
        public bool oneTime = false;
        public string faceTowardsName;
        public Transform faceTowards;
        public bool faceTowardsNearestCharacter = false;
        [ShowIf(nameof(faceTowardsNearestCharacter))] public PartyList party;

        private bool done = false;

        private void Start()
        {
            if (!onUpdate)
                return;

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
            if (!onUpdate)
                return;

            if (oneTime && done)
                return;

            Rotate();

            if (oneTime)
            {
                done = true;
            }
        }

        public void Rotate()
        {
            if (faceTowards == null)
            {
                if (!setRotation && !addRotation)
                {
                    if (rotation != Vector3.zero)
                    {
                        transform.Rotate(rotation * Time.deltaTime);
                    }
                }
                else if (!setRotation && addRotation)
                {
                    transform.eulerAngles += rotation;
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
        }
    }
}