// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;
using dev.susybaka.Shared.Attributes;
using dev.susybaka.Shared.Audio;
using static dev.susybaka.raidsim.Core.GlobalVariables;

namespace dev.susybaka.raidsim.UI
{
    public class FightSelector : MonoBehaviour
    {
        TMP_Dropdown dropdown;
        Button loadButton;
        [SerializeField] private string loadButtonName = "LoadFight";
        public float loadDelay = 2f;
        public List<TimelineScene> scenes = new List<TimelineScene>();
        public int currentSceneIndex = 0;
        private TimelineScene currentScene;
        public TimelineScene CurrentScene => currentScene;
        private TimelineScene originalScene;

        private Coroutine ieLoadSceneDelayed;

#if UNITY_EDITOR
        private int previousSceneIndex = -1;

        private void OnValidate()
        {
            if (scenes != null && scenes.Count > 0 && previousSceneIndex != currentSceneIndex)
            {
                if (transform.TryGetComponentInChildren(out TMP_Dropdown _dropdown))
                {
                    _dropdown.value = currentSceneIndex;
                    _dropdown.RefreshShownValue();
                    previousSceneIndex = currentSceneIndex;
                }
            }
        }
#endif

        private void Awake()
        {
            foreach (Transform child in transform.parent)
            {
                if (child.gameObject.name == loadButtonName)
                {
                    loadButton = child.GetComponentInChildren<Button>();
                    break;
                }
            }

            currentScene = scenes[currentSceneIndex];
            originalScene = currentScene;
        }

        private void Start()
        {
            dropdown = GetComponentInChildren<TMP_Dropdown>();
            //Select(0);

            // Set the window title to include the fight name, if possible
            if (WindowTitle.TrySet($"raidsim - {GetFightName()}"))
            {
                WindowTitle.ResetWindowCache();
                WindowTitle.TrySet($"raidsim - {GetFightName()}");
            }
        }

        private void Update()
        {
            if (FightTimeline.Instance == null)
                return;

            dropdown.interactable = !FightTimeline.Instance.playing;
            loadButton.interactable = !FightTimeline.Instance.playing;
        }

        public void Select(int value)
        {
            if (scenes.Count < (value + 1))
            {
                Debug.Log("Fight not implemented yet.");
                return;
            }
            currentScene = scenes[value];
        }

        public void Reload()
        {
            if (FightTimeline.Instance != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.Play(FightTimeline.Instance.ReloadSound, FightTimeline.Instance.AudioVolume);
            }

            if (ChatHandler.Instance != null)
            {
                ChatHandler.Instance.Clear();
            }

            if (ieLoadSceneDelayed == null)
            {
                ieLoadSceneDelayed = StartCoroutine(IE_LoadSceneDelayed(originalScene, new WaitForSeconds(loadDelay)));
            }
        }

        public void Load()
        {
            if (ChatHandler.Instance != null)
            {
                ChatHandler.Instance.Clear();
            }

            if (ieLoadSceneDelayed == null)
            {
                ieLoadSceneDelayed = StartCoroutine(IE_LoadSceneDelayed(currentScene, new WaitForSeconds(loadDelay)));
            }
        }

        private void OnLoad(TimelineScene scene)
        {
            if (AssetHandler.Instance != null)
            {
                // Clear the cache before loading a new scene, since otherwise scenes sharing a bundle will cause issues
                if (SceneManager.GetActiveScene().path != scene.scene && SceneManager.GetActiveScene().name != scene.scene)
                {
                    AssetHandler.Instance.ClearCache();
                }

                // Load next scene�s AssetBundle
                AssetHandler.Instance.LoadSceneAssetBundle(scene.assetBundles);
            }

            SceneManager.LoadScene(scene.scene);
        }

        private string GetFightName()
        {
            return FightTimeline.Instance.timelineName;
        }

        private IEnumerator IE_LoadSceneDelayed(TimelineScene scene, WaitForSeconds wait)
        {
            yield return wait;
            ieLoadSceneDelayed = null;
            OnLoad(scene);
        }

        [System.Serializable]
        public struct TimelineScene
        {
            [Scene]
            public string scene;
            [AssetBundleNames]
            public string[] assetBundles;

            public TimelineScene(string scene, string[] assetBundles)
            {
                this.scene = scene;
                this.assetBundles = assetBundles;
            }

            public TimelineScene(string scene)
            {
                this.scene = scene;
                this.assetBundles = new string[0];
            }
        }
    }
}