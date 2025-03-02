using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShiftToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject target;
    public bool toggle = false;

    private bool poinerOver = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        poinerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        poinerOver = false;
    }

    void Update()
    {
        if (poinerOver)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (target.activeSelf != toggle)
                    target.SetActive(toggle);
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
            {
                target.SetActive(!toggle);
            }
        }
        else
        {
            target.SetActive(!toggle);
        }
    }
}
