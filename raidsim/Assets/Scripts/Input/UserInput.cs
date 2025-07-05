using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.StatusEffects;
using dev.susybaka.raidsim.Targeting;

namespace dev.susybaka.raidsim.Inputs
{
    public class UserInput : MonoBehaviour
    {
        public PlayerController characterController;
        public ActionController characterAction;
        public CharacterState characterState;
        public SimpleFreecam freecam;
        public ThirdPersonCamera cam;
        public TargetController targetController;
        public CanvasGroupToggleChildren debugOverlays;
        public bool inputEnabled = true;
        public bool movementInputEnabled = true;
        public bool rotationInputEnabled = true;
        public bool zoomInputEnabled = true;
        public bool targetRaycastInputEnabled = true;
        public bool usingController = false;
        public List<InputActionReference> controllerModifierKeys;
        public List<InputBinding> keys = new List<InputBinding>();
        private Dictionary<string, InputBinding> m_keys;
        public List<InputAxis> axes = new List<InputAxis>();
        private Dictionary<string, InputAxis> m_axes;
        private bool mutedBgm = false;

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

        private void OnEnable()
        {
            LinkControllerBinds();
        }

        private void OnDisable()
        {
            UnlinkControllerBinds();
        }

        private void Awake()
        {
            if (debugOverlays == null)
                debugOverlays = GameObject.Find("SimDebugOverlays").GetComponent<CanvasGroupToggleChildren>();

            if (debugOverlays != null && debugOverlays.Group != null)
            {
                debugOverlays.Group.alpha = 0f;
                debugOverlays.UpdateState();
            }

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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                GlobalVariables.muteBgm = !GlobalVariables.muteBgm;

                Debug.Log($"GlobalVariables.muteBgm: {GlobalVariables.muteBgm}");

                if (GlobalVariables.muteBgm && !mutedBgm)
                {
#if !UNITY_WEBPLAYER
                    Transform musicLoader = GameObject.Find("MusicLoader").transform;
                    AudioSource[] musicSources = musicLoader.GetComponentsInChildren<AudioSource>();

                    for (int i = 0; i < musicSources.Length; i++)
                    {
                        musicSources[i].volume = 0f;
                    }
#endif
                    mutedBgm = true;
                }
                else if (!GlobalVariables.muteBgm && mutedBgm)
                {
#if !UNITY_WEBPLAYER
                    Transform musicLoader = GameObject.Find("MusicLoader").transform;
                    AudioSource[] musicSources = musicLoader.GetComponentsInChildren<AudioSource>();

                    for (int i = 0; i < musicSources.Length; i++)
                    {
                        musicSources[i].volume = 1f;
                    }
#endif
                    mutedBgm = false;
                }
            }

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

            if (keys != null && keys.Count > 0)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    if (inputEnabled || keys[i].neverDisable)
                    {
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
            }
            if (axes != null && axes.Count > 0)
            {
                for (int i = 0; i < axes.Count; i++)
                {
                    if (inputEnabled || axes[i].neverDisable)
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
            }

            if (BindedKey(KeyBind.Keys["ResetKey"]))
            {
                ResetKey();
            }

            if (BindedKey(KeyBind.Keys["ToggleDebugOverlaysKey"]))
            {
                if (debugOverlays != null && debugOverlays.Group != null)
                {
                    if (debugOverlays.Group.alpha == 0f)
                        debugOverlays.Group.alpha = 1f;
                    else
                        debugOverlays.Group.alpha = 0f;
                }
            }
        }

        public void ResetKey()
        {
            if (FightTimeline.Instance != null)
                FightTimeline.Instance.ResetPauseState();
            SceneManager.LoadScene("menu");
        }

        public void VirtualKeyPress(int index)
        {
            if (keys.Count > index)
            {
                if (keys[index].action != null && characterAction != null)
                    characterAction.PerformAction(keys[index].action);
                if (keys[index].statusEffect != null && characterState != null)
                    characterState.AddEffect(keys[index].statusEffect, characterState);
                keys[index].onInput.Invoke();
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

        public void LinkControllerBinds()
        {
            for (int i = 0; i < controllerModifierKeys.Count; i++)
            {
                controllerModifierKeys[i].action.Enable();
            }

            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].hasControllerBinding && keys[i].controllerBind != null)
                {
                    keys[i].controllerBind.action.Enable();

                    // Capture the index inside a separate variable
                    int capturedIndex = i;
                    keys[i].controllerBind.action.performed += (ctx) =>
                    {
                        // Check if at least one modifier key is pressed
                        if (keys[capturedIndex].requiresModifier && IsAnyModifierPressed())
                        {
                            VirtualKeyPress(capturedIndex);
                        }
                        else if (!keys[capturedIndex].requiresModifier && !IsAnyModifierPressed())
                        {
                            VirtualKeyPress(capturedIndex);
                        }
                    };
                }
            }
        }

        public void UnlinkControllerBinds()
        {
            for (int i = 0; i < controllerModifierKeys.Count; i++)
            {
                controllerModifierKeys[i].action.Disable();
            }

            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].hasControllerBinding && keys[i].controllerBind != null)
                {
                    keys[i].controllerBind.action.Enable();

                    // Capture the index inside a separate variable
                    int capturedIndex = i;
                    keys[i].controllerBind.action.performed -= (ctx) =>
                    {
                        // Check if at least one modifier key is pressed
                        if (keys[capturedIndex].requiresModifier && IsAnyModifierPressed())
                        {
                            VirtualKeyPress(capturedIndex);
                        }
                        else if (!keys[capturedIndex].requiresModifier && !IsAnyModifierPressed())
                        {
                            VirtualKeyPress(capturedIndex);
                        }
                    };
                }
            }
        }

        // Helper function to check if ANY modifier key is currently pressed
        private bool IsAnyModifierPressed()
        {
            foreach (var modifier in controllerModifierKeys)
            {
                if (modifier != null && modifier.action.IsPressed())
                {
                    return true; // At least one modifier key is held
                }
            }
            return false; // No modifier keys are held
        }

        public float GetAxisDown(string name)
        {
            float axis = 0;
            if (m_axes.TryGetValue(name, out InputAxis inputAxis))
            {
                if (inputAxis.hasControllerBinding && inputAxis.controllerBind != null)
                {
                    Vector2 controllerVector = inputAxis.controllerBind.action.ReadValue<Vector2>();

                    if (controllerVector != Vector2.zero)
                    {
                        switch (inputAxis.controllerBindUseAxis)
                        {
                            case NewInputAxis.x:
                                axis = controllerVector.x;
                                break;
                            case NewInputAxis.y:
                                axis = controllerVector.y;
                                break;
                            case NewInputAxis.z:
                                axis = 0;
                                break;
                        }
                        usingController = true;
                    }
                }

                if (inputAxis.weakModifierKeys)
                {
                    if (inputAxis.positive.bind != null && AnyBindedKey(inputAxis.positive.bind))
                    {
                        usingController = false;
                        axis += 1;
                    }
                    if (inputAxis.negative.bind != null && AnyBindedKey(inputAxis.negative.bind))
                    {
                        usingController = false;
                        axis -= 1;
                    }
                }
                else
                {
                    if (inputAxis.positive.bind != null && BindedKey(inputAxis.positive.bind))
                    {
                        usingController = false;
                        axis += 1;
                    }
                    if (inputAxis.negative.bind != null && BindedKey(inputAxis.negative.bind))
                    {
                        usingController = false;
                        axis -= 1;
                    }
                }
            }
            return axis;
        }

        public float GetAxis(string name)
        {
            float axis = 0;

            if (m_axes.TryGetValue(name, out InputAxis inputAxis))
            {
                if (inputAxis.hasControllerBinding && inputAxis.controllerBind != null)
                {
                    Vector2 controllerVector = inputAxis.controllerBind.action.ReadValue<Vector2>();

                    //Debug.Log($"controllerVector {controllerVector}");

                    if (controllerVector != Vector2.zero)
                    {
                        switch (inputAxis.controllerBindUseAxis)
                        {
                            case NewInputAxis.x:
                                axis = controllerVector.x;
                                break;
                            case NewInputAxis.y:
                                axis = controllerVector.y;
                                break;
                            case NewInputAxis.z:
                                axis = 0;
                                break;
                        }
                        usingController = true;
                    }
                }

                if (inputAxis.weakModifierKeys)
                {
                    if (inputAxis.positive.bind != null && AnyBindedKeyHeld(inputAxis.positive.bind))
                    {
                        usingController = false;
                        axis += 1;
                    }
                    if (inputAxis.negative.bind != null && AnyBindedKeyHeld(inputAxis.negative.bind))
                    {
                        usingController = false;
                        axis -= 1;
                    }
                }
                else
                {
                    if (inputAxis.positive.bind != null && BindedKeyHeld(inputAxis.positive.bind))
                    {
                        usingController = false;
                        axis += 1;
                    }
                    if (inputAxis.negative.bind != null && BindedKeyHeld(inputAxis.negative.bind))
                    {
                        usingController = false;
                        axis -= 1;
                    }
                }
            }
            return axis;
        }

        public bool GetAxisButton(string name)
        {
            if (m_axes.TryGetValue(name, out InputAxis inputAxis))
            {
                if (inputAxis.hasControllerBinding && inputAxis.controllerBind != null)
                {
                    Vector2 controllerVector = inputAxis.controllerBind.action.ReadValue<Vector2>();

                    if (controllerVector != Vector2.zero)
                    {
                        switch (inputAxis.controllerBindUseAxis)
                        {
                            case NewInputAxis.x:
                                if (controllerVector.x != 0)
                                    return true;
                                break;
                            case NewInputAxis.y:
                                if (controllerVector.y != 0)
                                    return true;
                                break;
                        }
                        usingController = true;
                    }
                }

                if (inputAxis.weakModifierKeys)
                {
                    if (inputAxis.positive.bind != null && AnyBindedKeyHeld(inputAxis.positive.bind))
                    {
                        usingController = false;
                        return true;
                    }
                    else if (inputAxis.negative.bind != null && AnyBindedKeyHeld(inputAxis.negative.bind))
                    {
                        usingController = false;
                        return true;
                    }
                }
                else
                {
                    if (inputAxis.positive.bind != null && BindedKeyHeld(inputAxis.positive.bind))
                    {
                        usingController = false;
                        return true;
                    }
                    else if (inputAxis.negative.bind != null && BindedKeyHeld(inputAxis.negative.bind))
                    {
                        usingController = false;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool GetAxisButtonDown(string name)
        {
            if (m_axes.TryGetValue(name, out InputAxis inputAxis))
            {
                if (inputAxis.hasControllerBinding && inputAxis.controllerBind != null)
                {
                    Vector2 controllerVector = inputAxis.controllerBind.action.ReadValue<Vector2>();

                    if (controllerVector != Vector2.zero)
                    {
                        switch (inputAxis.controllerBindUseAxis)
                        {
                            case NewInputAxis.x:
                                if (controllerVector.x != 0)
                                    return true;
                                break;
                            case NewInputAxis.y:
                                if (controllerVector.y != 0)
                                    return true;
                                break;
                        }
                        usingController = true;
                    }
                }

                if (inputAxis.weakModifierKeys)
                {
                    if (inputAxis.positive.bind != null && AnyBindedKey(inputAxis.positive.bind))
                    {
                        usingController = false;
                        return true;
                    }
                    else if (inputAxis.negative.bind != null && AnyBindedKey(inputAxis.negative.bind))
                    {
                        usingController = false;
                        return true;
                    }
                }
                else
                {
                    if (inputAxis.positive.bind != null && BindedKey(inputAxis.positive.bind))
                    {
                        usingController = false;
                        return true;
                    }
                    else if (inputAxis.negative.bind != null && BindedKey(inputAxis.negative.bind))
                    {
                        usingController = false;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool GetButtonDown(string name)
        {
            if (m_keys.TryGetValue(name, out InputBinding key))
            {
                //Debug.Log($"key {name} key.hasControllerBinding {key.hasControllerBinding} key.controllerBind {key.controllerBind}");

                if (key.hasControllerBinding && key.controllerBind != null)
                {

                    if (key.requiresModifier)
                    {
                        for (int i = 0; i < controllerModifierKeys.Count; i++)
                        {
                            if (controllerModifierKeys[i].action.IsPressed() && key.controllerBind.action.WasPressedThisFrame())
                            {
                                usingController = true;
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (key.controllerBind.action.WasPressedThisFrame())
                        {
                            usingController = true;
                            return true;
                        }
                    }
                }

                if (key.bind != null)
                {
                    bool result = BindedKey(key.bind);

                    if (result)
                        usingController = false;

                    return result;
                }
            }
            return false;
        }

        public bool GetButton(string name)
        {
            if (m_keys.TryGetValue(name, out InputBinding key))
            {
                //Debug.Log($"key {name} key.hasControllerBinding {key.hasControllerBinding} key.controllerBind {key.controllerBind}");

                if (key.hasControllerBinding && key.controllerBind != null)
                {
                    Debug.Log($"button {name}");

                    if (key.requiresModifier)
                    {
                        for (int i = 0; i < controllerModifierKeys.Count; i++)
                        {
                            if (controllerModifierKeys[i].action.IsPressed() && key.controllerBind.action.IsPressed())
                            {
                                usingController = true;
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (key.controllerBind.action.IsPressed())
                        {
                            usingController = true;
                            return true;
                        }
                    }
                }

                if (key.bind != null)
                {
                    bool result = BindedKeyHeld(key.bind);

                    if (result)
                        usingController = false;

                    return result;
                }
            }
            return false;
        }

        public bool BindedKey(KeyBind keyBind)
        {
            return (keyBind.keyCode != KeyCode.None && (Input.GetKeyDown(keyBind.keyCode) & Input.GetKey(KeyCode.LeftShift) == keyBind.shift & Input.GetKey(KeyCode.LeftAlt) == keyBind.alt & Input.GetKey(KeyCode.LeftControl) == keyBind.control)) || (keyBind.mouseButton != -1 && (Input.GetMouseButton(keyBind.mouseButton) & Input.GetKey(KeyCode.LeftShift) == keyBind.shift & Input.GetKey(KeyCode.LeftAlt) == keyBind.alt & Input.GetKey(KeyCode.LeftControl) == keyBind.control));
        }

        public bool AnyBindedKey(KeyBind keyBind)
        {
            return (keyBind.keyCode != KeyCode.None && Input.GetKeyDown(keyBind.keyCode)) || (keyBind.mouseButton > -1 && Input.GetMouseButtonDown(keyBind.mouseButton));
        }

        public bool BindedKeyHeld(KeyBind keyBind)
        {
            return (keyBind.keyCode != KeyCode.None && (Input.GetKey(keyBind.keyCode) & Input.GetKey(KeyCode.LeftShift) == keyBind.shift & Input.GetKey(KeyCode.LeftAlt) == keyBind.alt & Input.GetKey(KeyCode.LeftControl) == keyBind.control)) || (keyBind.mouseButton != -1 && (Input.GetMouseButton(keyBind.mouseButton) & Input.GetKey(KeyCode.LeftShift) == keyBind.shift & Input.GetKey(KeyCode.LeftAlt) == keyBind.alt & Input.GetKey(KeyCode.LeftControl) == keyBind.control));
        }

        public bool AnyBindedKeyHeld(KeyBind keyBind)
        {
            return (keyBind.keyCode != KeyCode.None && Input.GetKey(keyBind.keyCode)) || (keyBind.mouseButton > -1 && Input.GetMouseButton(keyBind.mouseButton));
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
            public bool hasControllerBinding;
            public bool requiresModifier;
            public bool neverDisable;
            public InputActionReference controllerBind;

            public InputBinding(string name, KeyBind bind, CharacterAction action, StatusEffectData statusEffect, UnityEvent onInput, UnityEvent onHeld, bool hasControllerBinding, bool requiresModifier, bool neverDisable, InputActionReference controllerBind)
            {
                this.name = name;
                this.bind = bind;
                this.action = action;
                this.statusEffect = statusEffect;
                this.onInput = onInput;
                this.onHeld = onHeld;
                this.hasControllerBinding = hasControllerBinding;
                this.requiresModifier = requiresModifier;
                this.neverDisable = neverDisable;
                this.controllerBind = controllerBind;
            }

            public void Rebind(KeyBind bind)
            {
                this.bind = bind;
                if (action != null)
                    action.currentKeybind = bind;
            }
        }

        public enum NewInputAxis { x, y, z }

        [System.Serializable]
        public struct InputAxis
        {
            public string name;
            public InputBinding positive;
            public InputBinding negative;
            public bool weakModifierKeys;
            public bool hasControllerBinding;
            public bool neverDisable;
            public InputActionReference controllerBind;
            public NewInputAxis controllerBindUseAxis;

            public InputAxis(string name, InputBinding positive, InputBinding negative, bool weakModifierKeys, bool hasControllerBinding, bool neverDisable, InputActionReference controllerBind, NewInputAxis controllerBindUseAxis)
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
                this.weakModifierKeys = weakModifierKeys;
                this.hasControllerBinding = hasControllerBinding;
                this.neverDisable = neverDisable;
                this.controllerBind = controllerBind;
                this.controllerBindUseAxis = controllerBindUseAxis;
            }

            public void Rebind(KeyBind bindPositive, KeyBind bindNegative)
            {
                positive.Rebind(bindPositive);
                negative.Rebind(bindNegative);
            }
        }
    }

}