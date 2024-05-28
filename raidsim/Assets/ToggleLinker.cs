using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleLinker : MonoBehaviour
{
    public List<Toggle> toggles = new List<Toggle>();

    public void UpdateToggles(Toggle toggle)
    {
        foreach (Toggle t in toggles)
        {
            if (t != toggle)
            {
                t.SetIsOnWithoutNotify(false);
            }
            else
            {
                t.SetIsOnWithoutNotify(true);
            }
        }
    }
}
