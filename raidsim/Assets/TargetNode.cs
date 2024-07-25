using UnityEngine;
using UnityEngine.Events;

public class TargetNode : MonoBehaviour
{
    [SerializeField] private CharacterState characterState;
    [SerializeField] private ActionController actionController;
    [SerializeField] private TargetController targetController;
    [SerializeField] private bool allowAutomaticSetup = true;
    [SerializeField] private bool targetable;
    public bool Targetable { get { return characterState != null ? !characterState.untargetable : targetable; } }
    [SerializeField] private int group;
    public int Group { get { return group; } }

    public UnityEvent onTarget;
    public UnityEvent onDetarget;

    void Awake()
    {
        SetupCharacterState();
        SetupActionController();
        SetupTargetController();
    }

    void Update()
    {
        if (characterState != null)
        {
            targetable = !characterState.untargetable;
        }
    }

    public bool TryGetCharacterState(out CharacterState characterState)
    {
        if (this.characterState == null)
        {
            SetupCharacterState();
        }

        characterState = this.characterState;

        return characterState != null;
    }

    public bool TryGetActionController(out ActionController actionController)
    {
        if (this.actionController == null)
        {
            SetupActionController();
        }

        actionController = this.actionController;

        return actionController != null;
    }

    public bool TryGetTargetController(out TargetController targetController)
    {
        if (this.targetController == null)
        {
            SetupTargetController();
        }

        targetController = this.targetController;

        return targetController != null;
    }

    public CharacterState GetCharacterState()
    {
        return characterState ?? SetupCharacterState();
    }

    public ActionController GetActionController()
    {
        return actionController ?? SetupActionController();
    }

    public TargetController GetTargetController()
    {
        return targetController ?? SetupTargetController();
    }

    private CharacterState SetupCharacterState()
    {
        if (!allowAutomaticSetup)
            return characterState;

        if (characterState == null)
        {
            if (TryGetComponent(out CharacterState resultSelf))
            {
                characterState = resultSelf;
            }
            else if (transform.parent.TryGetComponent(out CharacterState resultParent))
            {
                characterState = resultParent;
            }
            else if (transform.root.TryGetComponent(out CharacterState resultRoot))
            {
                characterState = resultRoot;
            }
        }

        return characterState;
    }

    private ActionController SetupActionController()
    {
        if (!allowAutomaticSetup)
            return actionController;

        if (actionController == null)
        {
            if (TryGetComponent(out ActionController resultSelf))
            {
                actionController = resultSelf;
            }
            else if (transform.parent.TryGetComponent(out ActionController resultParent))
            {
                actionController = resultParent;
            }
            else if (transform.root.TryGetComponent(out ActionController resultRoot))
            {
                actionController = resultRoot;
            }
        }

        return actionController;
    }

    private TargetController SetupTargetController()
    {
        if (!allowAutomaticSetup)
            return targetController;

        if (actionController == null)
        {
            if (TryGetComponent(out TargetController resultSelf))
            {
                targetController = resultSelf;
            }
            else if (transform.parent.TryGetComponent(out TargetController resultParent))
            {
                targetController = resultParent;
            }
            else if (transform.root.TryGetComponent(out TargetController resultRoot))
            {
                targetController = resultRoot;
            }
        }

        return targetController;
    }
}