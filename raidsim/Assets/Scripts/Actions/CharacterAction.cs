using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ActionController;

public class CharacterAction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    Button button;

    [Header("Info")]
    public CharacterActionData data;
    private float timer;
    public bool isAvailable { private set; get; }
    public bool isDisabled;

    [Header("Events")]
    public UnityEvent<ActionInfo> onExecute;

    [Header("Visuals")]
    public Image recastFill;
    public CanvasGroup recastFillGroup;
    public CanvasGroup selectionBorder;
    public CanvasGroup clickHighlight;
    public TextMeshProUGUI recastTimeText;

    private bool pointer;

    void Awake()
    {
        button = GetComponent<Button>();

        recastFillGroup.alpha = 0f;
        selectionBorder.alpha = 0f;
        clickHighlight.alpha = 0f;
    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            isAvailable = false;
            recastFillGroup.alpha = 1f;
            if (recastTimeText != null && data.recast > 2.5f)
            {
                recastTimeText.text = timer.ToString("F0");
            }
        }
        else
        {
            timer = 0f;
            isAvailable = true;
            recastFillGroup.alpha = 0f;
            if (recastTimeText != null)
            {
                recastTimeText.text = "";
            }
        }

        recastFill.fillAmount = Utilities.Map(data.recast - timer, 0f, data.recast, 0f, 1f);

        if (!isDisabled)
        {
            button.interactable = isAvailable;
        }
        else
        {
            button.interactable = false;
        }
    }

    public void Initialize(ActionController controller)
    {
        if (button != null)
        {
            button.onClick.AddListener(() => { controller.PerformAction(this); });
        }
    }

    public void ExecuteAction(ActionInfo action)
    {
        if (data.buff != null)
            action.source.AddEffect(data.buff);

        onExecute.Invoke(action);
    }

    public void ActivateCooldown()
    {
        isAvailable = false;
        timer = data.recast;
    }

    public void ResetCooldown()
    {
        isAvailable = true;
        timer = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointer = true;
        selectionBorder.LeanAlpha(1f, 0.25f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointer = false;
        selectionBorder.LeanAlpha(0f, 0.25f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData == null && pointer)
        {
            return;
        }

        clickHighlight.transform.localScale = Vector3.zero;
        clickHighlight.transform.LeanScale(Vector3.one, 0.5f).setOnComplete(() => { clickHighlight.LeanAlpha(0f, 0.25f); });
        clickHighlight.LeanAlpha(1f, 0.25f);
    }
}
