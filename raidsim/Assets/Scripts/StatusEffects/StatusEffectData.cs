using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Status Effect", menuName = "FFXIV/New Status Effect")]
public class StatusEffectData : ScriptableObject
{
    [Header("Info")]
    public string statusName = "Unnamed Status Effect";
    public bool negative = true;
    public bool unique = false;
    public float length = 10f;
    public float maxLength = 10f;
    public int appliedStacks = 1;
    public int maxStacks = 1;
    public GameObject statusEffect;
    public GameObject hudElement;
    public List<StatusEffectData> incompatableStatusEffects = new List<StatusEffectData>();
    public List<CharacterState.Role> assignedRoles = new List<CharacterState.Role>();
}
