using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalStructs;

[CreateAssetMenu(fileName = "New Character Action", menuName = "FFXIV/New Character Action")]
public class CharacterActionData : ScriptableObject
{
    public enum ActionType { Spell, Weaponskill, Ability }

    [Header("Info")]
    public string actionName = "Unnamed Action";
    public ActionType actionType = ActionType.Spell;
    public Damage damage = new Damage(0, true);
    public int maxTargets = 1;
    public float range = 25f;
    public float radius = 30f;
    public int manaCost = 0;
    public float cast = 2.0f;
    public float recast = 2.5f;
    public float animationLock = 0.2f;
    public bool canBeSlideCast = true;
    public StatusEffectData buff;
    public StatusEffectData debuff;
    public string animationName = string.Empty;
}
