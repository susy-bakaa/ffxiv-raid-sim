using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class BotNode : MonoBehaviour
{
    public string nodeName;
    [Tag] public string targetTag;

    void Awake()
    {
        nodeName = gameObject.name;    
    }

    public void UpdateNode()
    {
        if (!string.IsNullOrEmpty(targetTag))
        {
            Transform target = GameObject.FindGameObjectWithTag(targetTag).transform;
            if (target != null)
            {
                transform.position = target.position;
            }
        }
    }
}
