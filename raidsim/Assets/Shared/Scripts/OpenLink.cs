using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace dev.susybaka.Shared
{
    public class OpenLink : MonoBehaviour
    {
        [SerializeField] private string link = string.Empty;

        private Button button;
        private Coroutine ieOpenDelay;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void Open()
        {
            if (ieOpenDelay == null)
            {
                ieOpenDelay = StartCoroutine(IE_OpenDelay(new WaitForSecondsRealtime(0.5f)));
                button.interactable = false;
            }
        }

        private IEnumerator IE_OpenDelay(WaitForSecondsRealtime wait)
        {
            yield return wait;
            OpenInternal();
            button.interactable = true;
            ieOpenDelay = null;
        }

        private void OpenInternal()
        {
            if (string.IsNullOrEmpty(link))
            {
                Debug.LogWarning("Link is not set.");
                return;
            }

            Application.OpenURL(link);
        }
    }
}