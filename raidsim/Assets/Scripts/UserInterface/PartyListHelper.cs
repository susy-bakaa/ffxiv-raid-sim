using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PartyList;

[RequireComponent(typeof(PartyList))]
public class PartyListHelper : MonoBehaviour
{
    private PartyList party;
    public PartyList PartyList { get { return party; } }

    public int updateEnmityList = 130;
    public TargetController player;

    private List<EnmityInfo> enmityAgainstPlayerTarget = new List<EnmityInfo>();

    void Awake()
    {
        party = GetComponent<PartyList>();
        if (player == null)
        {
            for (int i = 0; i < party.members.Count; i++)
            {
                if (party.members[i].characterState == null)
                    continue;

                if (party.members[i].characterState.characterName.ToLower().Contains("player"))
                {
                    player = party.members[i].targetController;
                }
            }
        }
    }

    void Update()
    {
        if (Utilities.RateLimiter(updateEnmityList))
        {
            if (player != null && player.currentTarget != null && player.allowedGroups.Contains(player.currentTarget.Group))
            {
                // Get the enmity list sorted by enmity values (highest first)
                enmityAgainstPlayerTarget = party.GetEnmityValuesList(player.currentTarget.GetCharacterState());
            }
            else
            {
                enmityAgainstPlayerTarget.Clear();
            }
        }
    }

    public List<EnmityInfo> GetCurrentPlayerTargetEnmityList()
    {
        return enmityAgainstPlayerTarget;
    }
}
