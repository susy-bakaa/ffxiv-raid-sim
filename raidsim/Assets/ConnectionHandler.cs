using System;
using System.Collections;
using dev.susybaka.raidsim.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace dev.susybaka.raidsim.Core
{
    public class ConnectionHandler : MonoBehaviour
    {
        public static ConnectionHandler Instance;

        public HudWindow disconnectPopup;
        public bool connectedToServer = true;
        public int timeoutSeconds = 5;

        public string pingUrl = "https://assets.susybaka.dev/ping";
        private Coroutine ieCheckConnection;
        private bool hasDisconnected = false;

        private void Awake()
        {
#if UNITY_WEBPLAYER
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            if (disconnectPopup != null)
                disconnectPopup.CloseWindow();
#else
            Debug.Log("Not a WebGL build -> ConnectionHandler was destroyed!");
            Destroy(gameObject);
            return;
#endif
        }

        public void Start()
        {
            connectedToServer = true;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += CheckConnectionOnSceneLoad;
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= CheckConnectionOnSceneLoad;
        }

        private void CheckConnectionOnSceneLoad(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            CheckConnection();
        }

        public void CheckConnection()
        {
            if (ieCheckConnection == null)
            {
                ieCheckConnection = StartCoroutine(IE_CheckServerConnection((success) =>
                {
                    connectedToServer = success;
                }));
            }

            if (!hasDisconnected && !connectedToServer)
            {
                hasDisconnected = true;
                if (disconnectPopup != null)
                    disconnectPopup.OpenWindow();
            }
        }

        private IEnumerator IE_CheckServerConnection(Action<bool> onComplete)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(pingUrl))
            {
                request.timeout = timeoutSeconds;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning($"[ConnectionHandler] Connection check to remote server failed: {request.error}");
                    onComplete?.Invoke(false);
                }
                else
                {
                    Debug.Log("[ConnectionHandler] Connection check to remote server succeeded!");
                    onComplete?.Invoke(true);
                }
                ieCheckConnection = null;
            }
        }
    }
}