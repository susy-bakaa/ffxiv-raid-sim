using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace dev.susybaka.raidsim.UI
{
    public sealed class ResizableWindow : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private Vector2 minSize = new(420, 220);
        [SerializeField] private Vector2 maxSize = new(1400, 900);

        [Tooltip("If your panel is anchored bottom-left and grows up/right, leave this at (1, 1).")]
        [SerializeField] private Vector2 dragSign = new(1f, 1f);

        [Tooltip("Optional: force layout rebuild after resizing (good if you see stale wrapping).")]
        [SerializeField] private bool forceLayoutRebuild = true;

        public UnityEvent<Vector2> onResized;

        private Vector2 _startSize;
        private Vector2 _startPointerLocal;
        private HudElement hudElement;

        private void Awake()
        {
            hudElement = GetComponent<HudElement>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (target == null)
                return;

            hudElement?.OnSelect();

            _startSize = target.sizeDelta;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                target, eventData.position, eventData.pressEventCamera, out _startPointerLocal);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (target == null)
                return;

            hudElement?.OnDeselect();

            onResized?.Invoke(target.sizeDelta);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (target == null)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                target, eventData.position, eventData.pressEventCamera, out var localNow);

            var delta = localNow - _startPointerLocal;
            var newSize = _startSize + new Vector2(delta.x * dragSign.x, delta.y * dragSign.y);

            newSize.x = Mathf.Clamp(newSize.x, minSize.x, maxSize.x);
            newSize.y = Mathf.Clamp(newSize.y, minSize.y, maxSize.y);

            target.sizeDelta = newSize;

            if (forceLayoutRebuild)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(target);
            }
        }
    }
}