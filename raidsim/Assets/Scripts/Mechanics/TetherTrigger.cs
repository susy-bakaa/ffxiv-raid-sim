// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections;
using System.Collections.Generic;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using dev.susybaka.raidsim.Visuals;
using dev.susybaka.Shared;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.UI.PartyList;

namespace dev.susybaka.raidsim.Mechanics
{
    public class TetherTrigger : MonoBehaviour
    {
        LineRenderer[] lineRenderers;
        public enum TetherType { nearest, furthest, preDefined }

        public bool log = false;
        public PartyList partyList;
        public TetherType tetherType = TetherType.nearest;
        public CharacterState tetherSource;
        [Obsolete("Use tetherSource instead. This field is kept for compatability reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] public Transform startPoint;
        [FormerlySerializedAs("startOffset")] public Vector3 sourceOffset;
        public CharacterState tetherTarget;
        [Obsolete("Use tetherTarget instead. This field is kept for compatability reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] public Transform endPoint;
        [FormerlySerializedAs("endOffset")] public Vector3 targetOffset;
        public float maxDistance;
        public float breakDelay = 0.5f;
        public float visualBreakDelay = 0.75f;
        public bool initializeOnStart;
        public bool worldSpace = true;
        public bool grabbable = false;
        public bool makeSourceTargetTarget = false; // If true, when forming or solving a tether, source character will change their target controller target to the target character.
        public float tickRate = 10f; // Mechanic tick (Hz). 10Hz = 0.1s.
        [ShowIf(nameof(grabbable))] public float grabRadius = 0.6f; // How far from the tether line (in meters) counts as a grab (XZ plane).
        [ShowIf(nameof(grabbable))] public float swapLockSeconds = 0.75f; // After a swap, ignore further swaps for this many seconds.
        [ShowIf(nameof(grabbable))] public float carrierRegrabExtraRadius = 0.15f; // Don't allow the current carrier to re-grab immediately unless they clearly retake it (optional).

        public UnityEvent<ActionInfo> onForm;
        public UnityEvent<ActionInfo> onSwap;
        public UnityEvent<ActionInfo> onBreak;
        public UnityEvent<ActionInfo> onSolved;
        public UnityEvent<ActionInfo> onTick;

        private bool initialized;
        //private CharacterState startCharacter;
        //private CharacterState endCharacter;
        private Coroutine ieSetLineRenderersActive;
        private SimpleShaderFade[] shaderFades;
        private SimpleTetherEffect[] tetherEffects;
        private float tickAccum;
        private float swapLockRemaining;

#if UNITY_EDITOR
        [Header("Editor")]
        public bool drawDebug = true;
        public CharacterState attachToCharacter;
        [NaughtyAttributes.Button("Initialize")]
        public void InitializeButton()
        {
            if (attachToCharacter != null)
            {
                tetherTarget = attachToCharacter;
                Initialize(attachToCharacter);
            }
            else
            {
                Initialize();
            }
        }
#pragma warning disable CS0618 // Disable obsolete warning for startPoint and endPoint for the editor tool
        [NaughtyAttributes.Button("Migrate")]
        public void MigrateButton()
        {
            if (tetherSource == null && startPoint != null)
            {
                CharacterState cs = startPoint.GetComponentInParents<CharacterState>();
                if (cs != null)
                {
                    tetherSource = cs;
                    startPoint = null;
                    Debug.Log($"[TetherTrigger ({gameObject.name})] Migrated startPoint to tetherSource on {name}.");
                }
                else
                {
                    Debug.LogWarning($"[TetherTrigger ({gameObject.name})] Could not find CharacterState for startPoint on {name}. Migration failed.");
                }
            }
            if (tetherTarget == null && endPoint != null)
            {
                CharacterState cs = endPoint.GetComponentInParents<CharacterState>();
                if (cs != null)
                {
                    tetherTarget = cs;
                    endPoint = null;
                    Debug.Log($"[TetherTrigger ({gameObject.name})] Migrated endPoint to tetherTarget on {name}.");
                }
                else
                {
                    Debug.LogWarning($"[TetherTrigger ({gameObject.name})] Could not find CharacterState for endPoint on {name}. Migration failed.");
                }
            }
        }
#pragma warning restore CS0618
#endif

        private void Awake()
        {
            if (log)
                Debug.Log($"[TetherTrigger ({gameObject.name})] Awake called.");

            shaderFades = GetComponentsInChildren<SimpleShaderFade>(true);
            tetherEffects = GetComponentsInChildren<SimpleTetherEffect>(true);
            lineRenderers = GetComponentsInChildren<LineRenderer>(true);
            SetLineRenderersActive(false);
            if (tetherEffects != null && tetherEffects.Length > 0)
            {
                foreach (SimpleTetherEffect effect in tetherEffects)
                {
                    if (effect != null)
                    {
                        effect.Initialize(this);
                        effect.SetVisible(false);
                    }
                }
            }
            if (partyList == null)
            {
                partyList = FightTimeline.Instance.partyList;
            }
            initialized = false;

            if (FightTimeline.Instance != null)
            {
                if (tetherTarget != null)
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
            //startCharacter = null;
            //endCharacter = null;
        }

        private void Update()
        {
            if (tetherSource == null || tetherTarget == null)
                return;

            if (lineRenderers != null && lineRenderers.Length > 0)
            {
                foreach (LineRenderer lineRenderer in lineRenderers)
                {
                    if (lineRenderer == null)
                        continue;

                    if (worldSpace)
                    {
                        lineRenderer.SetPositions(new Vector3[2] { tetherSource.pivot.position + sourceOffset, tetherTarget.pivot.position + targetOffset });
                    }
                    else
                    {
                        lineRenderer.SetPositions(new Vector3[2] { tetherSource.pivot.localPosition + sourceOffset, tetherTarget.pivot.localPosition + targetOffset });
                    }
                }

            }

            if (maxDistance > 0f)
            {
                if (worldSpace)
                {
                    if (Vector3.Distance(tetherSource.transform.position, tetherTarget.transform.position) > maxDistance)
                    {
                        BreakTether();
                    }
                }
                else
                {
                    if (Vector3.Distance(tetherSource.transform.localPosition, tetherTarget.transform.localPosition) > maxDistance)
                    {
                        BreakTether();
                    }
                }
            }

            // Tick accumulator
            float tickInterval = (tickRate <= 0f) ? 0.1f : 1f / tickRate;
            tickAccum += Time.deltaTime;
            if (tickAccum < tickInterval)
                return;

            tickAccum -= tickInterval; // keep remainder for stability

            onTick?.Invoke(new ActionInfo(null, tetherSource, tetherTarget));

            if (!grabbable || partyList == null)
                return;

            if (swapLockRemaining > 0f)
                swapLockRemaining -= Time.deltaTime;

            if (swapLockRemaining > 0f)
                return;

            TryGrabTether();
        }

        public void ResetTether()
        {
            StopAllCoroutines();
            initialized = false;
            SetLineRenderersActive(false);
            //startCharacter = null;
            //endCharacter = null;
            ieSetLineRenderersActive = null;
        }

        public void ResetTetherFull()
        {
            StopAllCoroutines();
            initialized = false;
            SetLineRenderersActive(false);
            //startCharacter = null;
            //endCharacter = null;
            tetherTarget = null;
            ieSetLineRenderersActive = null;
        }

        public void Initialize()
        {
            if (!initialized)
            {
                if (log)
                    Debug.Log($"[TetherTrigger ({gameObject.name})] Initializing tether.");

                FormTether();
                initialized = true;
            }
        }

        public void Initialize(CharacterState target)
        {
            if (!initialized)
            {
                if (log)
                    Debug.Log($"[TetherTrigger ({gameObject.name})] Initializing tether with target {target.characterName}.");

                FormTether(target);
                initialized = true;
            }
        }

        public void FormTether()
        {
            if (log)
                Debug.Log($"[TetherTrigger ({gameObject.name})] FormTether called with TetherType {tetherType}.");

            List<CharacterState> members =  new List<CharacterState>(partyList.GetActiveMembers());

            switch (tetherType)
            {
                default:
                {
                    if (log)
                        Debug.Log($"[TetherTrigger ({gameObject.name})] FormTether nearest from party of {members.Count}");

                    CharacterState closestMember = null;
                    float closestDistance = float.MaxValue;

                    foreach (CharacterState member in members)
                    {
                        float distance = Vector3.Distance(transform.position, member.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestMember = member;
                        }
                    }

                    // Assuming a certain threshold for tethering, e.g., 1 unit
                    //float tetherThreshold = 1.0f;
                    //if (closestMember != null && closestDistance < tetherThreshold)
                    //{
                    //    FormTether(closestMember);
                    //}

                    if (log)
                        Debug.Log($"[TetherTrigger ({gameObject.name})] FormTether nearest selected {closestMember.characterName} at distance {closestDistance}");

                    FormTether(closestMember);
                    break;
                }
                case TetherType.furthest:
                {
                    if (log)
                        Debug.Log($"[TetherTrigger ({gameObject.name})] FormTether furthest from party of {members.Count}");

                    CharacterState furthestMember = null;
                    float furthestDistance = 0f;

                    foreach (CharacterState member in members)
                    {
                        float distance = Vector3.Distance(transform.position, member.transform.position);
                        if (distance > furthestDistance)
                        {
                            furthestDistance = distance;
                            furthestMember = member;
                        }
                    }

                    // Assuming a certain threshold for tethering, e.g., 1 unit
                    //float tetherThreshold = 1.0f;
                    //if (furthestMember != null && furthestDistance > tetherThreshold)
                    //{
                    //    FormTether(furthestMember);
                    //}

                    if (log)
                        Debug.Log($"[TetherTrigger ({gameObject.name})] FormTether furthest selected {furthestMember.characterName} at distance {furthestDistance}");

                    FormTether(furthestMember);
                    break;
                }
                case TetherType.preDefined:
                {
                    if (tetherSource != null && tetherTarget != null)
                    {
                        FormTether(tetherSource, tetherTarget);
                    }
                    else
                    {
                        SolveTether();
                    }
                    break;
                }
            }

            // Set source's target to the tether target if applicable
            if (makeSourceTargetTarget && tetherSource != null && tetherTarget != null && tetherSource.targetController != null && tetherTarget.targetController != null)
            {
                if (tetherTarget.targetController.self != null)
                {
                    tetherSource.targetController.SetTarget(tetherTarget.targetController.self);
                }
            }
        }

        public void FormTether(CharacterState target)
        {
            if (target == null)
            {
                Debug.LogWarning($"[TetherTrigger] Could not create a tether with null target. Operation aborted.");
                return;
            }

            FormTether(tetherSource, target);
        }

        public void FormTether(CharacterState source, CharacterState target)
        {
            if (source.gameObject.activeInHierarchy == false || target.gameObject.activeInHierarchy == false)
                Destroy(gameObject);

            SetLineRenderersActive(true);
            tetherSource = source;
            tetherTarget = target;

            onForm.Invoke(new ActionInfo(null, tetherSource, tetherTarget));
        }

        public void BreakTether()
        {
            ActionInfo actionInfo = new ActionInfo(null, tetherSource, tetherTarget);
            
            if (log)
                Debug.Log($"[TetherTrigger ({gameObject.name})] Tether between {tetherSource.characterName} ({tetherSource.gameObject.name}) and {tetherTarget.characterName} ({tetherTarget.gameObject.name}) broken.\nActionInfo: action '{actionInfo.action}' source '{actionInfo.source}' target '{actionInfo.target}' targetIsPlayer '{actionInfo.targetIsPlayer}'");
            
            if (ieSetLineRenderersActive == null)
                ieSetLineRenderersActive = StartCoroutine(IE_SetLineRenderersActive(false, new WaitForSeconds(visualBreakDelay)));
            Utilities.FunctionTimer.Create(this, () => onBreak.Invoke(actionInfo), breakDelay, $"TetherTrigger_{this}_{GetHashCode()}_Break_Delay", false, true);
        }

        public void SolveTether()
        {
            ActionInfo actionInfo = new ActionInfo(null, tetherSource, tetherTarget);

            if (log)
                Debug.Log($"[TetherTrigger ({gameObject.name})] Tether between {tetherSource.characterName} ({tetherSource.gameObject.name}) and {tetherTarget.characterName} ({tetherTarget.gameObject.name}) was solved.\nActionInfo: action '{actionInfo.action}' source '{actionInfo.source}' target '{actionInfo.target}' targetIsPlayer '{actionInfo.targetIsPlayer}'");

            if (ieSetLineRenderersActive == null)
                ieSetLineRenderersActive = StartCoroutine(IE_SetLineRenderersActive(false, new WaitForSeconds(visualBreakDelay)));
            Utilities.FunctionTimer.Create(this, () => onSolved.Invoke(actionInfo), breakDelay, $"TetherTrigger_{this}_{GetHashCode()}_Solve_Delay", false, true);

            // Set source's target to the tether target if applicable here as well, in case the tether was swapped before being solved
            if (makeSourceTargetTarget && tetherSource != null && tetherTarget != null && tetherSource.targetController != null && tetherTarget.targetController != null)
            {
                if (tetherTarget.targetController.self != null)
                {
                    tetherSource.targetController.SetTarget(tetherTarget.targetController.self);
                }
            }
        }

        private void TryGrabTether()
        {
            List<CharacterState> members = partyList.GetActiveMembers();
            if (members == null || members.Count == 0)
                return;

            Vector3 a3 = tetherSource.pivot.position;
            Vector3 b3 = tetherTarget.pivot.position;

            // Flatten to XZ plane
            Vector2 A = new Vector2(a3.x, a3.z);
            Vector2 B = new Vector2(b3.x, b3.z);

            // If tether is degenerate, skip.
            if ((B - A).sqrMagnitude < 0.0001f)
                return;

            CharacterState currentCarrier = tetherTarget;

            if (currentCarrier == null)
                return;

            CharacterState bestCandidate = null;
            float bestT = float.PositiveInfinity; // smallest t = closest to source along segment
            float bestDistSq = float.PositiveInfinity;

            for (int i = 0; i < members.Count; i++)
            {
                CharacterState c = members[i];
                if (c == null)
                    continue;

                // Skip the current carrier if it’s the same CharacterState as endPoint.
                // (If your endPoint isn't the carrier transform, adjust this.)
                if (currentCarrier != null && ReferenceEquals(c, currentCarrier))
                    continue;

                Vector3 p3 = c.transform.position;
                Vector2 P = new Vector2(p3.x, p3.z);

                float t;
                Vector2 closest = ClosestPointOnSegment(A, B, P, out t);
                float distSq = (P - closest).sqrMagnitude;

                // Optional: make it slightly harder for the current carrier to "regrab" if you *don't* skip them.
                float radius = grabRadius;
                // If you decide NOT to skip currentCarrier above, you can do:
                // if (ReferenceEquals(c, currentCarrier)) radius += carrierRegrabExtraRadius;

                if (distSq > radius * radius)
                    continue;

                // Winner selection:
                // 1) smallest t (closest to source)
                // 2) then smallest distance (more "inside" the line)
                if (t < bestT - 0.0001f || (Mathf.Abs(t - bestT) <= 0.0001f && distSq < bestDistSq))
                {
                    bestCandidate = c;
                    bestT = t;
                    bestDistSq = distSq;
                }
            }

            if (bestCandidate == null)
                return;

            // Swap tether end to the new carrier.
            CharacterState oldCarrier = currentCarrier;
            tetherTarget = bestCandidate;

            swapLockRemaining = swapLockSeconds;

            onSwap?.Invoke(new ActionInfo(null, oldCarrier, bestCandidate));
        }

        /// <summary>
        /// Returns closest point on segment AB to point P, and outputs t in [0,1].
        /// </summary>
        private static Vector2 ClosestPointOnSegment(Vector2 A, Vector2 B, Vector2 P, out float t)
        {
            Vector2 AB = B - A;
            float abLenSq = Vector2.Dot(AB, AB);
            if (abLenSq <= 0.000001f)
            {
                t = 0f;
                return A;
            }

            t = Vector2.Dot(P - A, AB) / abLenSq;
            t = Mathf.Clamp01(t);
            return A + AB * t;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawDebug || tetherSource == null || tetherTarget == null || tetherSource.pivot == null || tetherTarget.pivot == null)
                return;

            Vector3 a = tetherSource.pivot.position;
            Vector3 b = tetherTarget.pivot.position;

            Gizmos.DrawLine(a, b);

            // Draw a "tube" approximation at a few points to show intercept radius
            if (grabRadius > 0f)
            {
                Gizmos.DrawWireSphere(a, grabRadius);
                Gizmos.DrawWireSphere(b, grabRadius);

                Vector3 mid = (a + b) * 0.5f;
                Gizmos.DrawWireSphere(mid, grabRadius);
            }
        }
#endif

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

                if (shaderFades == null || shaderFades.Length < 1)
                {
                    lineRenderer.gameObject.SetActive(state);
                }
            }

            if (shaderFades != null && shaderFades.Length > 0)
            {
                foreach (SimpleShaderFade shaderFade in shaderFades)
                {
                    if (shaderFade != null)
                    {
                        if (state)
                        {
                            shaderFade.FadeIn();
                        }
                        else
                        {
                            shaderFade.FadeOut();
                        }
                    }
                }
            }

            if (tetherEffects != null && tetherEffects.Length > 0)
            {
                foreach (SimpleTetherEffect effect in tetherEffects)
                {
                    if (effect != null)
                    {
                        effect.SetVisible(state);
                    }
                }
            }
        }
    }
}