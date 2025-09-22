// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using dev.susybaka.raidsim.Visuals;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.UI.PartyList;

namespace dev.susybaka.raidsim.Mechanics
{
    public class TetherTrigger : MonoBehaviour
    {
        LineRenderer[] lineRenderers;
        public enum TetherType { nearest, furthest, preDefined }

        public PartyList partyList;
        public TetherType tetherType = TetherType.nearest;
        public Transform startPoint;
        public Vector3 startOffset;
        public Transform endPoint;
        public Vector3 endOffset;
        public float maxDistance;
        public float breakDelay = 0.5f;
        public float visualBreakDelay = 0.75f;
        public bool initializeOnStart;
        public bool worldSpace = true;

        public UnityEvent<CharacterState> onForm;
        public UnityEvent onBreak;
        public UnityEvent onSolved;

        private bool initialized;
        private CharacterState startCharacter;
        private CharacterState endCharacter;
        private Coroutine ieSetLineRenderersActive;
        private SimpleShaderFade shaderFade;

#if UNITY_EDITOR
        [Header("Editor")]
        public CharacterState attachToCharacter;
        [NaughtyAttributes.Button("Initialize")]
        public void InitializeButton()
        {
            if (attachToCharacter != null)
            {
                endPoint = attachToCharacter.transform.Find("Pivot");
                Initialize(attachToCharacter);
            }
            else
            {
                Initialize();
            }
        }
#endif

        private void Awake()
        {
            shaderFade = GetComponentInChildren<SimpleShaderFade>(true);
            lineRenderers = GetComponentsInChildren<LineRenderer>(true);
            SetLineRenderersActive(false);
            if (partyList == null)
            {
                partyList = FightTimeline.Instance.partyList;
            }

            initialized = false;

            if (FightTimeline.Instance != null)
            {
                if (endPoint != null)
                    FightTimeline.Instance.onReset.AddListener(ResetTether);
                else
                    FightTimeline.Instance.onReset.AddListener(ResetTetherFull);
            }
        }

        private void Start()
        {
            if (initializeOnStart)
            {
                Initialize();
            }
        }

        private void OnDisable()
        {
            initialized = false;
            startCharacter = null;
            endCharacter = null;
        }

        private void Update()
        {
            if (startPoint == null || endPoint == null)
                return;

            if (lineRenderers != null && lineRenderers.Length > 0)
            {
                foreach (LineRenderer lineRenderer in lineRenderers)
                {
                    if (lineRenderer == null)
                        continue;

                    if (worldSpace)
                    {
                        lineRenderer.SetPositions(new Vector3[2] { startPoint.position + startOffset, endPoint.position + endOffset });
                    }
                    else
                    {
                        lineRenderer.SetPositions(new Vector3[2] { startPoint.localPosition + startOffset, endPoint.localPosition + endOffset });
                    }
                }

            }

            if (maxDistance > 0f)
            {
                if (worldSpace)
                {
                    if (Vector3.Distance(startPoint.position, endPoint.position) > maxDistance)
                    {
                        BreakTether();
                    }
                }
                else
                {
                    if (Vector3.Distance(startPoint.localPosition, endPoint.localPosition) > maxDistance)
                    {
                        BreakTether();
                    }
                }
            }
        }

        public void ResetTether()
        {
            StopAllCoroutines();
            initialized = false;
            SetLineRenderersActive(false);
            startCharacter = null;
            endCharacter = null;
            ieSetLineRenderersActive = null;
        }

        public void ResetTetherFull()
        {
            StopAllCoroutines();
            initialized = false;
            SetLineRenderersActive(false);
            startCharacter = null;
            endCharacter = null;
            endPoint = null;
            ieSetLineRenderersActive = null;
        }

        public void Initialize()
        {
            if (!initialized)
            {
                FormTether();
                initialized = true;
            }
        }

        public void Initialize(CharacterState target)
        {
            if (!initialized)
            {
                FormTether(target);
                initialized = true;
            }
        }

        public void FormTether()
        {
            switch (tetherType)
            {
                default:
                {
                    CharacterState closestMember = null;
                    float closestDistance = float.MaxValue;

                    foreach (PartyMember member in partyList.members)
                    {
                        float distance = Vector3.Distance(transform.position, member.characterState.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestMember = member.characterState;
                        }
                    }

                    // Assuming a certain threshold for tethering, e.g., 1 unit
                    //float tetherThreshold = 1.0f;
                    //if (closestMember != null && closestDistance < tetherThreshold)
                    //{
                    //    FormTether(closestMember);
                    //}
                    FormTether(closestMember);
                    break;
                }
                case TetherType.furthest:
                {
                    CharacterState furthestMember = null;
                    float furthestDistance = 0f;

                    foreach (PartyMember member in partyList.members)
                    {
                        float distance = Vector3.Distance(transform.position, member.characterState.transform.position);
                        if (distance > furthestDistance)
                        {
                            furthestDistance = distance;
                            furthestMember = member.characterState;
                        }
                    }

                    // Assuming a certain threshold for tethering, e.g., 1 unit
                    //float tetherThreshold = 1.0f;
                    //if (furthestMember != null && furthestDistance > tetherThreshold)
                    //{
                    //    FormTether(furthestMember);
                    //}
                    FormTether(furthestMember);
                    break;
                }
                case TetherType.preDefined:
                {
                    if (startPoint != null && endPoint != null)
                    {
                        FormTether(startPoint, endPoint);
                    }
                    else
                    {
                        SolveTether();
                    }
                    break;
                }
            }
        }

        public void FormTether(CharacterState target)
        {
            Transform result = target.transform.Find("Pivot"); // target.transform.GetChild(target.transform.childCount - 2).transform

            if (result == null)
            {
                Debug.LogWarning($"[TetherTrigger] Could not find 'Pivot' in {target.gameObject.name}. Using the target's transform instead.");
                result = target.transform;
            }

            FormTether(startPoint, result);
        }

        public void FormTether(Transform start, Transform end)
        {
            if (start.gameObject.activeInHierarchy == false || end.gameObject.activeInHierarchy == false)
                Destroy(gameObject);

            SetLineRenderersActive(true);
            startPoint = start;
            endPoint = end;

            if (end.TryGetComponentInParents(out CharacterState endState))
            {
                endCharacter = endState;
                onForm.Invoke(endState);
            }
            else if (start.TryGetComponentInParents(out CharacterState startState))
            {
                startCharacter = startState;
                onForm.Invoke(startState);
            }
            else
            {
                endCharacter = null;
                startCharacter = null;
                onForm.Invoke(null);
            }
        }

        public void BreakTether()
        {
            if (ieSetLineRenderersActive == null)
                ieSetLineRenderersActive = StartCoroutine(IE_SetLineRenderersActive(false, new WaitForSeconds(visualBreakDelay)));
            Utilities.FunctionTimer.Create(this, () => onBreak.Invoke(), breakDelay, $"TetherTrigger_{this}_{GetHashCode()}_Break_Delay", false, true);
        }

        public void SolveTether()
        {
            if (ieSetLineRenderersActive == null)
                ieSetLineRenderersActive = StartCoroutine(IE_SetLineRenderersActive(false, new WaitForSeconds(visualBreakDelay)));
            Utilities.FunctionTimer.Create(this, () => onSolved.Invoke(), breakDelay, $"TetherTrigger_{this}_{GetHashCode()}_Solve_Delay", false, true);
        }

        private IEnumerator IE_SetLineRenderersActive(bool state, WaitForSeconds wait)
        {
            yield return wait;
            SetLineRenderersActive(state);
            ieSetLineRenderersActive = null;
        }

        private void SetLineRenderersActive(bool state)
        {
            foreach (LineRenderer lineRenderer in lineRenderers)
            {
                if (lineRenderer == null)
                    continue;

                if (shaderFade == null)
                {
                    lineRenderer.gameObject.SetActive(state);
                }
                else
                {
                    if (state)
                        shaderFade.FadeIn();
                    else
                        shaderFade.FadeOut();
                }
            }
        }
    }
}