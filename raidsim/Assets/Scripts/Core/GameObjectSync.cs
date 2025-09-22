// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;

namespace dev.susybaka.raidsim.Events 
{
    public class GameObjectSync : MonoBehaviour
    {
        public string targetPath;
        public bool searchFromRoot = false;
        public GameObject target;
        public bool beginActive = false;

        private bool started = false;

        private void OnEnable()
        {
            if (started && target != null)
            {
                target.SetActive(true);
                return;
            }

            Setup();
        }

        private void OnDisable()
        {
            if (target != null)
            {
                target.SetActive(false);
                return;
            }
        }

        public void Setup()
        {
            if (target == null)
            {
                if (!string.IsNullOrEmpty(targetPath) && !targetPath.Contains('%'))
                {
                    if (searchFromRoot)
                    {
                        target = transform.root.Find(targetPath)?.gameObject;
                    }
                    else
                    {
                        target = transform.Find(targetPath)?.gameObject;
                    }
                }
            }
            else if (target != null && !started)
            {
                started = true;
                if (beginActive)
                {
                    target.SetActive(true);
                }
                else
                {
                    target.SetActive(false);
                    gameObject.SetActive(false);
                }
            }
        }
    }
}