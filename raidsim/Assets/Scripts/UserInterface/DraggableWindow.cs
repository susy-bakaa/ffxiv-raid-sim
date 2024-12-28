using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace susy_baka.raidsim.UserInterface
{
    public class DraggableWindowScript : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        [Hidden] public SavePosition targetSave;

        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform widgetTransform;
        [SerializeField] private RectTransform targetTransform;
        [SerializeField] private UnityEvent<Vector2> onOwnPositionAltered;
        [SerializeField] private UnityEvent<Vector2> onTargetPositionAltered;
        //[SerializeField] private UnityEvent<Vector2> onPositionAltered;

        private Vector2 widgetTransformDefaultPosition;
        private Vector2 targetTransformDefaultPosition;

        void Awake()
        {
            widgetTransformDefaultPosition = widgetTransform.anchoredPosition;
            targetTransformDefaultPosition = targetTransform.anchoredPosition;

            if (targetTransform != null)
            {
                targetSave = targetTransform.GetComponentInChildren<SavePosition>();
            }
        }

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

        public void ResetPosition()
        {
            widgetTransform.anchoredPosition = widgetTransformDefaultPosition;
            targetTransform.anchoredPosition = targetTransformDefaultPosition;
            onOwnPositionAltered.Invoke(widgetTransform.anchoredPosition);
            onTargetPositionAltered.Invoke(targetTransform.anchoredPosition);
        }
    }
}