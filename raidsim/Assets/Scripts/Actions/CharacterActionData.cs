using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalStructs;

[CreateAssetMenu(fileName = "New Character Action", menuName = "FFXIV/New Character Action")]
public class CharacterActionData : ScriptableObject
{
    public enum ActionType { Spell, Weaponskill, Ability, Auto }

    [Header("Info")]
    public string actionName = "Unnamed Action";
    public ActionType actionType = ActionType.Spell;
    public Damage damage = new Damage(0, true);
    public bool isShield = false;
    public bool isHeal = false;
    public bool isTargeted = false;
    public bool topEnmity = false;
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
    public bool canBeSlideCast = true;
    public bool rollsGcd = true;
    public StatusEffectData buff;
    public bool dispelBuffInstead = false;
    public StatusEffectData debuff;
    public bool dispelDebuffInstead = false;
    public CharacterActionData comboAction;
    public string animationName = string.Empty;
    public bool playAnimationDirectly = false;

    void Reset()
    {
        enmity = damage.value;
    }
}