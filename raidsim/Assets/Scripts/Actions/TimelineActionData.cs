using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Timeline Action", menuName = "FFXIV/New Timeline Action")]
public class TimelineActionData : ScriptableObject
{
    [Header("Information")]
    public string actionName = "Unnamed Timeline Action";
    [Header("Actor")]
    public int characterIndex = -1;
    public CharacterActionData makeCharacterPerformAction;
    [Header("Targets")]
    public int[] effectedCharacterIndexes;
    public int damageToCharacters = 0;
    public StatusEffectData[] giveDebuffs;
    public bool randomPlayers = true;
}
