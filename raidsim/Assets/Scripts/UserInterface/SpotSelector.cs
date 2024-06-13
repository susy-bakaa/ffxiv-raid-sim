using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpotSelector : MonoBehaviour
{
    TMP_Dropdown dropdown;
    
    public PartyList partyList;
    public PlayerController player;
    public AIController[] bots;
    public BotNode[] spots;

    void Awake()
    {
        Select(0);
    }

    void Start()
    {
        dropdown = GetComponentInChildren<TMP_Dropdown>();
    }

    void Update()
    {
        dropdown.interactable = !FightTimeline.Instance.playing;
    }

    public void Select(int value)
    {
        player.clockSpot = spots[value];

        int botNameIndex = 0; 

        for (int i = 0; i < bots.Length; i++)
        {
            if (bots[i].clockSpot == player.clockSpot)
            {
                bots[i].state.characterName = "Disabled";
                bots[i].gameObject.SetActive(false);
            }
            else
            {
                bots[i].state.characterName = $"AI{botNameIndex + 1}";
                bots[i].gameObject.SetActive(true);
                botNameIndex++;
            }
        }

        partyList.UpdatePartyList();
    }
}
