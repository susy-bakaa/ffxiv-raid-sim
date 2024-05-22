using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using NaughtyAttributes;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HudElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UserInput input;

    public int priority = 1;
    public bool blocksAllInput = false;
    public bool blocksPosInput = false;
    public bool blocksRotInput = false;
    public bool blocksScrInput = false;

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (input == null)
            return;

        if (blocksAllInput)
            input.inputEnabled = false;
        if (blocksPosInput)
            input.movementInputEnabled = false;
        if (blocksRotInput)
            input.rotationInputEnabled = false;
        if (blocksScrInput)
            input.zoomInputEnabled = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (input == null)
            return;

        if (blocksAllInput)
            input.inputEnabled = true;
        if (blocksPosInput)
            input.movementInputEnabled = true;
        if (blocksRotInput)
            input.rotationInputEnabled = true;
        if (blocksScrInput)
            input.zoomInputEnabled = true;
    }
}
