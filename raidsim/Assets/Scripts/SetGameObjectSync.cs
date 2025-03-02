using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetGameObjectSync : MonoBehaviour
{
    [SerializeField] private string replacedPart = string.Empty;
    private GameObjectSync[] syncs;
    private bool done = false;

    private void OnDisable()
    {
        if (done)
            return;

        syncs = GetComponentsInChildren<GameObjectSync>(true);

        if (syncs != null && syncs.Length > 0 && !string.IsNullOrEmpty(replacedPart))
        {
            for (int i = 0; i < syncs.Length; i++)
            {
                syncs[i].targetPath = syncs[i].targetPath.Replace("%i", replacedPart);
                syncs[i].Setup();
            }
            done = true;
        }
    }
}
