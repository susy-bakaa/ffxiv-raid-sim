// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
using UnityEngine;
using UnityEngine.Events;
using dev.illa4257;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.SaveLoad
{
    [RequireComponent(typeof(Hotbar))]
    public class SaveHotbar : MonoBehaviour
    {
        [SerializeField] private Hotbar target;
        [SerializeField] private CharacterState character;
        [SerializeField] private float randomDelay = 0.5f;
        [SerializeField] private bool onlySaveOnChange = true;
        [SerializeField] private string presetName = "hotbars";

        public UnityEvent onStart;

        IniStorage ini;
        IniStorage presetIni;
        HotbarGroupSnapshot lastSnapshot;
        string group = string.Empty;

#if UNITY_EDITOR
        [NaughtyAttributes.Button("Export Current Hotbar to Preset")]
        public void ExportCurrentToPreset()
        {
            var path = System.IO.Path.Combine(Application.streamingAssetsPath, $"{presetName}.ini");

            var presetIni = new IniStorage();
            presetIni.Load(path);

            string section = SectionFor(target.Controller.GetGroupDefinition(target.GroupId).saveScope, group, GlobalVariables.roleNames[(int)character.role]);
            var snap = target.Controller.CreateSnapshot(target.GroupId);
            var json = JsonUtility.ToJson(snap);
            presetIni.Set(section, $"s{target.GroupId}", json);
            presetIni.Save();
        }
#endif

        private void Awake()
        {
            if (target == null)
                Debug.LogError("Target hotbar null!", gameObject);

            if (character == null)
                Debug.LogError("Character state null!", gameObject);

            var path = System.IO.Path.Combine(Application.streamingAssetsPath, $"{presetName}.ini");

            ini = new IniStorage(GlobalVariables.configPath);
            presetIni = new IniStorage(path);
            group = FightTimeline.Instance.timelineAbbreviation;
            lastSnapshot = null;
        }

        private void Start()
        {
            string section = SectionFor(target.Controller.GetGroupDefinition(target.GroupId).saveScope, group, GlobalVariables.roleNames[(int)character.role]);
            randomDelay = Random.Range(randomDelay, randomDelay + 0.2f);
            Utilities.FunctionTimer.Create(this, () => OnStart(), Random.Range(1f, 1.25f), $"{section}_savehotbar_onstart_delay", true, false);
        }

        private void OnStart()
        {
            if (LoadValue())
            {
                onStart.Invoke();
            }
            else
            {
                LoadDefaults();
            }
        }

        public void SaveValue()
        {
            string section = SectionFor(target.Controller.GetGroupDefinition(target.GroupId).saveScope, group, GlobalVariables.roleNames[(int)character.role]);

            Utilities.FunctionTimer.Create(this, () => {
                ini.Load(GlobalVariables.configPath);
                var snap = target.Controller.CreateSnapshot(target.GroupId);
                var json = JsonUtility.ToJson(snap);
                // Only save if changed, or if we're ignoring that check
                // This is done so if nothing changed we avoid writing defaults to users config if they never actually change their hotbar from the default
                if (!snap.ValueEquals(lastSnapshot) || !onlySaveOnChange) 
                {
                    ini.Set(section, $"s{target.GroupId}", json);
                    ini.Save();
                    lastSnapshot = snap;
                }
            }, randomDelay, $"{section}_savehotbar_savevalue_delay", true, false);
        }

        public bool LoadValue()
        {
            bool result = false;
            string section = SectionFor(target.Controller.GetGroupDefinition(target.GroupId).saveScope, group, GlobalVariables.roleNames[(int)character.role]);
            if (ini.Contains(section, $"s{target.GroupId}"))
            {
                var json = ini.GetString(section, $"s{target.GroupId}");
                if (!string.IsNullOrEmpty(json))
                {
                    var snap = JsonUtility.FromJson<HotbarGroupSnapshot>(json);
                    lastSnapshot = snap;
                    target.Controller.ApplySnapshot(snap, true);
                    result = true;
                }
            }
            return result;
        }

        public void LoadDefaults()
        {
            string section = SectionFor(target.Controller.GetGroupDefinition(target.GroupId).saveScope, group, GlobalVariables.roleNames[(int)character.role]);

            // First remove any existing saved data for this hotbar,
            // so if the user resets to defaults after making changes we don't have the old changes or these defaults saved in their config
            // and their hotbar will just follow the defaults from the preset file
            ini.Load(GlobalVariables.configPath);
            if (ini.Contains(section, $"s{target.GroupId}"))
            {
                ini.Remove(section, $"s{target.GroupId}");
                ini.RemoveGroup(section);
                ini.Save();
            }
            
            if (presetIni.Contains(section, $"s{target.GroupId}"))
            {
                var json = presetIni.GetString(section, $"s{target.GroupId}");
                if (!string.IsNullOrEmpty(json))
                {
                    var snap = JsonUtility.FromJson<HotbarGroupSnapshot>(json);
                    lastSnapshot = snap;
                    target.Controller.ApplySnapshot(snap, true);
                }
            }
        }

        public void Reload()
        {
            Start();
        }

        private static string SectionFor(HotbarSaveScope scope, string timelineId, string roleId)
        {
            return scope switch
            {
                HotbarSaveScope.TimelineOnly => $"{timelineId}:Hotbar",
                HotbarSaveScope.TimelineAndRole => $"{timelineId}:Hotbar:{roleId}",
                HotbarSaveScope.Global => "Global:Hotbar",
                _ => $"{timelineId}:Hotbar:{roleId}"
            };
        }
    }
}