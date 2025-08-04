using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace dev.susybaka.raidsim.UI
{
    public class HudElementColor : MonoBehaviour
    {
        public enum ColorType { main, highlight, background }

        public ColorType colorType;
        public Color currentColor;
        public Image image;
        public TextMeshProUGUI text;
        public Outline outline;

        private void Awake()
        {
            if (image != null)
                currentColor = image.color;
            if (text != null)
                currentColor = text.color;
            if (outline != null)
                currentColor = outline.effectColor;
        }

        public void SetColor(List<Color> colors)
        {
            Color newColor = colors[(int)colorType];

            currentColor = newColor;

            if (image != null)
                image.color = newColor;
            if (text != null)
                text.color = newColor;
            if (outline != null)
                outline.effectColor = newColor;
        }
    }
}