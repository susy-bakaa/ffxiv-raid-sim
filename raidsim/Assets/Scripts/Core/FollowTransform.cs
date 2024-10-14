using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    RectTransform rectTransform;

    public Transform target;
    public RectTransform targetUI;
    public bool anchoredPosition;
    public bool x;
    public bool y;
    public bool z;
    public float speed = -1f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (targetUI != null && rectTransform != null)
        {
            if (anchoredPosition)
            {
                if (speed >= 0)
                {
                    Vector2 newPos = rectTransform.anchoredPosition;
                    if (x)
                        newPos.x = Mathf.MoveTowards(rectTransform.anchoredPosition.x, targetUI.anchoredPosition.x, speed * FightTimeline.deltaTime);
                    if (y)
                        newPos.y = Mathf.MoveTowards(rectTransform.anchoredPosition.y, targetUI.anchoredPosition.y, speed * FightTimeline.deltaTime);
                    rectTransform.anchoredPosition = newPos;
                }
                else
                {
                    if (x && y)
                        rectTransform.anchoredPosition = targetUI.anchoredPosition;
                    else
                    {
                        if (x)
                            rectTransform.anchoredPosition = new Vector2(targetUI.anchoredPosition.x, rectTransform.anchoredPosition.y);
                        if (y)
                            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, targetUI.anchoredPosition.y);
                    }
                }
            }
            else
            {
                if (speed >= 0)
                {
                    Vector3 newPos = rectTransform.localPosition;
                    if (x)
                        newPos.x = Mathf.MoveTowards(rectTransform.localPosition.x, targetUI.localPosition.x, speed * FightTimeline.deltaTime);
                    if (y)
                        newPos.y = Mathf.MoveTowards(rectTransform.localPosition.y, targetUI.localPosition.y, speed * FightTimeline.deltaTime);
                    rectTransform.localPosition = newPos;
                }
                else
                {
                    if (x && y)
                        rectTransform.localPosition = targetUI.localPosition;
                    else
                    {
                        if (x)
                            rectTransform.localPosition = new Vector2(targetUI.localPosition.x, rectTransform.localPosition.y);
                        if (y)
                            rectTransform.localPosition = new Vector2(rectTransform.localPosition.x, targetUI.localPosition.y);
                    }
                }
            }
            return;
        }
        else if (targetUI == null && target == null)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        if (speed >= 0)
        {
            Vector3 newPos = transform.position;
            if (x)
                newPos.x = Mathf.MoveTowards(transform.position.x, target.position.x, speed * FightTimeline.deltaTime);
            if (y)
                newPos.y = Mathf.MoveTowards(transform.position.y, target.position.y, speed * FightTimeline.deltaTime);
            if (z)
                newPos.z = Mathf.MoveTowards(transform.position.z, target.position.z, speed * FightTimeline.deltaTime);
            transform.position = newPos;
        }
        else
        {
            if (x && y && z)
            {
                transform.position = target.position;
            }
            else
            {
                if (x)
                {
                    transform.position = new Vector3(target.position.x, transform.position.y, transform.position.z);
                }
                if (y)
                {
                    transform.position = new Vector3(transform.position.x, target.position.y, transform.position.z);
                }
                if (z)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y, target.position.z);
                }
            }
        }
    }
}
