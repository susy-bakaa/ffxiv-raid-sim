using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using NaughtyAttributes;
#endif

namespace dev.susybaka.raidsim.Characters
{
    public class SignMarker : MonoBehaviour
    {
        private CanvasGroup parentGroup;
        private CanvasGroup group;
        private Image image;

        public List<Sprite> sprites = new List<Sprite>();
        public int index = 0;

#if UNITY_EDITOR
        [Button("Assign Marker")]
        public void ShowMarkerButton()
        {
            AssignMarker(index);
        }
#endif

        private void Awake()
        {
            parentGroup = transform.parent.GetComponent<CanvasGroup>();
            group = GetComponent<CanvasGroup>();
            image = GetComponentInChildren<Image>();
            ResetMarker();
        }

        public void AssignMarker(int index)
        {
            if (parentGroup.alpha < 1f)
                return;

            if (index > -1 && index < sprites.Count)
            {
                this.index = index;
                image.sprite = sprites[this.index];
                group.alpha = 1f;
            }
            else
                ResetMarker();
        }

        public void ResetMarker()
        {
            group.alpha = 0f;
        }
    }
}