using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

[RequireComponent(typeof(TMP_Dropdown))]
public class ResolutionDropdown : MonoBehaviour
{
    private TMP_Dropdown resolutionDropdown;
    [SerializeField] private GameObject arrowStandard;
    [SerializeField] private GameObject arrowWhenExpanded;

    private Resolution[] resolutions;

    private List<Resolution> filteredResolutions;

    private float currentRefreshRate;
    private int currentResolutionIndex = 0;

    private Vector2 currentResolution;
    int savedValueX = 0;
    int savedValueY = 0;

    public string group = "";
    public string key = "UnnamedResolution";

    private string keyX { get { return $"{key}Width"; } }
    private string keyY { get { return $"{key}Height"; } }

    public UnityEvent<Vector2> onStart;

    IniStorage ini;

    void Awake()
    {
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
            if ((float)resolutions[i].refreshRateRatio.value == currentRefreshRate)
            {
                filteredResolutions.Add(resolutions[i]);
            }
        }

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

            options.Add(resolutionOption);

            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height && (float)filteredResolutions[i].refreshRateRatio.value == currentRefreshRate)
            {
                currentResolutionIndex = i;
                currentResolution = new Vector2(Screen.width, Screen.height);
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex = 0;
        resolutionDropdown.RefreshShownValue();
        SetResolution(currentResolutionIndex, true);
    }

    void Start()
    {
        Utilities.FunctionTimer.Create(this, () => OnStart(), Random.Range(1f, 1.25f), $"{group}_{key}_resolution_onstart_delay", true, true);
    }

    void Update()
    {
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
    }

    public void SetResolution(int resolutionIndex, bool fullscreen)
    {
        currentResolutionIndex = resolutionIndex;

        Resolution resolution = filteredResolutions[resolutionIndex];

        Screen.SetResolution(resolution.width, resolution.height, fullscreen);

        currentResolution = new Vector2(resolution.width, resolution.height);

        resolutionDropdown.SetValueWithoutNotify(currentResolutionIndex);
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolutionWithoutNotify(int resolutionIndex, bool fullscreen)
    {
        currentResolutionIndex = resolutionIndex;

        Resolution resolution = filteredResolutions[resolutionIndex];

        Screen.SetResolution(resolution.width, resolution.height, fullscreen);

        currentResolution = new Vector2(resolution.width, resolution.height);

        resolutionDropdown.SetValueWithoutNotify(currentResolutionIndex);
        resolutionDropdown.RefreshShownValue();
    }

    private void OnStart()
    {
        if (ini.Contains(group, $"i{keyX}") && ini.Contains(group, $"i{keyY}"))
        {
            savedValueX = ini.GetInt(group, $"i{keyX}");
            savedValueY = ini.GetInt(group, $"i{keyY}");

            for (int i = 0; i < filteredResolutions.Count; i++)
            {
                if (filteredResolutions[i].width == savedValueX && filteredResolutions[i].height == savedValueY && (float)filteredResolutions[i].refreshRateRatio.value == currentRefreshRate)
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
    }

    public void SaveValue(int value)
    {
        ini.Load(GlobalVariables.configPath);

        savedValueX = filteredResolutions[value].width;
        savedValueY = filteredResolutions[value].height;
        ini.Set(group, $"i{keyX}", savedValueX);
        ini.Set(group, $"i{keyY}", savedValueY);

        Utilities.FunctionTimer.Create(this, () => ini.Save(), 0.5f, $"{group}_{key}_dropdown_savevalue_delay", true, false);
    }
}