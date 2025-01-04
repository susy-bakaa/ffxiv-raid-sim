using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UserInput : MonoBehaviour
{
    public PlayerController characterController;
    public ActionController characterAction;
    public CharacterState characterState;
    public SimpleFreecam freecam;
    public ThirdPersonCamera cam;
    public TargetController targetController;
    public bool inputEnabled = true;
    public bool movementInputEnabled = true;
    public bool rotationInputEnabled = true;
    public bool zoomInputEnabled = true;
    public bool targetRaycastInputEnabled = true;
    public List<InputBinding> keys = new List<InputBinding>();
    private Dictionary<string, InputBinding> m_keys;
    public List<InputAxis> axes = new List<InputAxis>();
    private Dictionary<string, InputAxis> m_axes;

#if UNITY_EDITOR
    private void OnValidate()
    {
        for (int i = 0; i < axes.Count; i++)
        {
            InputAxis a = axes[i];
            InputBinding p = a.positive;
            p.name = $"{a.name}_Positive";
            a.positive = p;
            InputBinding n = a.negative;
            n.name = $"{a.name}_Negative";
            a.negative = n;
            axes[i] = a;
        }
    }
#endif

    void Awake()
    {
        if (KeyBind.Keys == null)
            KeyBind.Keys = new Dictionary<string, KeyBind>();
        if (m_axes == null)
            m_axes = new Dictionary<string, InputAxis>();
        if (m_keys == null)
            m_keys = new Dictionary<string, InputBinding>();

        if (keys != null && keys.Count > 0)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                KeyBind.Keys.TryAdd(keys[i].name, keys[i].bind);
                if (keys[i].action != null)
                    keys[i].action.currentKeybind = keys[i].bind;
                m_keys.TryAdd(keys[i].name, keys[i]);
            }
        }
        if (axes != null && axes.Count > 0)
        {
            for (int i = 0; i < axes.Count; i++)
            {
                KeyBind.Keys.TryAdd($"Axis_{axes[i].name}_Positive", axes[i].positive.bind);
                KeyBind.Keys.TryAdd($"Axis_{axes[i].name}_Negative", axes[i].negative.bind);
                if (axes[i].positive.action != null)
                    axes[i].positive.action.currentKeybind = axes[i].positive.bind;
                if (axes[i].negative.action != null)
                    axes[i].negative.action.currentKeybind = axes[i].negative.bind;
                m_axes.TryAdd(axes[i].name, axes[i]);
            }
        }
    }

    void Update()
    {
        if (characterController == null)
            return;
        if (characterAction == null)
            return;
        if (characterState == null)
            return;
        if (freecam == null)
            return;
        if (cam == null)
            return;

        if (!inputEnabled)
        {
            characterController.enableInput = false;
            freecam.enableSpeed = false;
            freecam.enableMovement = false; 
            freecam.enableRotation = false;
            cam.enableZooming = false;
            cam.enableMovement = false;
            cam.enableRotation = false;
            return;
        }
        else
        {
            if (characterController != null)
                characterController.enableInput = movementInputEnabled;
            if (freecam.active)
                characterController.enableInput = false;
            freecam.enableSpeed = zoomInputEnabled;
            freecam.enableMovement = movementInputEnabled;
            freecam.enableRotation = rotationInputEnabled;
            cam.enableZooming = zoomInputEnabled;
            cam.enableMovement = movementInputEnabled;
            cam.enableRotation = rotationInputEnabled;
        }

        if (targetController != null)
            targetController.canMouseRaycast = targetRaycastInputEnabled;

        if (!inputEnabled)
            return;

        if (keys != null && keys.Count > 0)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                //Debug.Log($"KeyBind {keys[i].bind} state {BindedKey(keys[i].bind)}");
                if (keys[i].bind != null && BindedKey(keys[i].bind))
                {
                    if (keys[i].action != null && characterAction != null)
                        characterAction.PerformAction(keys[i].action);
                    if (keys[i].statusEffect != null && characterState != null)
                        characterState.AddEffect(keys[i].statusEffect, characterState);
                    keys[i].onInput.Invoke();
                }
                else if (keys[i].bind != null && BindedKeyHeld(keys[i].bind))
                {
                    keys[i].onHeld.Invoke();
                }
            }
        }
        if (axes != null && axes.Count > 0)
        {
            for (int i = 0; i < axes.Count; i++)
            {
                if (axes[i].positive.bind != null && BindedKey(axes[i].positive.bind))
                {
                    if (axes[i].positive.action != null && characterAction != null)
                        characterAction.PerformAction(axes[i].positive.action);
                    if (axes[i].positive.statusEffect != null && characterState != null)
                        characterState.AddEffect(axes[i].positive.statusEffect, characterState);
                    axes[i].positive.onInput.Invoke();
                }
                else if (axes[i].positive.bind != null && BindedKeyHeld(axes[i].positive.bind))
                {
                    axes[i].positive.onHeld.Invoke();
                }
                if (axes[i].negative.bind != null && BindedKey(axes[i].negative.bind))
                {
                    if (axes[i].negative.action != null && characterAction != null)
                        characterAction.PerformAction(axes[i].negative.action);
                    if (axes[i].negative.statusEffect != null && characterState != null)
                        characterState.AddEffect(axes[i].negative.statusEffect, characterState);
                    axes[i].negative.onInput.Invoke();
                }
                else if (axes[i].negative.bind != null && BindedKeyHeld(axes[i].negative.bind))
                {
                    axes[i].negative.onHeld.Invoke();
                }
            }
        }

        if (BindedKey(KeyBind.Keys["ResetKey"]))
        {
            if (FightTimeline.Instance != null)
                FightTimeline.Instance.ResetPauseState();
            SceneManager.LoadScene("menu");
        }
    }

    public void VirtualKeyPress(string name)
    {
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i].name == name)
            {
                if (keys[i].action != null && characterAction != null)
                    characterAction.PerformAction(keys[i].action);
                if (keys[i].statusEffect != null && characterState != null)
                    characterState.AddEffect(keys[i].statusEffect, characterState);
                keys[i].onInput.Invoke();
            }
        }
    }

    public float GetAxisDown(string name)
    {
        float axis = 0;
        if (m_axes.TryGetValue(name, out InputAxis inputAxis))
        {
            if (inputAxis.positive.bind != null && BindedKey(inputAxis.positive.bind))
                axis += 1;
            if (inputAxis.negative.bind != null && BindedKey(inputAxis.negative.bind))
                axis -= 1;
        }
        return axis;
    }

    public float GetAxis(string name)
    {
        float axis = 0;

        if (Input.GetAxis(name) != 0)
        {
            if (name == "VerticalLegacy")
                return -Input.GetAxis(name);
            else
                return Input.GetAxis(name);
        }

        if (m_axes.TryGetValue(name, out InputAxis inputAxis))
        {
            if (inputAxis.positive.bind != null && BindedKeyHeld(inputAxis.positive.bind))
                axis += 1;
            if (inputAxis.negative.bind != null && BindedKeyHeld(inputAxis.negative.bind))
                axis -= 1;
        }
        return axis;
    }

    public bool GetAxisButton(string name)
    {
        if (m_axes.TryGetValue(name, out InputAxis inputAxis))
        {
            if (inputAxis.positive.bind != null && BindedKeyHeld(inputAxis.positive.bind))
                return true;
            else if (inputAxis.negative.bind != null && BindedKeyHeld(inputAxis.negative.bind))
                return true;
        }
        return false;
    }

    public bool GetAxisButtonDown(string name)
    {
        if (m_axes.TryGetValue(name, out InputAxis inputAxis))
        {
            if (inputAxis.positive.bind != null && BindedKey(inputAxis.positive.bind))
                return true;
            else if (inputAxis.negative.bind != null && BindedKey(inputAxis.negative.bind))
                return true;
        }
        return false;
    }

    public bool GetButtonDown(string name)
    {
        if (m_keys.TryGetValue(name, out InputBinding key))
        {
            if (key.bind != null)
                return BindedKey(key.bind);
        }
        return false;
    }

    public bool GetButton(string name)
    {
        if (m_keys.TryGetValue(name, out InputBinding key))
        {
            if (key.bind != null)
                return BindedKeyHeld(key.bind);
        }
        return false;
    }

    public bool BindedKey(KeyBind keyBind)
    {
        return (keyBind.keyCode != KeyCode.None && (Input.GetKeyDown(keyBind.keyCode) & Input.GetKey(KeyCode.LeftShift) == keyBind.shift & Input.GetKey(KeyCode.LeftAlt) == keyBind.alt & Input.GetKey(KeyCode.LeftControl) == keyBind.control)) || (keyBind.mouseButton != -1 && (Input.GetMouseButton(keyBind.mouseButton) & Input.GetKey(KeyCode.LeftShift) == keyBind.shift & Input.GetKey(KeyCode.LeftAlt) == keyBind.alt & Input.GetKey(KeyCode.LeftControl) == keyBind.control));
    }

    public bool BindedKeyHeld(KeyBind keyBind)
    {
        return (keyBind.keyCode != KeyCode.None && (Input.GetKey(keyBind.keyCode) & Input.GetKey(KeyCode.LeftShift) == keyBind.shift & Input.GetKey(KeyCode.LeftAlt) == keyBind.alt & Input.GetKey(KeyCode.LeftControl) == keyBind.control)) || (keyBind.mouseButton != -1 && (Input.GetMouseButton(keyBind.mouseButton) & Input.GetKey(KeyCode.LeftShift) == keyBind.shift & Input.GetKey(KeyCode.LeftAlt) == keyBind.alt & Input.GetKey(KeyCode.LeftControl) == keyBind.control));
    }

    public void Remap(string name, KeyBind bind)
    {
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i].name == name)
            {
                InputBinding updatedKey = keys[i];
                updatedKey.Rebind(bind);
                keys[i] = updatedKey;
                KeyBind.Keys[name] = bind;
                m_keys[name] = updatedKey;
            }
        }
    }

    public void RemapAxis(string name, KeyBind bindPositive, KeyBind bindNegative)
    {
        for (int i = 0; i < axes.Count; i++)
        {
            if (axes[i].name == name)
            {
                InputAxis updatedAxis = axes[i];
                updatedAxis.Rebind(bindPositive, bindNegative);
                axes[i] = updatedAxis;
                KeyBind.Keys[$"Axis_{name}_Positive"] = bindPositive;
                KeyBind.Keys[$"Axis_{name}_Negative"] = bindNegative;
                m_axes[name] = updatedAxis;
            }
        }
    }

    [System.Serializable]
    public struct InputBinding 
    {
        public string name;
        public KeyBind bind;
        public CharacterAction action;
        public StatusEffectData statusEffect;
        public UnityEvent onInput;
        public UnityEvent onHeld;

        public InputBinding(string name, KeyBind bind, CharacterAction action, StatusEffectData statusEffect, UnityEvent onInput, UnityEvent onHeld)
        {
            this.name = name;
            this.bind = bind;
            this.action = action;
            this.statusEffect = statusEffect;
            this.onInput = onInput;
            this.onHeld = onHeld;
        }

        public void Rebind(KeyBind bind)
        {
            this.bind = bind;
            if (action != null)
                action.currentKeybind = bind;
        }
    }

    [System.Serializable]
    public struct InputAxis
    {
        public string name;
        public InputBinding positive;
        public InputBinding negative;

        public InputAxis(string name, InputBinding positive, InputBinding negative)
        {
            this.name = name;
            this.positive = positive;
            InputBinding p = this.positive;
            p.name = $"{name}_Positive";
            this.positive = p;
            this.negative = negative;
            InputBinding n = this.negative;
            n.name = $"{name}_Negative";
            this.negative = n;
        }

        public void Rebind(KeyBind bindPositive, KeyBind bindNegative)
        {
            positive.Rebind(bindPositive);
            negative.Rebind(bindNegative);
        }
    }
}
