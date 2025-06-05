/**
MIT License
Copyright (c) 2023 Bayat Games
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
**/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bayat.Games.Animation.Utilities
{

    /// <summary>
    /// Animator controller utility methods and extensions.
    /// </summary>
    public static class AnimatorUtility
    {

        #region Fields

        private static Dictionary<Animator, HashSet<int>> animatorToParameters = new();
        private static Dictionary<string, int> parameterNameToHash = new();
        private static Dictionary<Animator, int> animatorUsages = new();

        #endregion

        #region Helpers

        /// <summary>
        /// Clears all the caches.
        /// </summary>
        public static void ClearCaches()
        {
            animatorToParameters.Clear();
            parameterNameToHash.Clear();
            animatorUsages.Clear();
        }

        /// <summary>
        /// Initializes the animator.
        /// </summary>
        /// <remarks>
        /// 1. Disables logging of the animator.
        /// 2. Gets the animator parameters and caches them
        /// </remarks>
        /// <param name="animator">The animator</param>
        public static void InitializeAnimator(this Animator animator)
        {

            // Uncomment to disable log warnings to save on performance
            //animator.logWarnings = false;

            // Add parameters as cached
            GetParameters(animator);
        }

        /// <summary>
        /// Adds a usage for the animator.
        /// </summary>
        /// <remarks>
        /// Once the usage count becomes greater than 0, it gets the animator's parameters and caches them.
        /// </remarks>
        /// <param name="animator">The animator</param>
        public static void AddAnimatorUsage(this Animator animator)
        {
            if (animatorUsages.TryGetValue(animator, out int currentUsages))
            {
                animatorUsages[animator] = currentUsages + 1;
            }
            else
            {
                animatorUsages[animator] = 1;

                // Initialize on first usage
                InitializeAnimator(animator);
            }
        }

        /// <summary>
        /// Removes the usage from the animator.
        /// </summary>
        /// <remarks>
        /// If the usage count is 0, then the animator parameters will be removed from the cache, call this when the Unity object is being destroyed or removed, or you no longer use the animator.
        /// </remarks>
        /// <param name="animator">The animator</param>
        public static void RemoveAnimatorUsage(this Animator animator)
        {
            if (animatorUsages.TryGetValue(animator, out int currentUsages))
            {
                animatorUsages[animator] = currentUsages - 1;
            }
            else
            {
                animatorUsages.Remove(animator);

                // Removo cached parameters
                animatorToParameters.Remove(animator);
            }
        }

        /// <summary>
        /// Gets the parameter's name hash.
        /// </summary>
        /// <remarks>
        /// It uses <see cref="Animator.StringToHash(string)"/> for the first time and then caches the name in order to be reused later on.
        /// </remarks>
        /// <param name="parameterName">The parameter name</param>
        /// <returns>Returns the parameter's name hash</returns>
        public static int GetParameterNameHash(string parameterName)
        {
            if (parameterNameToHash.TryGetValue(parameterName, out int parameterHash))
            {
                return parameterHash;
            }

            return parameterNameToHash[parameterName] = Animator.StringToHash(parameterName);
        }

        /// <summary>
        /// Gets the animator parameters as a <see cref="HashSet{T}"/> of their name hashes.
        /// </summary>
        /// <remarks>
        /// It uses the cached version if the method is called more than once.
        /// </remarks>
        /// <param name="animator">The animator</param>
        /// <param name="parameterName">The parameter name</param>
        /// <returns>Returns a <see cref="HashSet{T}"/> of animator parameters name hashes</returns>
        public static HashSet<int> GetParameters(Animator animator)
        {
            HashSet<int> parameters;
            if (animatorToParameters.TryGetValue(animator, out parameters))
            {
                return parameters;
            }

            parameters = new HashSet<int>();
            for (int i = 0; i < animator.parameterCount; i++)
            {
                AnimatorControllerParameter parameter = animator.GetParameter(i);
                parameters.Add(parameter.nameHash);
            }

            return parameters;
        }

        /// <summary>
        /// Checks whether the animator has the parameter or not.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterName">The parameter name</param>
        /// <returns>Returns true if the animator has the parameter, otherwise false</returns>
        public static bool HasParameter(this Animator animator, string parameterName)
        {
            return HasParameter(animator, GetParameterNameHash(parameterName));
        }

        /// <summary>
        /// Checks whether the animator has the parameter or not.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterHash">The parameter hash</param>
        /// <returns>Returns true if the animator has the parameter, otherwise false</returns>
        public static bool HasParameter(this Animator animator, int parameterHash)
        {
            return GetParameters(animator).Contains(parameterHash);
        }

        #endregion

        #region Reset Trigger

        /// <summary>
        /// Resets the value of the given trigger parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterName">The parameter name</param>
        public static void ResetTriggerSafe(this Animator animator, string parameterName)
        {
            ResetTriggerSafe(animator, GetParameterNameHash(parameterName));
        }

        /// <summary>
        /// Resets the value of the given trigger parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterHash">The parameter hash</param>
        public static void ResetTriggerSafe(this Animator animator, int parameterHash)
        {
            if (HasParameter(animator, parameterHash))
            {
                animator.ResetTrigger(parameterHash);
            }
        }

        #endregion

        #region Set Trigger

        /// <summary>
        /// Sets the value of the given trigger parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterName">The parameter name</param>
        public static void SetTriggerSafe(this Animator animator, string parameterName)
        {
            SetTriggerSafe(animator, GetParameterNameHash(parameterName));
        }

        /// <summary>
        /// Sets the value of the given trigger parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterHash">The parameter hash</param>
        public static void SetTriggerSafe(this Animator animator, int parameterHash)
        {
            if (HasParameter(animator, parameterHash))
            {
                animator.SetTrigger(parameterHash);
            }
        }

        #endregion

        #region Get Bool

        /// <summary>
        /// Gets the value of the given boolean parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterName">The parameter name</param>
        public static bool GetBoolSafe(this Animator animator, string parameterName, bool defaultValue = false)
        {
            return GetBoolSafe(animator, GetParameterNameHash(parameterName), defaultValue);
        }

        /// <summary>
        /// Gets the value of the given boolean parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterHash">The parameter hash</param>
        public static bool GetBoolSafe(this Animator animator, int parameterHash, bool defaultValue = false)
        {
            if (HasParameter(animator, parameterHash))
            {
                return animator.GetBool(parameterHash);
            }

            return defaultValue;
        }

        #endregion

        #region Set Bool

        /// <summary>
        /// Sets the value of the given boolean parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterName">The parameter name</param>
        public static void SetBoolSafe(this Animator animator, string parameterName, bool value)
        {
            SetBoolSafe(animator, GetParameterNameHash(parameterName), value);
        }

        /// <summary>
        /// Sets the value of the given boolean parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterHash">The parameter hash</param>
        public static void SetBoolSafe(this Animator animator, int parameterHash, bool value)
        {
            if (HasParameter(animator, parameterHash))
            {
                animator.SetBool(parameterHash, value);
            }
        }

        #endregion

        #region Get Float

        /// <summary>
        /// Gets the value of the given float parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterName">The parameter name</param>
        public static float GetFloatSafe(this Animator animator, string parameterName, float defaultValue = 0f)
        {
            return GetFloatSafe(animator, GetParameterNameHash(parameterName), defaultValue);
        }

        /// <summary>
        /// Gets the value of the given float parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterHash">The parameter hash</param>
        public static float GetFloatSafe(this Animator animator, int parameterHash, float defaultValue = 0f)
        {
            if (HasParameter(animator, parameterHash))
            {
                return animator.GetFloat(parameterHash);
            }

            return defaultValue;
        }

        #endregion

        #region Set Float

        /// <summary>
        /// Sets the value of the given integer parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterName">The parameter name</param>
        public static void SetIntegerSafe(this Animator animator, string parameterName, int value)
        {
            SetIntegerSafe(animator, GetParameterNameHash(parameterName), value);
        }

        /// <summary>
        /// Sets the value of the given integer parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterHash">The parameter hash</param>
        public static void SetIntegerSafe(this Animator animator, int parameterHash, int value)
        {
            if (HasParameter(animator, parameterHash))
            {
                animator.SetInteger(parameterHash, value);
            }
        }

        #endregion

        #region Get Integer

        /// <summary>
        /// Gets the value of the given integer parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterName">The parameter name</param>
        public static int GetIntegerSafe(this Animator animator, string parameterName, int defaultValue = 0)
        {
            return GetIntegerSafe(animator, GetParameterNameHash(parameterName), defaultValue);
        }

        /// <summary>
        /// Gets the value of the given integer parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterHash">The parameter hash</param>
        public static int GetIntegerSafe(this Animator animator, int parameterHash, int defaultValue = 0)
        {
            if (HasParameter(animator, parameterHash))
            {
                return animator.GetInteger(parameterHash);
            }

            return defaultValue;
        }

        #endregion

        #region Set Integer

        /// <summary>
        /// Sets the value of the given float parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterName">The parameter name</param>
        public static void SetFloatSafe(this Animator animator, string parameterName, float value)
        {
            SetFloatSafe(animator, GetParameterNameHash(parameterName), value);
        }

        /// <summary>
        /// Sets the value of the given float parameter safely.
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="parameterHash">The parameter hash</param>
        public static void SetFloatSafe(this Animator animator, int parameterHash, float value)
        {
            if (HasParameter(animator, parameterHash))
            {
                animator.SetFloat(parameterHash, value);
            }
        }

        #endregion

    }

}