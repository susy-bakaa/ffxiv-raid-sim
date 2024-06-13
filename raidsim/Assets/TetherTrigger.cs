using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    public UnityEvent onForm;
    public UnityEvent onBreak;

    void Awake()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();
        lineRenderer.gameObject.SetActive(false);
    }

    void Update()
    {
        if (startPoint == null || endPoint == null) 
            return;

        if (lineRenderer != null)
        {
            lineRenderer.SetPositions(new Vector3[2] { startPoint.position + startOffset, endPoint.position + endOffset });
        }

        if (maxDistance > 0f)
        {
            if (Vector3.Distance(startPoint.position, endPoint.position) > maxDistance)
            {
                BreakTether();
            }
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

                foreach (CharacterState member in partyList.members)
                {
                    float distance = Vector3.Distance(transform.position, member.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestMember = member;
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

                foreach (CharacterState member in partyList.members)
                {
                    float distance = Vector3.Distance(transform.position, member.transform.position);
                    if (distance > furthestDistance)
                    {
                        furthestDistance = distance;
                        furthestMember = member;
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
        FormTether(startPoint, target.transform);
    }

    public void FormTether(Transform start, Transform end)
    {
        lineRenderer.gameObject.SetActive(true);
        startPoint = start;
        endPoint = end;
        onForm.Invoke();
    }

    public void BreakTether()
    {
        lineRenderer.gameObject.SetActive(false);
        onBreak.Invoke();
    }
}