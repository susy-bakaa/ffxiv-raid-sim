using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyList : MonoBehaviour
{
    public List<CharacterState> members = new List<CharacterState>();
    public List<HudElement> hudElements = new List<HudElement>();

    public List<CharacterState> GetActiveMembers()
    {
        List<CharacterState> m = new List<CharacterState>();

        for (int i = 0; i < members.Count; i++)
        {
            if (members[i].gameObject.activeSelf)
            {
                m.Add(members[i]);
            }
        }

        return m;
    }

    public bool HasDeadMembers()
    {
        for (int i = 0;i < members.Count;i++)
        {
            if (members[i].dead)
                return true;
        }
        return false;
    }

    public CharacterState GetLowestHealthMember()
    {
        if (members == null || members.Count == 0)
        {
            return null; // or handle the case where there are no members
        }

        // Create a copy of the members list
        List<CharacterState> shuffledMembers = new List<CharacterState>(members);

        // Randomize the copy
        shuffledMembers.Shuffle();

        // Find the member with the lowest health in the shuffled list
        CharacterState lowestHealthMember = shuffledMembers[0];

        for (int i = 1; i < shuffledMembers.Count; i++)
        {
            if (shuffledMembers[i].health < lowestHealthMember.health)
            {
                lowestHealthMember = shuffledMembers[i];
            }
        }

        return lowestHealthMember;
    }

    public CharacterState GetHighestHealthMember()
    {
        if (members == null || members.Count == 0)
        {
            return null; // or handle the case where there are no members
        }

        // Create a copy of the members list
        List<CharacterState> shuffledMembers = new List<CharacterState>(members);

        // Randomize the copy
        shuffledMembers.Shuffle();

        // Find the member with the highest health in the shuffled list
        CharacterState highestHealthMember = shuffledMembers[0];

        for (int i = 1; i < shuffledMembers.Count; i++)
        {
            if (shuffledMembers[i].health > highestHealthMember.health)
            {
                highestHealthMember = shuffledMembers[i];
            }
        }

        return highestHealthMember;
    }

    public void UpdatePartyList()
    {
        if (members.Count != hudElements.Count)
        {
            Debug.LogError($"There are more party members ({members.Count}) than hud elements ({hudElements.Count})!");
            return;
        }

        for (int i = 0; i < members.Count; i++)
        {
            hudElements[i].gameObject.SetActive(members[i].gameObject.activeSelf);
        }
    }
}
