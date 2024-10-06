using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace susy_baka.raidsim.UserInterface
{
    public class DraggableWindowScript : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform widgetTransform;
        [SerializeField] private RectTransform targetTransform;
        [SerializeField] private UnityEvent<Vector2> onOwnPositionAltered;
        [SerializeField] private UnityEvent<Vector2> onTargetPositionAltered;
        //[SerializeField] private UnityEvent<Vector2> onPositionAltered;

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (targetTransform != null)
                targetTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

            widgetTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            //onPositionAltered.Invoke(widgetTransform.anchoredPosition);
            onOwnPositionAltered.Invoke(widgetTransform.anchoredPosition);
            if (targetTransform != null)
                onTargetPositionAltered.Invoke(targetTransform.anchoredPosition);

            //Debug.Log($"OnPointerUp {transform.parent.gameObject.name}");
        }
    }
}