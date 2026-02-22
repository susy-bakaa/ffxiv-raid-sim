// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
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

        public UnityEvent onStart;

        IniStorage ini;
        string group = string.Empty;

#if UNITY_EDITOR
        [Header("Editor")]
        [SerializeField] private string presetName = "default";
        [NaughtyAttributes.Button("Export Current Hotbar to Preset")]
        public void ExportCurrentToPreset()
        {
            // You can use your IniStorage if it supports arbitrary paths;
            // otherwise write manually.
            var path = System.IO.Path.Combine(Application.streamingAssetsPath, $"{presetName}.ini");

            var presetIni = new IniStorage();
            presetIni.Load(path); // or start empty if your IniStorage supports it

            string section = SectionFor(target.Controller.GetGroupDefinition(target.GroupId).saveScope, group, character.role.ToString());
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

            //target.Controller.OnGroupChanged

            ini = new IniStorage(GlobalVariables.configPath);
            group = FightTimeline.Instance.timelineAbbreviation;
        }

        private void Start()
        {
            string section = SectionFor(target.Controller.GetGroupDefinition(target.GroupId).saveScope, group, character.role.ToString());
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
                // TODO load defaults
            }
        }

        public void SaveValue()
        {
            string section = SectionFor(target.Controller.GetGroupDefinition(target.GroupId).saveScope, group, character.role.ToString());

            Utilities.FunctionTimer.Create(this, () => {
                ini.Load(GlobalVariables.configPath);
                var snap = target.Controller.CreateSnapshot(target.GroupId);
                var json = JsonUtility.ToJson(snap);
                ini.Set(section, $"s{target.GroupId}", json);
                // Optional safety if your INI can't handle braces/quotes well:
                // json = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
                ini.Save();
            }, randomDelay, $"{section}_savehotbar_savevalue_delay", true, false);
        }

        public bool LoadValue()
        {
            bool result = false;
            string section = SectionFor(target.Controller.GetGroupDefinition(target.GroupId).saveScope, group, character.role.ToString());
            if (ini.Contains(section, $"s{target.GroupId}"))
            {
                var json = ini.GetString(section, $"s{target.GroupId}");
                // If you encoded to Base64, decode here:
                // json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(json));
                if (!string.IsNullOrEmpty(json))
                {
                    var snap = JsonUtility.FromJson<HotbarGroupSnapshot>(json);
                    target.Controller.ApplySnapshot(snap, true);
                    result = true;
                }
            }
            return result;
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