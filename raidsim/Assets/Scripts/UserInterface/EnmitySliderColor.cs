using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class EnmitySliderColor : MonoBehaviour
{
    private Slider slider;

    [SerializeField] private HudElementGroup hudElementGroup;
    public bool useAlternativeColors = false;
    [SerializeField] private ColorPair defaultColors;
    [SerializeField] private ColorPair alternativeColors;
    [SerializeField] private ColorPair maxColors;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Update()
    {
        if (slider.value >= slider.maxValue)
        {
            if (hudElementGroup.background != null)
                hudElementGroup.background.color = maxColors.color1;
            if (hudElementGroup.backgroundOutline != null)
                hudElementGroup.backgroundOutline.effectColor = maxColors.color1;
            if (hudElementGroup.fill != null)
                hudElementGroup.fill.color = maxColors.color2;
            if (hudElementGroup.fillOutline != null)
                hudElementGroup.fillOutline.effectColor = maxColors.color2;
            if (hudElementGroup.label != null)
                hudElementGroup.label.color = maxColors.color2;
            return;
        }
        if (!useAlternativeColors)
        {
            if (hudElementGroup.background != null)
                hudElementGroup.background.color = defaultColors.color1;
            if (hudElementGroup.backgroundOutline != null)
                hudElementGroup.backgroundOutline.effectColor = defaultColors.color1;
            if (hudElementGroup.fill != null)
                hudElementGroup.fill.color = defaultColors.color2;
            if (hudElementGroup.fillOutline != null)
                hudElementGroup.fillOutline.effectColor = defaultColors.color2;
            if (hudElementGroup.label != null)
                hudElementGroup.label.color = defaultColors.color2;
        }
        else
        {
            if (hudElementGroup.background != null)
                hudElementGroup.background.color = alternativeColors.color1;
            if (hudElementGroup.backgroundOutline != null)
                hudElementGroup.backgroundOutline.effectColor = alternativeColors.color1;
            if (hudElementGroup.fill != null)
                hudElementGroup.fill.color = alternativeColors.color2;
            if (hudElementGroup.fillOutline != null)
                hudElementGroup.fillOutline.effectColor = alternativeColors.color2;
            if (hudElementGroup.label != null)
                hudElementGroup.label.color = alternativeColors.color2;
        }
    }

    [System.Serializable]
    public struct ColorPair
    {
        public Color color1;
        public Color color2;

        public ColorPair(Color color1, Color color2)
        {
            this.color1 = color1;
            this.color2 = color2;
        }
    }

    [System.Serializable]
    public struct HudElementGroup
    {
        public Image background;
        public Outline backgroundOutline;
        public Image fill;
        public Outline fillOutline;
        public TextMeshProUGUI label;

        public HudElementGroup(Image background, Outline backgroundOutline, Image fill, Outline fillOutline, TextMeshProUGUI label)
        {
            this.background = background;
            this.backgroundOutline = backgroundOutline;
            this.fill = fill;
            this.fillOutline = fillOutline;
            this.label = label;
        }
    }
}
