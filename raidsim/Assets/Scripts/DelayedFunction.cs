using System;
using System.Collections;
using UnityEngine;

namespace dev.susybaka.raidsim
{
    public class DelayedFunction : MonoBehaviour
    {
        Coroutine ieDelayedFunction;

        public void Trigger(Action function, WaitForSeconds wait)
        {
            if (wait == null)
            {
                Debug.LogWarning("No delay specified. Aborting.");
                return;
            }

            if (ieDelayedFunction == null && gameObject.scene.isLoaded)
                ieDelayedFunction = StartCoroutine(IE_DelayedFunction(function, wait));
        }

        public void Trigger(Action function, WaitForSecondsRealtime wait)
        {
            if (wait == null)
            {
                Debug.LogWarning("No delay specified. Aborting.");
                return;
            }

            if (ieDelayedFunction == null && gameObject.scene.isLoaded)
                ieDelayedFunction = StartCoroutine(IE_DelayedFunctionRealtime(function, wait));
        }

        private IEnumerator IE_DelayedFunction(Action action, WaitForSeconds wait)
        {
            yield return wait;
            action?.Invoke();
            Destroy(gameObject);
        }

        private IEnumerator IE_DelayedFunctionRealtime(Action action, WaitForSecondsRealtime wait)
        {
            yield return wait;
            action?.Invoke();
            Destroy(gameObject);
        }
    }
}