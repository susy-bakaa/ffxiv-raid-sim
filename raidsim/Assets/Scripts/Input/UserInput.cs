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

    void Awake()
    {
        if (keys.Count > 0 && KeyBind.Keys == null)
        {
            KeyBind.Keys = new Dictionary<string, KeyBind>();

            for (int i = 0; i < keys.Count; i++)
            {
                KeyBind.Keys.Add(keys[i].name, keys[i].bind);
            }
        }
    }

    void Update()
    {
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

        targetController.canMouseRaycast = targetRaycastInputEnabled;

        for (int i = 0;i < keys.Count; i++)
        {
            //Debug.Log($"KeyBind {keys[i].bind} state {BindedKey(keys[i].bind)}");
            if (keys[i].bind != null && BindedKey(keys[i].bind))
            {
                if (keys[i].action != null && characterAction != null)
                    characterAction.PerformAction(keys[i].action);
                if (keys[i].statusEffect != null && characterState != null)
                    characterState.AddEffect(keys[i].statusEffect);
                keys[i].onInput.Invoke();
            }
        }

        if (BindedKey(KeyBind.Keys["ResetKey"]) || BindedKey(KeyBind.Keys["ResetKeyAlt"]))
        {
            SceneManager.LoadScene("menu");
        }
    }

    public bool BindedKey(KeyBind keyBind)
    {
        return (keyBind.keyCode != KeyCode.None && (Input.GetKeyDown(keyBind.keyCode) & Input.GetKey(KeyCode.LeftShift) == keyBind.shift & Input.GetKey(KeyCode.LeftAlt) == keyBind.alt & Input.GetKey(KeyCode.LeftControl) == keyBind.control)) || (keyBind.mouseButton != -1 && (Input.GetMouseButton(keyBind.mouseButton) & Input.GetKey(KeyCode.LeftShift) == keyBind.shift & Input.GetKey(KeyCode.LeftAlt) == keyBind.alt & Input.GetKey(KeyCode.LeftControl) == keyBind.control));
    }

    [System.Serializable]
    public struct InputBinding 
    {
        public string name;
        public KeyBind bind;
        public CharacterAction action;
        public StatusEffectData statusEffect;
        public UnityEvent onInput;

        public InputBinding(string name, KeyBind bind, CharacterAction action, StatusEffectData statusEffect, UnityEvent onInput)
        {
            this.name = name;
            this.bind = bind;
            this.action = action;
            this.statusEffect = statusEffect;
            this.onInput = onInput;
        }
    }
}
