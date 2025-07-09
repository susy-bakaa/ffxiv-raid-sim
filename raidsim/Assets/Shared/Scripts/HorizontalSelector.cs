using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

namespace dev.susybaka.Shared.UserInterface
{
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("UI/Custom/Horizontal Selector")]
    public class HorizontalSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Technical")]
        [SerializeField] private List<string> data = new List<string>();
        [HideInInspector] public List<string> Data { get { return data; } }
        private Image background = null;
        private TextMeshProUGUI label = null;
        [HideInInspector] public TextMeshProUGUI Label { get { return label; } }
        private Button right = null;
        private Button left = null;

        public int index = 0;
        [HideInInspector] public int maxIndex = 0;
        public bool lockLabel { get; private set; }

        public UnityEvent<int> OnValueChanged;

        [Header("Visuals")]
        public bool overrideControl = false;
        public Color colorIdle;
        public Color colorHover;
        public Color ColorSelected;
        public Image overrideGraphic;

        public void SetData(List<string> data)
        {
            if (data != null)
                this.data = data;
            if (!lockLabel)
                ClearLabel();
        }
        public void ClearData()
        {
            data = new List<string>();
        }
        public void SetLabel(string label)
        {
            if (label != null)
            {
                this.label.text = label;
                lockLabel = true;
            }
        }
        public void ClearLabel()
        {
            if (data.Count > 0 && label != null)
            {
                label.text = data[index];
            }
            lockLabel = false;
        }

        void Start()
        {
            if (overrideGraphic == null)
                background = GetComponent<Image>();
            else
                background = overrideGraphic;

            background.color = colorIdle;
            label = transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
            right = transform.GetChild(1).GetComponentInChildren<Button>();
            if (transform.GetChild(2) != null)
                left = transform.GetChild(2).GetComponentInChildren<Button>();

            right.onClick.AddListener(OnRightClick);
            left.onClick.AddListener(OnLeftClick);
            right.onClick.AddListener(OnPointerSelect);
            left.onClick.AddListener(OnPointerSelect);

            maxIndex = data.Count - 1;

            if (!lockLabel)
                label.text = data[index];
        }

        void OnRightClick()
        {
            if ((index + 1) >= data.Count)
            {
                index = 0;
            }
            else
            {
                index++;
            }

            if (!lockLabel)
                label.text = data[index];
            OnValueChanged.Invoke(index);
        }

        void OnLeftClick()
        {
            if (index == 0)
            {
                index = data.Count - 1;
            }
            else
            {
                index--;
            }

            if (!lockLabel)
                label.text = data[index];
            OnValueChanged.Invoke(index);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (overrideControl)
            { return; }

            ResetColor();
            background.color = colorHover;
        }

        public void OnPointerEnter()
        {
            if (!overrideControl)
            { return; }

            ResetColor();
            background.color = colorHover;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (overrideControl)
            { return; }

            ResetColor();
        }

        public void OnPointerExit()
        {
            if (!overrideControl)
            { return; }

            ResetColor();
        }

        public void OnPointerSelect()
        {
            //if (overrideControl) { return; }

            ResetColor();
            background.color = ColorSelected;
        }

        public void ResetColor()
        {
            background.color = colorIdle;
            maxIndex = data.Count - 1;
        }
    }
}