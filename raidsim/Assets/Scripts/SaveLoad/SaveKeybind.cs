using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(KeybindButton))]
public class SaveKeybind : MonoBehaviour
{
    KeybindButton keyBindButton;
    KeyBind keyBind;
    int keyCode = -1;
    int mouseButton = -1;
    int alt = 0;
    int control = 0;
    int shift = 0;

    public string group = "";
    public string key = "UnnamedKeybind";
    private string keyCodeKey => $"i{key}KeyCode";
    private string mouseButtonKey => $"i{key}MouseButton";
    private string altKey => $"i{key}Alt";
    private string controlKey => $"i{key}Control";
    private string shiftKey => $"i{key}Shift";

    public UnityEvent<KeyBind> onStart;

    IniStorage ini;
    int id = 0;
    float wait;

    void Awake()
    {
        keyBindButton = GetComponent<KeybindButton>();
        keyBind = keyBindButton.GetCurrentBind();
        ini = new IniStorage(GlobalVariables.configPath);
        wait = UnityEngine.Random.Range(0.15f, 0.65f);
        id = Random.Range(0, 10000);
    }

    void Start()
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
}
