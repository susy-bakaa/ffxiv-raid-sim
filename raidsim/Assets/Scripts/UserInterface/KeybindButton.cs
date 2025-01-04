// Useful ref: https://discussions.unity.com/t/need-help-with-keybind-script-functionality/697577
// Most of the code referenced from the Korean TEA sim
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
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
    TextMeshProUGUI labelNegative;
    [SerializeField] public UserInput input;
    [SerializeField] public bool axis = false;
    [SerializeField][ShowIf("axis")] public Button buttonNegative;
    [SerializeField] public string keyName;
    public UnityEvent<KeyBind> onBind;
    public UnityEvent<KeyBindPair> onBindAxis;

    private bool binding;
    private bool bindingNegative;

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
        if (buttonNegative != null && axis)
        {
            labelNegative = buttonNegative.GetComponentInChildren<TextMeshProUGUI>();
            buttonNegative.onClick.AddListener(RebindNegative);
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

    public void RebindNegative()
    {
        if (!axis)
            return;

        labelNegative.text = "Press a key";
        bindingNegative = true;
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

    public void Rebind(KeyBindPair binds)
    {
        Rebind(binds.positive, binds.negative);
    }

    public void Rebind(KeyBind positive, KeyBind negative)
    {
        KeyBind keyBind = null;
        KeyBind negativeKeyBind = null;

        if (positive == null)
            return;
        if (negative == null)
            return;

        if (positive.keyCode != KeyCode.None)
        {
            keyBind = new KeyBind(positive.keyCode, positive.alt, positive.control, positive.shift);
        }
        else if (positive.mouseButton > -1)
        {
            keyBind = new KeyBind(positive.mouseButton, positive.alt, positive.control, positive.shift);
        }

        if (negative.keyCode != KeyCode.None)
        {
            negativeKeyBind = new KeyBind(negative.keyCode, negative.alt, negative.control, negative.shift);
        }
        else if (negative.mouseButton > -1)
        {
            negativeKeyBind = new KeyBind(negative.mouseButton, negative.alt, negative.control, negative.shift);
        }

        if (keyBind != null && negativeKeyBind != null)
        {
            input.RemapAxis(keyName, keyBind, negativeKeyBind);
            label.text = keyBind.ToString();
            labelNegative.text = negativeKeyBind.ToString();
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

    public KeyBind GetCurrentAxisBind(bool positive)
    {
        return GetAxisBind(keyName, positive);
    }

    public KeyBind GetAxisBind(string name, bool positive)
    {
        if (string.IsNullOrEmpty(name) || input == null)
            return null;
        foreach (InputAxis i in input.axes)
        {
            if (i.name == name)
            {
                if (positive)
                    return i.positive.bind;
                else
                    return i.negative.bind;
            }
        }
        return null;
    }

    private void OnGUI()
    {
        if (string.IsNullOrEmpty(keyName) || input == null || (!binding && !bindingNegative))
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
            if (!axis)
            {
                input.Remap(keyName, keyBind);
                label.text = keyBind.ToString();
                onBind.Invoke(keyBind);
            }
            else
            {
                if (bindingNegative)
                {
                    KeyBind keyBindPositive = GetCurrentAxisBind(true);
                    input.RemapAxis(keyName, keyBindPositive, keyBind);
                    label.text = keyBindPositive.ToString();
                    labelNegative.text = keyBind.ToString();
                    onBindAxis.Invoke(new KeyBindPair(keyBindPositive, keyBind));
                }
                else
                {
                    KeyBind keyBindNegative = GetCurrentAxisBind(false);
                    input.RemapAxis(keyName, keyBind, keyBindNegative);
                    label.text = keyBind.ToString();
                    labelNegative.text = keyBindNegative.ToString();
                    onBindAxis.Invoke(new KeyBindPair(keyBind, keyBindNegative));
                }
            }
            binding = false;
            bindingNegative = false;
        }
    }

    [System.Serializable]
    public struct KeyBindPair
    {
        public KeyBind positive;
        public KeyBind negative;

        public KeyBindPair(KeyBind positive, KeyBind negative)
        {
            this.positive = positive;
            this.negative = negative;
        }
    }
}
