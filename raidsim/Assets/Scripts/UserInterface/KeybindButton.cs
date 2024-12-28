// Useful ref: https://discussions.unity.com/t/need-help-with-keybind-script-functionality/697577
// Most of the code referenced from the Korean TEA sim
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UserInput;

[RequireComponent(typeof(Button))]
public class KeybindButton : MonoBehaviour
{
    Button button;
    TextMeshProUGUI label;
    [SerializeField] public UserInput input;
    [SerializeField] public string keyName;
    public UnityEvent<KeyBind> onBind;

    private bool binding;

    private void Awake()
    {
        button = GetComponent<Button>();
        label = GetComponentInChildren<TextMeshProUGUI>();

        if (input == null)
        {
            input = FindObjectOfType<UserInput>();
        }

        if (button != null)
        {
            button.onClick.AddListener(Rebind);
        }
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(keyName) || input == null)
            return;

        input.keys.ForEach(i =>
        {
            if (i.name == keyName)
            {
                label.text = i.bind.ToString();
            }
        });
    }

    public void Rebind()
    {
        label.text = "Press a key";
        binding = true;
    }

    public void Rebind(KeyBind bind)
    {
        KeyBind keyBind = null;

        if (bind == null)
            return;

        if (bind.keyCode != KeyCode.None)
        {
            keyBind = new KeyBind(bind.keyCode, bind.alt, bind.control, bind.shift);
        }
        else if (bind.mouseButton > -1)
        {
            keyBind = new KeyBind(bind.mouseButton, bind.alt, bind.control, bind.shift);
        }

        if (keyBind != null)
        {
            input.Remap(keyName, keyBind);
            label.text = keyBind.ToString();
        }
    }

    public KeyBind GetCurrentBind()
    {
        return GetBind(keyName);
    }

    public KeyBind GetBind(string name)
    {
        if (string.IsNullOrEmpty(name) || input == null)
            return null;
        foreach (InputBinding i in input.keys)
        {
            if (i.name == name)
            {
                return i.bind;
            }
        }
        return null;
    }

    private void OnGUI()
    {
        if (string.IsNullOrEmpty(keyName) || input == null || !binding)
            return;

        Event current = Event.current;
        KeyBind keyBind = null;

        if (current.isKey)
        {
            string kc = current.keyCode.ToString();
            if (kc != "LeftShift" & kc != "LeftControl" & kc != "LeftAlt")
            {
                keyBind = new KeyBind(current.keyCode, current.alt, current.control, current.shift);
            }
        }
        else if (current.isMouse)
        {
            keyBind = new KeyBind(current.button, current.alt, current.control, current.shift);
        }

        if (keyBind != null)
        {
            input.Remap(keyName, keyBind);
            label.text = keyBind.ToString();
            onBind.Invoke(keyBind);
            binding = false;
        }
    }
}
