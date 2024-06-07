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
    RectTransform rectTransform;
    CanvasGroup canvasGroup;

    [Header("Sorting")]
    public int priority = 1;
    [Header("Input")]
    public UserInput input;
    public bool blocksAllInput = false;
    public bool blocksPosInput = false;
    public bool blocksRotInput = false;
    public bool blocksScrInput = false;
    [Header("Animation")]
    public bool doMovement = false;
    public Vector2 movement = Vector2.zero;
    public float fadeOutTime = -1f;
    public float fadeOutDelay = 0f;
    private bool fadeOutTimeSetupDone;
    public float lifeTime = -1f;
    private bool lifeTimeSetupDone;
    [Header("Visual")]
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

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (input == null)
        {
            if (blocksAllInput || blocksPosInput || blocksRotInput || blocksScrInput)
            {
                input = FindObjectOfType<UserInput>();
            }
        }
    }

    void Update()
    {
        if (lifeTime > 0f && !lifeTimeSetupDone)
        {
            float t = lifeTime;
            if (fadeOutTime > 0f)
            {
                t = fadeOutTime + fadeOutDelay + 1f;
            }
            Destroy(gameObject, t);
            lifeTimeSetupDone = true;
        }

        if (fadeOutTime > 0f && !fadeOutTimeSetupDone)
        {
            lifeTime = -1f;
            if (fadeOutDelay > 0f)
            {
                Utilities.FunctionTimer.Create(() => canvasGroup.LeanAlpha(0f, fadeOutTime), fadeOutDelay, $"{gameObject}_{gameObject.GetHashCode()}_HudElement_FadeOutDelay", false, true);
            }
            else
            {
                canvasGroup.LeanAlpha(0f, fadeOutTime);
            }
            fadeOutTimeSetupDone = true;
        }

        if (doMovement && movement != Vector2.zero)
        {
            rectTransform.anchoredPosition += movement * Time.deltaTime;
        }
    }

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

    public void OnPointerExit()
    {
        OnPointerExit(null);
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
