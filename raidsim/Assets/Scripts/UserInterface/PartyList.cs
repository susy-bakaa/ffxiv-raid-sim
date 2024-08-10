using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyList : MonoBehaviour
{
    public List<PartyMember> members = new List<PartyMember>();
    public string spriteAsset = "letters_1";
    public bool assignLetters = false;

    private List<TextMeshProUGUI> names = new List<TextMeshProUGUI>();

#if UNITY_EDITOR
    void OnValidate()
    {
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i].characterState != null)
            {
                PartyMember temp = members[i];

                temp.name = temp.characterState.characterName;

                members[i] = temp;
            }
        }
    }
#endif

    void Awake()
    {
        names.Clear();
        for (int i = 0; i < members.Count; i++)
        {
            names.Add(members[i].hudElement.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>());
        }
    }

    void Start()
    {
        if (assignLetters)
        {
            for (int i = 0;i < members.Count; i++)
            {
                members[i].characterState.characterLetter = members[i].letter;
            }
        }
    }

    public List<CharacterState> GetActiveMembers()
    {
        List<CharacterState> m = new List<CharacterState>();

        for (int i = 0; i < members.Count; i++)
        {
            if (members[i].characterState.gameObject.activeSelf)
            {
                m.Add(members[i].characterState);
            }
        }

        return m;
    }

    public bool HasDeadMembers()
    {
        for (int i = 0;i < members.Count;i++)
        {
            if (members[i].characterState.dead)
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
        List<CharacterState> shuffledMembers = new List<CharacterState>(members.Count);
        shuffledMembers.Clear();

        for (int i = 0; i < members.Count; i++)
        {
            shuffledMembers.Add(members[i].characterState);
        }

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
        List<CharacterState> shuffledMembers = new List<CharacterState>(members.Count);
        shuffledMembers.Clear();

        for (int i = 0; i < members.Count; i++)
        {
            shuffledMembers.Add(members[i].characterState);
        }

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

    public List<CharacterState> GetEnmityList(CharacterState towards)
    {
        if (members == null || members.Count == 0)
        {
            return new List<CharacterState>(); // Return an empty list if there are no members
        }

        // Create a copy of the members list
        List<CharacterState> sortedMembers = new List<CharacterState>();
        sortedMembers.Clear();

        for (int i = 0; i < members.Count; i++)
        {
            sortedMembers.Add(members[i].characterState);
        }

        // Sort the copy based on the enmity towards the specified CharacterState
        sortedMembers.Sort((a, b) =>
        {
            long enmityA = a.enmity.TryGetValue(towards, out long valueA) ? valueA : 0;
            long enmityB = b.enmity.TryGetValue(towards, out long valueB) ? valueB : 0;
            return enmityB.CompareTo(enmityA); // Sort in descending order
        });

        return sortedMembers;
    }

    public CharacterState GetHighestEnmityMember(CharacterState towards)
    {
        if (members == null || members.Count == 0)
        {
            return null; // or handle the case where there are no members
        }

        return GetEnmityList(towards)[0];
    }

    public CharacterState GetLowestEnmityMember(CharacterState towards)
    {
        if (members == null || members.Count == 0)
        {
            return null; // or handle the case where there are no members
        }

        List<CharacterState> enmityList = GetEnmityList(towards);

        return enmityList[enmityList.Count - 1];
    }

    public bool HasCharacterState(CharacterState characterState)
    {
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i].characterState == characterState)
                return true;
        }

        return false;
    }

    public void UpdatePartyList()
    {
        for (int i = 0; i < members.Count; i++)
        {
            members[i].hudElement.gameObject.SetActive(members[i].characterState.gameObject.activeSelf);
        }
    }

    [System.Serializable]
    public struct PartyMember
    {
        public string name;
        public CharacterState characterState;
        public ActionController actionController;
        public TargetController targetController;
        public HudElement hudElement;
        public int letter;

        public PartyMember(string name, CharacterState characterState, ActionController actionController, TargetController targetController, HudElement hudElement, int letter)
        {
            this.name = name;
            this.characterState = characterState;
            this.actionController = actionController;
            this.targetController = targetController;
            this.hudElement = hudElement;
            this.letter = letter;
        }
    }
}
