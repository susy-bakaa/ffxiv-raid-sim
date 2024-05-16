using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotNode : MonoBehaviour
{
    public string nodeName;

    void Awake()
    {
        nodeName = gameObject.name;    
    }
}
