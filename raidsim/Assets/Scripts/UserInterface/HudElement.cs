using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using NaughtyAttributes;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudElement : MonoBehaviour
{
    public int priority = 1;

    public List<Image> images = new List<Image>();
    public List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
    public List<Outline> outlines = new List<Outline>();
    public List<Color> defaultColors = new List<Color>();
    public List<Color> alternativeColors = new List<Color>();

#if UNITY_EDITOR
    private bool currentColor = false;

    [Button("Swap Color")]
    private void SwapColor()
    {
        currentColor = !currentColor;
        ChangeColors(currentColor);
    }
#endif

    public void ChangeColors(bool alt)
    {
        if (alt)
        {
            for (int i = 0; i < (images.Count + texts.Count); i++)
            {
                if (i < images.Count)
                {
                    images[i].color = alternativeColors[i];
                }
                else if (i >= images.Count && i < (images.Count + texts.Count))
                {
                    texts[i - images.Count].color = alternativeColors[i];
                }
                else if (i >= (images.Count + texts.Count))
                {
                    outlines[i - (images.Count + texts.Count)].effectColor = alternativeColors[i];
                }

            }
        }
        else
        {
            for (int i = 0; i < (images.Count + texts.Count); i++)
            {
                if (i < images.Count)
                {
                    images[i].color = defaultColors[i];
                }
                else if (i >= images.Count && i < (images.Count + texts.Count))
                {
                    texts[i - images.Count].color = defaultColors[i];
                }
                else if (i >= (images.Count + texts.Count))
                {
                    outlines[i - (images.Count + texts.Count)].effectColor = defaultColors[i];
                }
            }
        }
    }
}
