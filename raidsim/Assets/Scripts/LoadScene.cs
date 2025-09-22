// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

namespace dev.susybaka.raidsim
{
    public class LoadScene : MonoBehaviour
    {
        [SerializeField, Scene] private string scene;

        private void Start()
        {
            //Utilities.FunctionTimer.CleanUp();
            SceneManager.LoadScene(scene);
        }
    }
}