using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace dev.susybaka.raidsim.UI
{
    public class HudElementGroup : MonoBehaviour
    {
        public List<HudElement> elements = new List<HudElement>();
        public UnityEvent<PointerEventData> onPointerEnter;
        public UnityEvent<PointerEventData> onPointerExit;
        public UnityEvent<Button> onClick;

        private void Awake()
        {
            if (elements != null && elements.Count > 0)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    elements[i].onPointerEnter.AddListener(OnPointerEnter);
                    elements[i].onPointerExit.AddListener(OnPointerExit);

                    if (elements[i].TryGetComponent(out Button button))
                    {
                        button.onClick.AddListener(() => OnClick(button));
                    }
                }
            }
        }

        private void OnPointerEnter(PointerEventData data)
        {
            onPointerEnter.Invoke(data);
        }

        private void OnPointerExit(PointerEventData data)
        {
            onPointerExit.Invoke(data);
        }

        private void OnClick(Button button)
        {
            onClick.Invoke(button);
        }
    }
}