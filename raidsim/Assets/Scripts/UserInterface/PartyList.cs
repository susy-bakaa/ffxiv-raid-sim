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
