using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObjects : MonoBehaviour
{
    public List<string> objectNames = new List<string>();

    public void TriggerDestruction()
    {
        for (int i = 0; i < objectNames.Count; i++)
        {
            Destroy(GameObject.Find(objectNames[i]));
        }
    }
}
