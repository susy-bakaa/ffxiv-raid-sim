using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TweenObject : MonoBehaviour
{
    GameObject m_object;

    public LeanTweenType tweenType = LeanTweenType.linear;
    public float tweenDuration;
    public bool onStart;
    public Vector3 tweenScale = Vector3.zero;

    public UnityEvent onFinish;

    void Awake()
    {
        m_object = gameObject;
    }

    void Start()
    {
        if (onStart)
        {
            if (tweenScale != Vector3.zero)
            {                
                LeanTween.scale(m_object, tweenScale, tweenDuration).setEase(tweenType).setOnComplete(() => onFinish.Invoke());
            }
        }
    }

    void Update()
    {
        
    }

    public void TriggerTween(float duration = -1)
    {
        if (duration < 0)
        {
            duration = tweenDuration;
        }

        if (tweenScale != Vector3.zero)
        {
            LeanTween.scale(m_object, tweenScale, duration).setEase(tweenType).setOnComplete(() => onFinish.Invoke());
        }
    }
}
