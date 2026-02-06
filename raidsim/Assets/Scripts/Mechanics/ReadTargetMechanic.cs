using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class ReadTargetMechanic : FightMechanic
    {
        [Header("Read Target Settings")]
        public List<ScanData> targetDataList = new List<ScanData>();

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;
            
            if (actionInfo.target != null)
            {
                if (log)
                    Debug.Log($"[ReadTargetMechanic ({gameObject.name})] Reading target: {actionInfo.target.characterName} ({actionInfo.target.gameObject.name})");

                foreach (ScanData data in targetDataList)
                {
                    if (!string.IsNullOrEmpty(data.effect.name) && data.effect.data != null)
                    {
                        if (actionInfo.target.HasEffect(data.effect.name))
                        {
                            if (log)
                                Debug.Log($"[ReadTargetMechanic ({gameObject.name})] Found matching status effect: {data.effect.name} on target: {actionInfo.target.characterName} ({actionInfo.target.gameObject.name}), event triggered for '{data.name}'.");
                            
                            data.onFound.Invoke(actionInfo);
                        }
                    }
                }
            }
            else
            {
                if (log)
                    Debug.Log($"[ReadTargetMechanic ({gameObject.name})] No target available in ActionInfo.");
            }
        }

        [System.Serializable]
        public struct ScanData
        {
            public string name;
            public StatusEffectInfo effect;
            public UnityEvent<ActionInfo> onFound;
        }
    }
}