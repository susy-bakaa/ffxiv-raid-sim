using System.Collections.Generic;
using UnityEngine;

namespace dev.susybaka.Shared.Editor
{
    [System.Serializable]
    public class EventInfo
    {
        public float time = 0.0f;
        public string value;
    }

    [System.Serializable]
    public class ActionInfo
    {
        public string name;
        public List<EventInfo> eventList = new List<EventInfo>();
    }

    [CreateAssetMenu(fileName = "New Animation Event Data", menuName = "Wave/General/Animation Event Data")]
    public class AnimationEventData : ScriptableObject
    {
        public float fps = 30f;
        public List<ActionInfo> actionlist = new List<ActionInfo>();
    }
}