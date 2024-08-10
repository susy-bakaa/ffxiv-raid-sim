using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static PartyList;

public class TetherTrigger : MonoBehaviour
{
    LineRenderer lineRenderer;
    public enum TetherType { nearest, furthest }

    public PartyList partyList;
    public TetherType tetherType = TetherType.nearest;
    public Transform startPoint;
    public Vector3 startOffset;
    public Transform endPoint;
    public Vector3 endOffset;
    public float maxDistance;
    public float breakDelay = 0.5f;
    public bool initializeOnStart;
    public bool worldSpace = true;

    public UnityEvent<CharacterState> onForm;
    public UnityEvent onBreak;
    public UnityEvent onSolved;

    private bool initialized;
    private CharacterState startCharacter;
    private CharacterState endCharacter;

    void Awake()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();
        lineRenderer.gameObject.SetActive(false);
        if (partyList == null)
        {
            partyList = FightTimeline.Instance.partyList;
        }
    }

    void Start()
    {
        if (initializeOnStart)
        {
            Initialize();
        }
    }

    void Update()
    {
        if (startPoint == null || endPoint == null) 
            return;

        if (lineRenderer != null)
        {
            if (worldSpace)
            {
                lineRenderer.SetPositions(new Vector3[2] { startPoint.position + startOffset, endPoint.position + endOffset });
            }
            else
            {
                lineRenderer.SetPositions(new Vector3[2] { startPoint.localPosition + startOffset, endPoint.localPosition + endOffset });
            }
        }

        if (maxDistance > 0f)
        {
            if (worldSpace)
            {
                if (Vector3.Distance(startPoint.position, endPoint.position) > maxDistance)
                {
                    BreakTether();
                }
            }
            else
            {
                if (Vector3.Distance(startPoint.localPosition, endPoint.localPosition) > maxDistance)
                {
                    BreakTether();
                }
            }
        }
    }

    public void Initialize()
    {
        if (!initialized)
        {
            FormTether();
            initialized = true;
        }
    }

    public void FormTether()
    {
        switch (tetherType)
        {
            default:
            {
                CharacterState closestMember = null;
                float closestDistance = float.MaxValue;

                foreach (PartyMember member in partyList.members)
                {
                    float distance = Vector3.Distance(transform.position, member.characterState.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestMember = member.characterState;
                    }
                }

                // Assuming a certain threshold for tethering, e.g., 1 unit
                //float tetherThreshold = 1.0f;
                //if (closestMember != null && closestDistance < tetherThreshold)
                //{
                //    FormTether(closestMember);
                //}
                FormTether(closestMember);
                break;
            }
            case TetherType.furthest:
            {
                CharacterState furthestMember = null;
                float furthestDistance = 0f;

                foreach (PartyMember member in partyList.members)
                {
                    float distance = Vector3.Distance(transform.position, member.characterState.transform.position);
                    if (distance > furthestDistance)
                    {
                        furthestDistance = distance;
                        furthestMember = member.characterState;
                    }
                }

                // Assuming a certain threshold for tethering, e.g., 1 unit
                //float tetherThreshold = 1.0f;
                //if (furthestMember != null && furthestDistance > tetherThreshold)
                //{
                //    FormTether(furthestMember);
                //}
                FormTether(furthestMember);
                break;
            }
        }
    }

    public void FormTether(CharacterState target)
    {
        FormTether(startPoint, target.transform.GetChild(target.transform.childCount - 1).transform);
    }

    public void FormTether(Transform start, Transform end)
    {
        lineRenderer.gameObject.SetActive(true);
        startPoint = start;
        endPoint = end;

        if (end.parent.TryGetComponent(out CharacterState endState))
        {
            endCharacter = endState;
            onForm.Invoke(endState);
        }
        else if (start.parent.TryGetComponent(out CharacterState startState))
        {
            startCharacter = startState;
            onForm.Invoke(startState);
        }
        else
        {
            endCharacter = null;
            startCharacter = null;
            onForm.Invoke(null);
        }
    }

    public void BreakTether()
    {
        lineRenderer.gameObject.SetActive(false);
        Utilities.FunctionTimer.Create(() => onBreak.Invoke(), breakDelay, $"TetherTrigger_{this}_{GetHashCode()}_Break_Delay", false, true);
    }

    public void SolveTether()
    {
        lineRenderer.gameObject.SetActive(false);
        Utilities.FunctionTimer.Create(() => onSolved.Invoke(), breakDelay, $"TetherTrigger_{this}_{GetHashCode()}_Solve_Delay", false, true);
    }
}