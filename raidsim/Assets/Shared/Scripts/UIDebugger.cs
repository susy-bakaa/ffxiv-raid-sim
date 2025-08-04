using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace dev.susybaka.Shared.Editor
{
    public class UIDebugger : MonoBehaviour
    {
        public EventSystem eventSystem;
        public GraphicRaycaster raycaster;

        private PointerEventData pointerEventData;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                pointerEventData = new PointerEventData(eventSystem)
                {
                    position = Input.mousePosition
                };

                List<RaycastResult> results = new List<RaycastResult>();
                raycaster.Raycast(pointerEventData, results);

                if (results.Count == 0)
                {
                    Debug.Log("No UI elements under the mouse.");
                    return;
                }

                Debug.Log($"UI elements under mouse at {Input.mousePosition}:");

                foreach (RaycastResult result in results)
                {
                    Debug.Log($"- {result.gameObject.name} (sortingOrder: {result.sortingOrder}, depth: {result.depth})", result.gameObject);
                }
            }
        }
    }
}