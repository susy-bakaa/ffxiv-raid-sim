using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
#if UNITY_WEBPLAYER
using System.Collections;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;
#endif

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ServerConnectionLabel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
#pragma warning disable 0414
        private TextMeshProUGUI text;
        private TextMeshProUGUI subText;
        private CanvasGroup subTextGroup;
        public string connectedText = "<sprite=\"online_status\" name=\"1\">";
        public string disconnectedText = "<sprite=\"online_status\" name=\"0\">";
        public string connectedSubText = "ONLINE";
        public string disconnectedSubText = "OFFLINE";

        private bool isConnected = true;
#pragma warning restore 0414
#if UNITY_WEBPLAYER
        private IEnumerator Start()
        {
            text = GetComponent<TextMeshProUGUI>();
            subText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            subTextGroup = subText.GetComponent<CanvasGroup>();

            yield return new WaitForSecondsRealtime(0.5f);

            RefreshConnection();
            OnPointerExit(null);
        }

        void Update()
        {
            if (ConnectionHandler.Instance == null)
                return;

            if (ConnectionHandler.Instance.connectedToServer != isConnected)
            {
                RefreshConnection();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            RefreshConnection();
            subTextGroup.LeanAlpha(1f, 0.2f).setIgnoreTimeScale(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            subTextGroup.LeanAlpha(0f, 0.2f).setIgnoreTimeScale(true);
        }

        public void RefreshConnection()
        {
            if (ConnectionHandler.Instance == null)
            {
                text.text = "<sprite=\"online_status\" name=\"2\">";
                subText.text = "";
                return;
            }

            ConnectionHandler.Instance.CheckConnection();

            // Delay the visual updates to give ConnectionHandler time to update its status.
            // This is a bit of an ugly way to do this and if your connection is REALLY slow,
            // it will technically fail to update the visuals correctly until you hover again,
            // but it works for almost all cases and I am too lazy to implement it differently.
            Utilities.FunctionTimer.Create(this, () =>
            {
                if (ConnectionHandler.Instance.connectedToServer)
                {
                    isConnected = true;
                    text.text = connectedText;
                    subText.text = connectedSubText;
                    subText.color = Color.green;
                }
                else
                {
                    isConnected = false;
                    text.text = disconnectedText;
                    subText.text = disconnectedSubText;
                    subText.color = Color.red;
                }
            }, 0.5f, "ServerConnectionLabel_UpdateDelay", true, true);
        }
#else
        public void OnPointerEnter(PointerEventData eventData) { }
        public void OnPointerExit(PointerEventData eventData) { }
#endif
    }
}