// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace dev.susybaka.Shared.UserInterface
{
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("UI/Custom/Tab Button")]
    public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public TabGroup tabGroup;

        [HideInInspector]
        public Image background;

        public UnityEvent onTabSelected;
        public UnityEvent onTabDeselected;

        public void OnPointerClick(PointerEventData eventData)
        {
            tabGroup.OnTabSelected(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tabGroup.OnTabEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tabGroup.OnTabExit(this);
        }

        void Start()
        {
            if (tabGroup == null)
                tabGroup = GetComponentInParent<TabGroup>();

            background = GetComponent<Image>();
            tabGroup.Subscribe(this);
            background.color = tabGroup.tabIdle;
        }

        public void Select()
        {
            if (onTabSelected != null)
            {
                onTabSelected.Invoke();
            }
        }

        public void Deselect()
        {
            if (onTabDeselected != null)
            {
                onTabDeselected.Invoke();
            }
        }
    }
}