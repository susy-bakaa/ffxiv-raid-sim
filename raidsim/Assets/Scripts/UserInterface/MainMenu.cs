// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections;
#if UNITY_WEBPLAYER
using System.Runtime.InteropServices;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NaughtyAttributes;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.UI
{
    public class MainMenu : MonoBehaviour
    {
        [Scene]
        public string simSceneName = "demo";
        public string simSceneBundle = string.Empty;
        public CanvasGroup fadeOut;
        public Button[] disableOnLoad;

#if UNITY_WEBPLAYER
        public const string webplayerQuitURL = "https://susybaka.dev/tools.html";
        [DllImport("__Internal")]
        private static extern void ReplaceURL(string url);
#endif
        Coroutine ie_exitApp;

        private void Start()
        {
            fadeOut.interactable = false;
            fadeOut.blocksRaycasts = true;
            fadeOut.alpha = 1f;
            fadeOut.LeanAlpha(0f, 0.5f).setOnComplete(() => { fadeOut.blocksRaycasts = false; fadeOut.gameObject.SetActive(false); });

            KeyBind.SetupKeyNames();

#if UNITY_STANDALONE_WIN && !UNITY_STANDALONE_LINUX && !UNITY_EDITOR_LINUX
            if (Application.isEditor)
                return;

            try
            {
                var windowPtr = GlobalVariables.FindWindow(null, GlobalVariables.lastWindowName);
                if (windowPtr == IntPtr.Zero)
                {
                    Debug.LogError("Window handle not found. Skipping SetWindowText.");
                }
                else
                {
                    string windowName = "raidsim";
                    GlobalVariables.SetWindowText(windowPtr, windowName);
                    GlobalVariables.lastWindowName = windowName;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting window title: {ex}");
            }
#endif
        }

        public void LoadSimScene()
        {
            if (disableOnLoad != null && disableOnLoad.Length > 0)
            {
                foreach (Button button in disableOnLoad)
                {
                    button.interactable = false;
                }
            }

            // Load next scene�s AssetBundle
            AssetHandler.Instance.LoadSceneAssetBundle(simSceneBundle);

            fadeOut.gameObject.SetActive(true);
            fadeOut.blocksRaycasts = true;
            fadeOut.LeanAlpha(1f, 0.5f).setOnComplete(() => SceneManager.LoadScene(simSceneName));
            StopAllCoroutines();
        }

        public void ReloadMainMenu(Button button)
        {
            StopAllCoroutines();
            button.interactable = false;
            Utilities.FunctionTimer.Create(this, () => SceneManager.LoadScene(SceneManager.GetActiveScene().name), 0.5f, "mainmenu_reload", true, true);
        }

        public void Quit()
        {
            if (disableOnLoad != null && disableOnLoad.Length > 0)
            {
                foreach (Button button in disableOnLoad)
                {
                    button.interactable = false;
                }
            }
            fadeOut.gameObject.SetActive(true);
            fadeOut.blocksRaycasts = true;
            fadeOut.LeanAlpha(1f, 0.5f);
            if (ie_exitApp == null)
                ie_exitApp = StartCoroutine(IE_ExitApp());
        }

        private IEnumerator IE_ExitApp()
        {
            yield return new WaitForSecondsRealtime(0.66f);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
        ReplaceURL(webplayerQuitURL);
#else
        Application.Quit();
#endif
        }
    }
}