// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class IterativeMechanic : FightMechanic
    {
        [Header("Iterative Mechanic Settings")]
        public int iterationsDone = 0;
        public int maxIterations = -1;
        public bool repeat = false;
        public bool clamp = false;
        public List<UnityEvent<ActionInfo>> iterations;

        private bool finished = false;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (iterations != null && iterations.Count > 0)
            {
                maxIterations = iterations.Count;
            }
            else
            {
                maxIterations = -1;
            }
        }
#endif

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (iterations == null || iterations.Count < 1)
            {
                if (log)
                    Debug.Log($"[IterativeMechanic ({gameObject.name})] No available iterations to use, canceled execution.");
                return;
            }

            if (finished)
            {
                if (log)
                    Debug.Log($"[IterativeMechanic ({gameObject.name})] All iterations finished, canceled execution.");
                return;
            }

            // Determine which iteration to execute
            int iterationIndex = iterationsDone;
            
            // Handle clamping - use last iteration if we've exceeded
            if (clamp && iterationIndex >= iterations.Count)
            {
                iterationIndex = iterations.Count - 1;
            }
            
            // Execute the current iteration
            if (iterationIndex < iterations.Count)
            {
                if (log)
                    Debug.Log($"[IterativeMechanic ({gameObject.name})] Executing iteration {iterationIndex + 1}/{maxIterations}");
                
                iterations[iterationIndex]?.Invoke(actionInfo);
                iterationsDone++;
                
                // Check if we've reached the maximum
                if (iterationsDone >= maxIterations)
                {
                    if (repeat)
                    {
                        if (log)
                            Debug.Log($"[IterativeMechanic ({gameObject.name})] Reached max iterations, repeating from start.");
                        iterationsDone = 0;
                    }
                    else if (clamp)
                    {
                        if (log)
                            Debug.Log($"[IterativeMechanic ({gameObject.name})] Reached max iterations, clamping to final value.");
                        iterationsDone = maxIterations - 1;
                    }
                    else
                    {
                        if (log)
                            Debug.Log($"[IterativeMechanic ({gameObject.name})] Reached max iterations, marking as finished.");
                        finished = true;
                    }
                }
            }
        }

        public override void InterruptMechanic(ActionInfo actionInfo)
        {
            base.InterruptMechanic(actionInfo);
            
            if (log)
                Debug.Log($"[IterativeMechanic ({gameObject.name})] Mechanic interrupted, resetting state.");
            
            iterationsDone = 0;
            finished = false;
        }
    }
}