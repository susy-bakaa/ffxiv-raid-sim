// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using static dev.susybaka.raidsim.UI.KeybindButton;
using dev.illa4257;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.raidsim.UI;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.SaveLoad
{
    [RequireComponent(typeof(KeybindButton))]
    public class SaveKeybind : MonoBehaviour
    {
        KeybindButton keyBindButton;
        KeyBind keyBind;
        KeyBind keyBindNegative;
        int keyCode = -1;
        int mouseButton = -1;
        int alt = 0;
        int control = 0;
        int shift = 0;
        int keyCodeNegative = -1;
        int mouseButtonNegative = -1;
        int altNegative = 0;
        int controlNegative = 0;
        int shiftNegative = 0;
        bool axis = false;

        public string group = "";
        public string key = "UnnamedKeybind";
        private string negative = "Negative";
        private string keyCodeKey => $"i{key}KeyCode";
        private string mouseButtonKey => $"i{key}MouseButton";
        private string altKey => $"i{key}Alt";
        private string controlKey => $"i{key}Control";
        private string shiftKey => $"i{key}Shift";

        public UnityEvent<KeyBind> onStart;
        public UnityEvent<KeyBindPair> onStartPair;

        IniStorage ini;
        int id = 0;
        float wait;

        private void Awake()
        {
            keyBindButton = GetComponent<KeybindButton>();
            if (keyBindButton.axis)
            {
                axis = true;
                keyBind = keyBindButton.GetCurrentAxisBind(true);
                keyBindNegative = keyBindButton.GetCurrentAxisBind(false);
            }
            else
            {
                axis = false;
                keyBind = keyBindButton.GetCurrentBind();
            }
            ini = new IniStorage(GlobalVariables.configPath);
            wait = UnityEngine.Random.Range(0.15f, 0.65f);
            id = Random.Range(0, 10000);
        }

        private void Start()
        {
            if (!axis)
            {
                if (ini.Contains(group, keyCodeKey) || ini.Contains(group, mouseButtonKey))
                {
                    keyCode = ini.GetInt(group, keyCodeKey);
                    mouseButton = ini.GetInt(group, mouseButtonKey);
                    alt = ini.GetInt(group, altKey);
                    control = ini.GetInt(group, controlKey);
                    shift = ini.GetInt(group, shiftKey);

                    if ((KeyCode)keyCode != KeyCode.None)
                    {
                        keyBind = new KeyBind((KeyCode)keyCode, alt.ToBool(), control.ToBool(), shift.ToBool());
                    }
                    else if (mouseButton > -1)
                    {
                        keyBind = new KeyBind(mouseButton, alt.ToBool(), control.ToBool(), shift.ToBool());
                    }

                    if (keyBind != null)
                        keyBindButton.Rebind(keyBind);
                }
                onStart.Invoke(keyBind);
            }
            else
            {
                if ((ini.Contains(group, keyCodeKey) && ini.Contains(group, $"{keyCodeKey}{negative}")) || (ini.Contains(group, mouseButtonKey) && ini.Contains(group, $"{mouseButtonKey}{negative}")))
                {
                    keyCode = ini.GetInt(group, keyCodeKey);
                    mouseButton = ini.GetInt(group, mouseButtonKey);
                    alt = ini.GetInt(group, altKey);
                    control = ini.GetInt(group, controlKey);
                    shift = ini.GetInt(group, shiftKey);

                    keyCodeNegative = ini.GetInt(group, $"{keyCodeKey}{negative}");
                    mouseButtonNegative = ini.GetInt(group, $"{mouseButtonKey}{negative}");
                    altNegative = ini.GetInt(group, $"{altKey}{negative}");
                    controlNegative = ini.GetInt(group, $"{controlKey}{negative}");
                    shiftNegative = ini.GetInt(group, $"{shiftKey}{negative}");

                    if ((KeyCode)keyCode != KeyCode.None)
                    {
                        keyBind = new KeyBind((KeyCode)keyCode, alt.ToBool(), control.ToBool(), shift.ToBool());
                    }
                    else if (mouseButton > -1)
                    {
                        keyBind = new KeyBind(mouseButton, alt.ToBool(), control.ToBool(), shift.ToBool());
                    }

                    if ((KeyCode)keyCodeNegative != KeyCode.None)
                    {
                        keyBindNegative = new KeyBind((KeyCode)keyCodeNegative, altNegative.ToBool(), controlNegative.ToBool(), shiftNegative.ToBool());
                    }
                    else if (mouseButtonNegative > -1)
                    {
                        keyBindNegative = new KeyBind(mouseButtonNegative, altNegative.ToBool(), controlNegative.ToBool(), shiftNegative.ToBool());
                    }

                    if (keyBind != null && keyBindNegative != null)
                        keyBindButton.Rebind(keyBind, keyBindNegative);
                }
                onStartPair.Invoke(new KeyBindPair(keyBind, keyBindNegative));
            }
        }

        public void SaveValue(KeyBind value)
        {
            string n = gameObject.name;
            Utilities.FunctionTimer.Create(this, () => {
                ini.Load(GlobalVariables.configPath);

                keyCode = (int)value.keyCode;
                mouseButton = value.mouseButton;
                alt = value.alt.ToInt();
                control = value.control.ToInt();
                shift = value.shift.ToInt();

                ini.Set(group, keyCodeKey, keyCode);
                ini.Set(group, mouseButtonKey, mouseButton);
                ini.Set(group, altKey, alt);
                ini.Set(group, controlKey, control);
                ini.Set(group, shiftKey, shift);

                ini.Save();
            }, wait, $"SaveKeybind_{id}_{n}_savevalue_delay", false, true);
        }

        public void SaveValue(KeyBindPair value)
        {
            string n = gameObject.name;
            Utilities.FunctionTimer.Create(this, () => {
                ini.Load(GlobalVariables.configPath);

                keyCode = (int)value.positive.keyCode;
                mouseButton = value.positive.mouseButton;
                alt = value.positive.alt.ToInt();
                control = value.positive.control.ToInt();
                shift = value.positive.shift.ToInt();

                keyCodeNegative = (int)value.negative.keyCode;
                mouseButtonNegative = value.negative.mouseButton;
                altNegative = value.negative.alt.ToInt();
                controlNegative = value.negative.control.ToInt();
                shiftNegative = value.negative.shift.ToInt();

                ini.Set(group, keyCodeKey, keyCode);
                ini.Set(group, mouseButtonKey, mouseButton);
                ini.Set(group, altKey, alt);
                ini.Set(group, controlKey, control);
                ini.Set(group, shiftKey, shift);

                ini.Set(group, $"{keyCodeKey}{negative}", keyCodeNegative);
                ini.Set(group, $"{mouseButtonKey}{negative}", mouseButtonNegative);
                ini.Set(group, $"{altKey}{negative}", altNegative);
                ini.Set(group, $"{controlKey}{negative}", controlNegative);
                ini.Set(group, $"{shiftKey}{negative}", shiftNegative);

                ini.Save();
            }, wait, $"SaveKeybind_{id}_{n}_savevalue_delay", false, true);
        }
    }
}