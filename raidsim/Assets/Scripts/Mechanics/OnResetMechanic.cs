using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using dev.susybaka.raidsim.Core;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics 
{
    public class OnResetMechanic : FightMechanic
    {
        FightTimeline timeline;

        [Header("On Reset Mechanic")]
        public UnityEvent onReset;

        private Coroutine ieFetchFightTimeline;

        private void Awake()
        {
            SceneManager.sceneLoaded += OnLoad;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnLoad;
        }

        private void OnLoad(Scene scene, LoadSceneMode mode)
        {
            if (timeline != null)
            {
                timeline.onReset.RemoveListener(OnReset);
                timeline = null;
            }
            if (ieFetchFightTimeline == null)
                ieFetchFightTimeline = StartCoroutine(IE_FetchFightTimeline(new WaitForSecondsRealtime(1f)));
        }

        private IEnumerator IE_FetchFightTimeline(WaitForSecondsRealtime wait)
        {
            yield return wait;
            timeline = FightTimeline.Instance;
            if (log && timeline != null)
                Debug.Log($"[OnResetMechanic] Current FightTimeline.Instance found and located inside scene: '{timeline.gameObject.scene.name}'");
            else if (log)
                Debug.Log("[OnResetMechanic] Current FightTimeline.Instance not found! Mechanic can not run inside this scene.");
            OnLoadFinished();
            ieFetchFightTimeline = null;
        }

        private void OnLoadFinished()
        {
            if (timeline != null)
            {
                timeline.onReset.AddListener(OnReset);
            }
        }

        public void OnReset()
        {
            if (log)
            {
                if (!string.IsNullOrEmpty(mechanicName))
                    Debug.Log($"[OnResetMechanic] Event for '{mechanicName}' triggered.");
                else
                    Debug.Log($"[OnResetMechanic] Event for 'Unnamed Mechanic' triggered.");
            }
            onReset?.Invoke();
        }
    }
}