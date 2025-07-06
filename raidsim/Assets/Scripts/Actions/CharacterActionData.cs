using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.StatusEffects;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Actions
{
    [CreateAssetMenu(fileName = "New Character Action", menuName = "FFXIV/New Character Action")]
    public class CharacterActionData : ScriptableObject
    {
        public enum ActionType { Spell, Weaponskill, Ability, Auto }

        [Header("Info")]
        public string actionName = "Unnamed Action";
        public ActionType actionType = ActionType.Spell;
        public Damage damage = new Damage(0, true);
        public bool causesDirectDamage = true;
        public bool isShield = false;
        public bool isHeal = false;
        public bool isTargeted = false;
        public bool isGroundTargeted = false;
        public bool topEnmity = false;
        public bool hasMovement = false;
        public int[] targetGroups = new int[1] { 100 };
        public int maxTargets = 1;
        public float range = 25f;
        public float radius = 30f;
        public int manaCost = 0;
        public long enmity = 0;
        public float damageEnmityMultiplier = 1f;
        public float cast = 2.0f;
        public float recast = 2.5f;
        public float animationLock = 0.6f;
        public int charges = 1;
        public bool canBeSlideCast = true;
        public bool rollsGcd = true;
        public StatusEffectData buff;
        public bool dispelBuffInstead = false;
        public StatusEffectData debuff;
        public bool dispelDebuffInstead = false;
        public CharacterActionData comboAction;
        public bool playCastingAnimationDirectly = false;
        [ShowIf("playCastingAnimationDirectly")] public string castingAnimationName = string.Empty;
        [ShowIf("playCastingAnimationDirectly")] [Min(0f)] public float castingAnimationCrossFade = 0.2f;
        public string animationName = string.Empty;
        public bool playAnimationDirectly = false;
        [ShowIf("playAnimationDirectly")] [Min(0f)] public float animationCrossFade = 0.2f;
        public bool playAnimationOnFinish = false;
        public int onAnimationFinishId = -1;
        [Min(0f)] public float animationDelay = 0f;
        public string speech = string.Empty;
        public AudioClip speechAudio = null;
        public AudioClip jonSpeechAudio = null;

        private void Awake()
        {
            damage = new Damage(damage, null, damage.name);
        }

        private void Reset()
        {
            enmity = damage.value;
            damage = new Damage(0, true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            damage = new Damage(damage, null, damage.name);
        }
#endif

        public string GetActionName()
        {
            string finalName = Utilities.InsertSpaceBeforeCapitals(actionName.Replace(" ", ""));
            finalName = Utilities.InsertSpaces(finalName, true);

            return finalName;
        }
    }
}