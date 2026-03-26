// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Inputs;

namespace dev.susybaka.raidsim.UI
{
    public class TimelineConfigMenu : HudWindow
    {
        UserInput userInput;

        [SerializeField] private HudWindow menuBar;

        [Header("Timeline Config Options")]
        [SerializeField] private bool allowExport = false;
        [SerializeField] private CanvasGroup busyOverlay;
        [SerializeField] private List<GameObject> exportButtons = new List<GameObject>();
        [SerializeField] private List<TimelineConfigOption> options = new List<TimelineConfigOption>();
        [SerializeField] private string key;
        [SerializeField] private bool useTimelineAsKey = true;

        private ChatHandler chat;
        private WaitForSecondsRealtime waitForEachOption = new WaitForSecondsRealtime(0.6666f);

        protected override void Awake()
        {
            base.Awake();

            chat = ChatHandler.Instance;
            userInput = FindObjectOfType<UserInput>();

            if (allowExport)
            {
                for (int i = 0; i < exportButtons.Count; i++)
                {
                    exportButtons[i].SetActive(true);
                }

                if (busyOverlay == null)
                {
                    TaggedObject[] tagged = FindObjectsOfType<TaggedObject>();

                    foreach (TaggedObject taggedObject in tagged)
                    {
                        if (taggedObject.m_tag == "ImportingOverlay")
                        {
                            busyOverlay = taggedObject.GetComponent<CanvasGroup>();
                            break;
                        }
                    }
                }

                if (useTimelineAsKey)
                {
                    key = FightTimeline.Instance.timelineAbbreviation;
                }
                options.Clear();
                options.AddRange(transform.GetComponentsInChildren<TimelineConfigOption>(true));
            }
            else
            {
                for (int i = 0; i < exportButtons.Count; i++)
                {
                    exportButtons[i].SetActive(false);
                }
            }

            CloseWindow();
            menuBar.EnableInteractions();
        }

        private void Update()
        {
            if (userInput != null && isOpen)
            {
                userInput.targetRaycastInputEnabled = false;
                userInput.zoomInputEnabled = false;
            }
        }

        public void ToggleTimelineConfig()
        {
            if (isOpen)
            {
                CloseTimelineConfig();
            }
            else
            {
                OpenTimelineConfig();
            }
        }

        public void OpenTimelineConfig()
        {
            OpenWindow();
            if (menuBar != null)
            {
                menuBar.DisableInteractions();
            }
        }

        public void CloseTimelineConfig()
        {
            CloseWindow();
            if (menuBar != null)
            {
                menuBar.EnableInteractions();
            }
        }

        public void ExportTimelineConfig()
        {
            if (options.Count < 1 || !allowExport)
                return;

            List<string> configValues = new List<string>();

            configValues.Add(key); // Add the key as the first value in the export string

            foreach (var option in options)
            {
                configValues.Add(option.GetSelectedOption());
            }
            string exportString = string.Join(":", configValues);

            string code = TimelineConfigShareCode.Encode(exportString);

#if !UNITY_WEBGL
            GUIUtility.systemCopyBuffer = code;
#else
            WebGLCopyAndPaste.WebGLCopyAndPasteAPI.CopyToClipboard(code);
#endif

            if (chat != null)
            {
                chat.PostSystem("Timeline config share code copied to clipboard.");
            }
            Debug.Log($"Copied timeline config share code ({code.Length} chars) to clipboard.");
        }

        public void ImportTimelineConfig()
        {
            if (options.Count < 1 || !allowExport)
                return;

            string code1 = GUIUtility.systemCopyBuffer;
            string code2 = WebGLGlobalPaste.LastPastedText;

            if (code1.StartsWith("TC"))
            {
                ImportTimelineConfig(code1);

            }
            else
            {
                ImportTimelineConfig(code2);
            }
        }

        private bool ImportTimelineConfig(string code)
        {
            if (TimelineConfigShareCode.TryDecode(code, out string importString))
            {
                if (string.IsNullOrEmpty(importString))
                    return false;

                string[] configValues = importString.Split(':');

                if (configValues.Length < 2 || configValues[0] != key)
                {
                    if (chat != null)
                    {
                        chat.PostSystem("Imported timeline config is invalid or it is for a different timeline!", ChatKind.Error);
                    }
                    Debug.LogWarning("Imported timeline config is invalid or it is for a different timeline!");
                    return false;
                }

                StopAllCoroutines();
                StartCoroutine(IE_ImportTimelineConfig(configValues));
                return true;
            }
            else
            {
#if !UNITY_WEBGL
                if (chat != null)
                {
                    chat.PostSystem("Clipboard does not contain a valid timeline config share code!", ChatKind.Error);
                }
#else
                if (chat != null)
                {
                    chat.PostSystem("Clipboard does not contain a valid timeline config share code! Try pressing Ctrl+V to paste in the game window, in order to first copy your system clipboard over to the game.", ChatKind.Error);
                }
#endif
                Debug.LogWarning("Clipboard does not contain a valid timeline config share code.");
                return false;
            }
        }

        private IEnumerator IE_ImportTimelineConfig(string[] configValues)
        {
            if (busyOverlay != null)
            {
                busyOverlay.interactable = true;
                busyOverlay.blocksRaycasts = true;
                busyOverlay.LeanAlpha(1f, 0.25f);
            }
            for (int i = 1; i < configValues.Length; i++)
            {
                if (i - 1 < options.Count)
                {
                    options[i - 1].SetSelectedOption(configValues[i]);
                    yield return waitForEachOption;
                }
            }
            if (busyOverlay != null)
            {
                busyOverlay.interactable = false;
                busyOverlay.blocksRaycasts = false;
                busyOverlay.LeanAlpha(0f, 0.25f);
            }
            Debug.Log("Imported timeline config from clipboard (overwrite).");
        }
    }
}