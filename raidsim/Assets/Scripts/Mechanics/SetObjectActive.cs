using UnityEngine;

public class SetObjectActive : MonoBehaviour
{
    public GameObject objectToSet;
    public bool setChildrenToo;

    public void SetActiveTo(bool active)
    {
        SetObjectActiveTo(objectToSet, active);
    }

    private void SetObjectActiveTo(GameObject obj, bool active)
    {
        obj.SetActive(active);

        if (setChildrenToo)
        {
            foreach (Transform child in obj.transform)
            {
                SetObjectActiveTo(child.gameObject, active);
            }
        }
    }
}
