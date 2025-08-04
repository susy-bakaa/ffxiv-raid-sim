using System.Collections.Generic;
using UnityEngine;
using dev.susybaka.Shared.Attributes;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.StatusEffects
{
    [CreateAssetMenu(fileName = "New Status Effect", menuName = "FFXIV/New Status Effect")]
    public class StatusEffectData : ScriptableObject
    {
        [Header("Info")]
        public string statusName = "Unnamed Status Effect";
        public string statusDesc = "Who knows what this effect might do?";
        public bool negative = true;
        public bool unique = false;
        public bool infinite = false;
        public bool toggle = false;
        public bool rollsCooldown = true;
        public bool hidden = false;
        public bool lostOnDeath = true;
        public bool unaffectedByTimeScale = false;
        public bool instantCasts = false;
        public float reducesCastTimes = 0f;
        public float length = 10f;
        public float maxLength = 10f;
        public int appliedStacks = 1;
        public int maxStacks = 1;
        public GameObject statusEffect;
        public GameObject hudElement;
        public List<Sprite> icons = new List<Sprite>();
        public List<StatusEffectData> incompatableStatusEffects = new List<StatusEffectData>();
        public List<StatusEffectData> refreshStatusEffects = new List<StatusEffectData>();
        public List<Role> assignedRoles = new List<Role>();
        [SoundName] public string applySoundFx = "status_apply_positive";
        [SoundName] public string expireSoundFx = "status_expire_positive";

#if UNITY_EDITOR
        private void Reset()
        {
            if (hidden)
            {
                applySoundFx = "<None>";
                expireSoundFx = "<None>";
            }
        }
#endif

        [System.Serializable]
        public struct StatusEffectInfo
        {
            public string name;
            public StatusEffectData data;
            public int tag;
            public int stacks;

            public StatusEffectInfo(StatusEffectData effect, int tag, int stacks)
            {
                name = effect.statusName + "_" + tag;
                this.data = effect;
                this.tag = tag;
                this.stacks = stacks;
            }
        }

        [System.Serializable]
        public struct StatusEffectInfoArray
        {
            public string name;
            public StatusEffectInfo[] effectInfos;

            public StatusEffectInfoArray(StatusEffectInfo[] effectInfos)
            {
                if (effectInfos != null && effectInfos.Length > 0)
                    name = effectInfos[0].name;
                else
                    name = string.Empty;
                this.effectInfos = effectInfos;
            }
        }
    }
}