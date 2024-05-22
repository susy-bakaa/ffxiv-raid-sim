using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyBind
{
    public KeyCode keyCode;
    public int mouseButton;

    public bool alt;
    public bool control;
    public bool shift;

    public static Dictionary<string, KeyBind> Keys;

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
}