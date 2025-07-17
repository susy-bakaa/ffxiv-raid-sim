using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.Shared;
using dev.susybaka.Shared.Attributes;
using dev.susybaka.Shared.UserInterface;
using UnityEngine.Serialization;

namespace dev.susybaka.raidsim.UI
{
    public class HudElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        RectTransform rectTransform;
        CanvasGroup canvasGroup;
        UserInput input;
        HudElementPriority priorityHandler;
        HudElementGroup elementGroup;

        [Header("Sorting")]
        public int priority = 1;
        private int originalPriority;
        public bool hidden = false;
        private bool wasHidden;
        public bool omitSorting = false;
        [Header("Data")]
        public CharacterState characterState;
        public bool destroyIfMissing = false;
        public bool initializeOnStart = false;
        public bool isPartyListElement = false;
        public GameObject untargetableOverlay;
        public Button targetButton;
        [Header("Input")]
        public bool blocksAllInput = false;
        public bool blocksPosInput = false;
        public bool blocksRotInput = false;
        public bool blocksScrInput = false;
        public bool blocksTargetRaycasts = false;
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
        public bool changeCursor = false;
        [ShowIf("changeCursor")][CursorName][FormerlySerializedAs("cursorName")] public string hoverCursorName = "interact";
        [ShowIf("changeCursor")][CursorName] public string dragCursorName = "interact";
        [Header("Audio")]
        public bool restrictsAudio = true;
        [FormerlySerializedAs("canPlayAudio")] public bool playHoverAudio = false;
        [FormerlySerializedAs("handleButtonAudio")] public bool playClickAudio = false;
        [Header("Events")]
        public UnityEvent<HudElementEventInfo> onInitialize;
        public bool onPointerEnterEnabled = true;
        [ShowIf("onPointerEnterEnabled")] public UnityEvent<HudElementEventInfo> onPointerEnter;
        public bool onPointerExitEnabled = true;
        [ShowIf("onPointerExitEnabled")] public UnityEvent<HudElementEventInfo> onPointerExit;
        public bool onPointerClickEnabled = true;
        [ShowIf("onPointerClickEnabled")] public UnityEvent<HudElementEventInfo> onPointerClick;
        public bool otherPointerEventsEnabled = true;


#if UNITY_EDITOR
        [Header("Editor")]
        public int dummy;
        private bool currentColor = false;

        [Button("Swap Color")]
        private void SwapColor()
        {
            currentColor = !currentColor;
            ChangeColors(currentColor);
        }

        private void OnValidate()
        {
            if (!changeCursor)
            {
                hoverCursorName = "<None>";
                dragCursorName = "<None>";
            }
            else if (string.IsNullOrEmpty(dragCursorName) || (dragCursorName == "<None>" && hoverCursorName != "<None>"))
            {
                dragCursorName = hoverCursorName;
            }
        }
#endif

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            priorityHandler = GetComponentInParent<HudElementPriority>();
            elementGroup = Utilities.GetComponentInParents<HudElementGroup>(transform);

            if (elementGroup != null && elementGroup.populateAutomatically && !elementGroup.setupOnStart)
            {
                elementGroup.AddElement(this);
            }    

            if (transform.parent != null && transform.parent.TryGetComponent(out PartyList pList))
            {
                if (pList.SetupDone)
                    pList.UpdatePartyList();
                isPartyListElement = true;
                if (untargetableOverlay == null || targetButton == null)
                {
                    untargetableOverlay = transform.GetChild(transform.childCount - 1).gameObject;
                    targetButton = transform.GetComponentInChildren<Button>();
                    if (targetButton.gameObject.name == untargetableOverlay.gameObject.name)
                    {
                        untargetableOverlay = null;
                    }
                }
            }

            if (input == null)
            {
                if (blocksAllInput || blocksPosInput || blocksRotInput || blocksScrInput || blocksTargetRaycasts)
                {
                    if (FightTimeline.Instance != null)
                        input = FightTimeline.Instance.input;
                }
            }

            originalPriority = priority;
        }

        private void Start()
        {
            if (initializeOnStart)
            {
                Initialize();
            }
        }

        private void OnDestroy()
        {
            if (elementGroup != null)
            {
                elementGroup.RemoveElement(this);
            }
        }

        private void Update()
        {
            if (isPartyListElement)
            {
                if (Utilities.RateLimiter(13))
                {
                    if (characterState != null)
                    {
                        if (characterState.untargetable.value)
                        {
                            if (untargetableOverlay != null)
                                untargetableOverlay.SetActive(true);
                            if (targetButton != null)
                                targetButton.interactable = false;
                        }
                        else
                        {
                            if (untargetableOverlay != null)
                                untargetableOverlay.SetActive(false);
                            if (targetButton != null)
                                targetButton.interactable = true;
                        }
                    }
                }
            }

            if (wasHidden != hidden)
            {
                if (hidden)
                {
                    if (!omitSorting)
                    {
                        priority = 512;
                        canvasGroup.alpha = 0f;
                        canvasGroup.interactable = false;
                        canvasGroup.blocksRaycasts = false;

                        if (priorityHandler != null)
                            priorityHandler.UpdateSorting();
                    }
                    else
                    {
                        canvasGroup.alpha = 0f;
                    }
                }
                else
                {
                    if (!omitSorting)
                    {
                        priority = originalPriority;

                        if (fadeOutTime < 0f)
                            canvasGroup.alpha = 1f;

                        canvasGroup.interactable = true;
                        canvasGroup.blocksRaycasts = true;

                        if (priorityHandler != null)
                            priorityHandler.UpdateSorting();
                    }
                    else
                    {
                        canvasGroup.alpha = 1f;
                    }
                }
                wasHidden = hidden;
            }

            if (destroyIfMissing)
            {
                if (characterState == null)
                {
                    Destroy(gameObject, 0.1f);
                }
            }

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
                    Utilities.FunctionTimer.Create(this, () => canvasGroup.LeanAlpha(0f, fadeOutTime), fadeOutDelay, $"{gameObject}_{gameObject.GetHashCode()}_HudElement_FadeOutDelay", false, true);
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

        public void Initialize()
        {
            onInitialize.Invoke(new HudElementEventInfo(this));
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
            if (input != null)
            {
                if (blocksAllInput)
                    input.inputEnabled = false;
                if (blocksPosInput)
                    input.movementInputEnabled = false;
                if (blocksRotInput)
                    input.rotationInputEnabled = false;
                if (blocksScrInput)
                    input.zoomInputEnabled = false;
                if (blocksTargetRaycasts)
                    input.targetRaycastInputEnabled = false;
            }

            if (!onPointerEnterEnabled)
                return;

            if (changeCursor && CursorHandler.Instance != null && !string.IsNullOrEmpty(hoverCursorName))
            {
                CursorHandler.Instance.SetCursorByName(hoverCursorName);
            }

            onPointerEnter.Invoke(new HudElementEventInfo(this, eventData));
        }

        public void OnPointerExit()
        {
            OnPointerExit(null);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (input != null)
            {
                if (blocksAllInput)
                    input.inputEnabled = true;
                if (blocksPosInput)
                    input.movementInputEnabled = true;
                if (blocksRotInput)
                    input.rotationInputEnabled = true;
                if (blocksScrInput)
                    input.zoomInputEnabled = true;
                if (blocksTargetRaycasts)
                    input.targetRaycastInputEnabled = true;
            }

            if (!onPointerExitEnabled)
                return;

            if (changeCursor && CursorHandler.Instance != null && !string.IsNullOrEmpty(hoverCursorName))
            {
                CursorHandler.Instance.SetCursorByID(0); // Set back to default cursor
            }

            onPointerExit.Invoke(new HudElementEventInfo(this, eventData));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!onPointerClickEnabled)
                return;

            onPointerClick.Invoke(new HudElementEventInfo(this, eventData));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!otherPointerEventsEnabled)
                return;

            if (changeCursor && CursorHandler.Instance != null && !string.IsNullOrEmpty(hoverCursorName))
            {
                CursorHandler.Instance.SetCursorByName(dragCursorName);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!otherPointerEventsEnabled)
                return;

            if (changeCursor && CursorHandler.Instance != null && !string.IsNullOrEmpty(hoverCursorName))
            {
                CursorHandler.Instance.SetCursorByName(hoverCursorName);
            }
        }
    }

    public struct HudElementEventInfo
    {
        public HudElement element;
        public PointerEventData eventData;
        public bool boolean;
        public int integer;

        public HudElementEventInfo(HudElement element, PointerEventData eventData, bool boolean, int integer)
        {
            this.element = element;
            this.eventData = eventData;
            this.boolean = boolean;
            this.integer = integer;
        }

        public HudElementEventInfo(HudElement element, PointerEventData eventData, int integer)
        {
            this.element = element;
            this.eventData = eventData;
            this.boolean = false;
            this.integer = integer;
        }

        public HudElementEventInfo(HudElement element, PointerEventData eventData, bool boolean)
        {
            this.element = element;
            this.eventData = eventData;
            this.boolean = boolean;
            this.integer = 0;
        }

        public HudElementEventInfo(HudElement element, PointerEventData eventData)
        {
            this.element = element;
            this.eventData = eventData;
            this.boolean = false;
            this.integer = 0;
        }

        public HudElementEventInfo(HudElement element, bool boolean, int integer)
        {
            this.element = element;
            this.eventData = null;
            this.boolean = boolean;
            this.integer = integer;
        }

        public HudElementEventInfo(HudElement element, int integer)
        {
            this.element = element;
            this.eventData = null;
            this.boolean = false;
            this.integer = integer;
        }

        public HudElementEventInfo(HudElement element, bool boolean)
        {
            this.element = element;
            this.eventData = null;
            this.boolean = boolean;
            this.integer = 0;
        }

        public HudElementEventInfo(HudElement element)
        {
            this.element = element;
            this.eventData = null;
            this.boolean = false;
            this.integer = 0;
        }
    }
}