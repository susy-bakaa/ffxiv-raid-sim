// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
#pragma warning disable 0162
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using dev.illa4257;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class ResolutionDropdown : MonoBehaviour
    {
#pragma warning disable 0414
        private TMP_Dropdown resolutionDropdown;
        [SerializeField] private GameObject arrowStandard;
        [SerializeField] private GameObject arrowWhenExpanded;

        private Resolution[] resolutions;

        private List<Resolution> filteredResolutions;

        private float currentRefreshRate;
        private int currentResolutionIndex = 0;
        private const float HzTolerance = 0.5f;
#if UNITY_STANDALONE_WIN
        private const bool unfilteredResolutions = false;
#else
        private const bool unfilteredResolutions = true;
#endif

        private Vector2 currentResolution;
        int savedValueX = 0;
        int savedValueY = 0;

        public string group = "";
        public string key = "UnnamedResolution";

        private string keyX { get { return $"{key}Width"; } }
        private string keyY { get { return $"{key}Height"; } }

        public UnityEvent<Vector2> onStart;

        IniStorage ini;
#pragma warning restore 0414
        private void Awake()
        {
#if UNITY_WEBPLAYER
            return;
#else
            savedValueX = 0;
            savedValueY = 0;
            ini = new IniStorage(GlobalVariables.configPath);

            if (resolutionDropdown == null)
                resolutionDropdown = GetComponent<TMP_Dropdown>();

            resolutions = Screen.resolutions;

            filteredResolutions = new List<Resolution>();

            resolutionDropdown.ClearOptions();
            currentRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value;

            for (int i = 0; i < resolutions.Length; i++)
            {
                if (unfilteredResolutions)
                {
                    // On some systems, especially Linux, the resolutions won't get detected properly, so just allow all resolutions
                    filteredResolutions.Add(resolutions[i]);
                }
                else
                {
                    // Approximate match of refresh rate within tolerance, because some systems report slightly different refresh rates
                    // e.g. 59.94Hz vs 60Hz
                    if (Mathf.Abs((float)resolutions[i].refreshRateRatio.value - currentRefreshRate) <= HzTolerance)
                    {
                        filteredResolutions.Add(resolutions[i]);
                    }
                }
            }

            // Fallback if resolutions still won't get detected, just use and allow all resolutions
            if (filteredResolutions.Count == 0)
                filteredResolutions.AddRange(resolutions);

            filteredResolutions.Sort((a, b) => {
                if (a.width != b.width)
                    return b.width.CompareTo(a.width);
                else
                    return b.height.CompareTo(a.height);
            });

            List<string> options = new List<string>();

            for (int i = 0; i < filteredResolutions.Count; i++)
            {
                string resolutionOption = $"{filteredResolutions[i].width} x {filteredResolutions[i].height}";

                // Only show refresh rate if unfiltered resolutions are allowed, otherwise all resolutions have the same refresh rate
                if (unfilteredResolutions && filteredResolutions[i].refreshRateRatio.value > 0)
                    resolutionOption += $" {filteredResolutions[i].refreshRateRatio.value}Hz";

                options.Add(resolutionOption);

                // More lenient match allowing any refresh rate, because some systems report slightly different refresh rates
                bool matchFree = unfilteredResolutions && filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height;

                // Approximate match of refresh rate within tolerance, because some systems report slightly different refresh rates
                // e.g. 59.94Hz vs 60Hz
                // Strict match only allowing only resolutions with the monitor's current refresh rate
                bool matchStrict = !unfilteredResolutions && filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height && Mathf.Abs((float)filteredResolutions[i].refreshRateRatio.value - currentRefreshRate) <= HzTolerance;

                if (matchFree || matchStrict)
                {
                    currentResolutionIndex = i;
                    currentResolution = new Vector2(Screen.width, Screen.height);
                }
            }

            resolutionDropdown.AddOptions(options);

            if (currentResolutionIndex < 0 || currentResolutionIndex >= filteredResolutions.Count)
            {
                currentResolutionIndex = 0;
                currentResolution = new Vector2(filteredResolutions[0].width, filteredResolutions[0].height);
            }

            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
            SetResolution(currentResolutionIndex, Screen.fullScreen);
#endif
        }

        private void Start()
        {
#if UNITY_WEBPLAYER
            return;
#else
            Utilities.FunctionTimer.Create(this, () => OnStart(), Random.Range(1f, 1.25f), $"{group}_{key}_resolution_onstart_delay", true, true);
#endif
        }

        private void Update()
        {
#if UNITY_WEBPLAYER
            return;
#else
            if (arrowStandard == null || arrowWhenExpanded == null)
                return;

            if (resolutionDropdown.IsExpanded)
            {
                arrowStandard.SetActive(false);
                arrowWhenExpanded.SetActive(true);
            }
            else
            {
                arrowWhenExpanded.SetActive(false);
                arrowStandard.SetActive(true);
            }
#endif
        }

        public void SetResolution(int resolutionIndex, bool fullscreen)
        {
#if UNITY_WEBPLAYER
            return;
#else
            currentResolutionIndex = resolutionIndex;

            if (filteredResolutions == null || filteredResolutions.Count <= 0)
                return;

            Resolution resolution = filteredResolutions[resolutionIndex];

            //Debug.Log($"[SetResolution] resolution: 'x:{resolution.width}, y:{resolution.height}' resolutionIndex: '{resolutionIndex}'");

            if (resolution.width != GlobalVariables.currentGameResolution.width || resolution.height != GlobalVariables.currentGameResolution.height)
            {
                //Debug.Log($"Resolution has changed! Setting new resolution...");

                Screen.SetResolution(resolution.width, resolution.height, fullscreen);
                GlobalVariables.currentGameResolution = resolution;
            }

            currentResolution = new Vector2(resolution.width, resolution.height);

            resolutionDropdown.SetValueWithoutNotify(currentResolutionIndex);
            resolutionDropdown.RefreshShownValue();
#endif
        }

        public void SetResolutionWithoutNotify(int resolutionIndex, bool fullscreen)
        {
#if UNITY_WEBPLAYER
            return;
#else
            currentResolutionIndex = resolutionIndex;

            if (filteredResolutions == null || filteredResolutions.Count <= 0)
                return;

            Resolution resolution = filteredResolutions[resolutionIndex];

            //Debug.Log($"[SetResolutionWithoutNotify] resolution: 'x:{resolution.width}, y:{resolution.height}' resolutionIndex: '{resolutionIndex}'");

            if (resolution.width != GlobalVariables.currentGameResolution.width || resolution.height != GlobalVariables.currentGameResolution.height)
            {
                //Debug.Log($"Resolution has changed! Setting new resolution...");

                Screen.SetResolution(resolution.width, resolution.height, fullscreen);
                GlobalVariables.currentGameResolution = resolution;
            }

            currentResolution = new Vector2(resolution.width, resolution.height);

            resolutionDropdown.SetValueWithoutNotify(currentResolutionIndex);
            resolutionDropdown.RefreshShownValue();
#endif
        }

        private void OnStart()
        {
#if UNITY_WEBPLAYER
            return;
#else
            if (ini.Contains(group, $"i{keyX}") && ini.Contains(group, $"i{keyY}"))
            {
                savedValueX = ini.GetInt(group, $"i{keyX}");
                savedValueY = ini.GetInt(group, $"i{keyY}");

                for (int i = 0; i < filteredResolutions.Count; i++)
                {
                    // More lenient match allowing any refresh rate, because some systems report slightly different refresh rates
                    bool matchFree = unfilteredResolutions && filteredResolutions[i].width == savedValueX && filteredResolutions[i].height == savedValueY;

                    // Approximate match of refresh rate within tolerance, because some systems report slightly different refresh rates
                    // e.g. 59.94Hz vs 60Hz
                    // Strict match only allowing only resolutions with the monitor's current refresh rate
                    bool matchStrict = !unfilteredResolutions && filteredResolutions[i].width == savedValueX && filteredResolutions[i].height == savedValueY && Mathf.Abs((float)filteredResolutions[i].refreshRateRatio.value - currentRefreshRate) <= HzTolerance;

                    if (matchFree || matchStrict)
                    {
                        currentResolutionIndex = i;
                        currentResolution = new Vector2(Screen.width, Screen.height);
                    }
                }

                resolutionDropdown.value = currentResolutionIndex;
                resolutionDropdown.RefreshShownValue();
                resolutionDropdown.onValueChanged.Invoke(currentResolutionIndex);
                onStart.Invoke(currentResolution);
            }
#endif
        }

        public void SaveValue(int value)
        {
#if UNITY_WEBPLAYER
            return;
#else
            ini.Load(GlobalVariables.configPath);

            savedValueX = filteredResolutions[value].width;
            savedValueY = filteredResolutions[value].height;
            ini.Set(group, $"i{keyX}", savedValueX);
            ini.Set(group, $"i{keyY}", savedValueY);

            Utilities.FunctionTimer.Create(this, () => ini.Save(), 0.5f, $"{group}_{key}_dropdown_savevalue_delay", true, false);
#endif
        }
    }
}
#pragma warning restore 0162