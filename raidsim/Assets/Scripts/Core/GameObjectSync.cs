using UnityEngine;

public class GameObjectSync : MonoBehaviour
{
    public GameObject target;
    public bool beginActive = false;

    private void Start()
    {
        if (beginActive)
        {
            target.SetActive(true);
        }
        else
        {
            target.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (target != null)
        {
            target.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (target != null)
        {
            target.SetActive(false);
        }
    }
}
