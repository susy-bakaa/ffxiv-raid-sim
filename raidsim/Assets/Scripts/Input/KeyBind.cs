// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
// --------------------------------------------------------------
// Referenced: https://discussions.unity.com/t/how-to-get-the-nice-name-of-a-keycode/164747/2
// --------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dev.susybaka.raidsim.Inputs
{
    [System.Serializable]
    public class KeyBind
    {
        public KeyCode keyCode;
        public int mouseButton;

        public bool alt;
        public bool control;
        public bool shift;

        public static Dictionary<string, KeyBind> Keys;

        private static Dictionary<KeyCode, string> keyNames = new Dictionary<KeyCode, string>();
        private static bool keyNamesInitialized = false;

        public KeyBind()
        {
        }

        public KeyBind(KeyCode kc, bool a, bool c, bool s)
        {
            keyCode = kc;
            mouseButton = -1;
            alt = a;
            control = c;
            shift = s;
        }

        public KeyBind(int mb, bool a, bool c, bool s)
        {
            keyCode = KeyCode.None;
            mouseButton = mb;
            alt = a;
            control = c;
            shift = s;
        }

        public static void SetupKeyNames()
        {
            keyNamesInitialized = true;
            foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
                keyNames.TryAdd(k, k.ToString());
            // replace Alpha0, Alpha1, .. and Keypad0... with "0", "1", ...
            for (int i = 0; i < 10; i++)
            {
                keyNames[(KeyCode)((int)KeyCode.Alpha0 + i)] = i.ToString();
                keyNames[(KeyCode)((int)KeyCode.Keypad0 + i)] = i.ToString();
            }
            // replace Joystick1Button0, Joystick1Button1, ... with "J1B0", "J1B1", ...
            for (int j = 0; j < 9; j++) // assuming 8+1 joysticks
            {
                if (j == 0)
                {
                    for (int b = 0; b < 20; b++) // assuming 20 buttons per joystick
                    {
                        keyNames[(KeyCode)Enum.Parse(typeof(KeyCode), $"JoystickButton{b}")] = $"JB{b}";
                    }
                }
                else
                {
                    for (int b = 0; b < 20; b++) // assuming 20 buttons per joystick
                    {
                        keyNames[(KeyCode)Enum.Parse(typeof(KeyCode), $"Joystick{j}Button{b}")] = $"J{j}B{b}";
                    }
                }
            }
            keyNames[KeyCode.Period] = ".";
            keyNames[KeyCode.Comma] = ",";
            keyNames[KeyCode.Slash] = "/";
            keyNames[KeyCode.Ampersand] = "&";
            keyNames[KeyCode.Asterisk] = "*";
            keyNames[KeyCode.At] = "@";
            keyNames[KeyCode.Caret] = "^";
            keyNames[KeyCode.Colon] = ":";
            keyNames[KeyCode.Dollar] = "$";
            keyNames[KeyCode.Exclaim] = "!";
            keyNames[KeyCode.Greater] = ">";
            keyNames[KeyCode.Hash] = "#";
            keyNames[KeyCode.LeftParen] = "(";
            keyNames[KeyCode.LeftBracket] = "[";
            keyNames[KeyCode.Less] = "<";
            keyNames[KeyCode.Minus] = "-";
            keyNames[KeyCode.Percent] = "%";
            keyNames[KeyCode.Plus] = "+";
            keyNames[KeyCode.Question] = "?";
            keyNames[KeyCode.Quote] = "\"";
            keyNames[KeyCode.RightParen] = ")";
            keyNames[KeyCode.RightBracket] = "]";
            keyNames[KeyCode.Semicolon] = ";";
            keyNames[KeyCode.Underscore] = "_";
            keyNames[KeyCode.BackQuote] = "`";
            keyNames[KeyCode.Backslash] = "\\";
            keyNames[KeyCode.DoubleQuote] = "\"";
            keyNames[KeyCode.Equals] = "=";
            keyNames[KeyCode.LeftCurlyBracket] = "{";
            keyNames[KeyCode.RightCurlyBracket] = "}";
            keyNames[KeyCode.Pipe] = "|";
            keyNames[KeyCode.Tilde] = "~";
            keyNames[KeyCode.Equals] = "=";
            keyNames[KeyCode.Quote] = "'";
            keyNames[KeyCode.BackQuote] = "`";
            keyNames[KeyCode.PageDown] = "PD";
            keyNames[KeyCode.PageUp] = "PU";
            keyNames[KeyCode.Home] = "Hm";
            keyNames[KeyCode.End] = "End";
            keyNames[KeyCode.Break] = "PB";
            keyNames[KeyCode.Pause] = "PB";
            keyNames[KeyCode.SysReq] = "Sys";
            keyNames[KeyCode.Menu] = "Men";
            keyNames[KeyCode.Clear] = "Clr";
            keyNames[KeyCode.Help] = "Hel";
            keyNames[KeyCode.Print] = "Prt";
            keyNames[KeyCode.KeypadDivide] = "/";
            keyNames[KeyCode.KeypadMultiply] = "*";
            keyNames[KeyCode.KeypadMinus] = "-";
            keyNames[KeyCode.KeypadPlus] = "+";
            keyNames[KeyCode.KeypadEnter] = "Enter";
            keyNames[KeyCode.KeypadEquals] = "=";
            keyNames[KeyCode.KeypadPeriod] = ".";
            keyNames[KeyCode.LeftMeta] = "LM";
            keyNames[KeyCode.RightMeta] = "RM";
            keyNames[KeyCode.LeftWindows] = "LW";
            keyNames[KeyCode.RightWindows] = "RW";
            keyNames[KeyCode.RightCommand] = "RC";
            keyNames[KeyCode.LeftCommand] = "LC";
            keyNames[KeyCode.LeftApple] = "LA";
            keyNames[KeyCode.RightApple] = "RA";
            keyNames[KeyCode.Escape] = "Esc";
            keyNames[KeyCode.Delete] = "Del";
            keyNames[KeyCode.Insert] = "Ins";
            keyNames[KeyCode.UpArrow] = "(U)";
            keyNames[KeyCode.DownArrow] = "(D)";
            keyNames[KeyCode.LeftArrow] = "(L)";
            keyNames[KeyCode.RightArrow] = "(R)";
            keyNames[KeyCode.Space] = "Spc";
            keyNames[KeyCode.Backspace] = "BS";
            keyNames[KeyCode.Tab] = "Tab";
            keyNames[KeyCode.Return] = "Ent";
            keyNames[KeyCode.LeftControl] = "LCtrl";
            keyNames[KeyCode.RightControl] = "RCtrl";
            keyNames[KeyCode.LeftShift] = "LShift";
            keyNames[KeyCode.RightShift] = "RShift";
            keyNames[KeyCode.LeftAlt] = "LAlt";
            keyNames[KeyCode.RightAlt] = "RAlt";
            keyNames[KeyCode.CapsLock] = "CL";
            keyNames[KeyCode.Numlock] = "NL";
            keyNames[KeyCode.ScrollLock] = "SL";
        }

        public override string ToString()
        {
            string text = "";
            if (shift)
            {
                text = "Shift + " + text;
            }
            if (alt)
            {
                text = "Alt + " + text;
            }
            if (control)
            {
                text = "Control + " + text;
            }
            if (keyCode != KeyCode.None)
            {
                return text + keyCode.ToString();
            }
            return text + "Mouse" + (1 + mouseButton);
        }

        public string ToShortString()
        {
            if (!keyNamesInitialized)
            {
                SetupKeyNames();
            }

            string text = "<sprite=\"keys\" name=\"0\">";
            if (shift && !alt && !control)
            {
                text = "<sprite=\"keys\" name=\"2\">";
            }
            else if (alt && !shift && !control)
            {
                text = "<sprite=\"keys\" name=\"1\">";
            }
            else if (control && !shift && !alt)
            {
                text = "<sprite=\"keys\" name=\"3\">";
            }
            else if (shift && alt && !control)
            {
                text = "<sprite=\"keys\" name=\"4\">";
            }
            else if (shift && control && !alt)
            {
                text = "<sprite=\"keys\" name=\"5\">";
            }
            else if (alt && control && !shift)
            {
                text = "<sprite=\"keys\" name=\"7\">";
            }
            else if (shift && alt && control)
            {
                text = "<sprite=\"keys\" name=\"6\">";
            }
            if (keyCode != KeyCode.None)
            {
                return text + keyNames[keyCode];
            }
            else if (mouseButton > -1)
            {
                return text + "M" + (1 + mouseButton);
            }

            return text;
        }
    }
}