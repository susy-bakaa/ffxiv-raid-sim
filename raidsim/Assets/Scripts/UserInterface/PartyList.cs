using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static CharacterState;

public class PartyList : MonoBehaviour
{
    public List<PartyMember> members = new List<PartyMember>();
    public string spriteAsset = "letters_1";
    public int maxLetters = 7;
    public bool assignLetters = false;
    public List<Role> priorityOrder = new List<Role>();

    private List<TextMeshProUGUI> names = new List<TextMeshProUGUI>();
    private HudElementPriority hudPriority;

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
        hudPriority = GetComponent<HudElementPriority>();

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
                if (members[i].characterState == null)
                    continue;

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

    public List<EnmityInfo> GetEnmityValuesList(CharacterState towards)
    {
        if (members == null || members.Count == 0 || towards == null)
        {
            return new List<EnmityInfo>(); // Return an empty list if there are no members
        }

        // Create a list of EnmityInfo
        List<EnmityInfo> enmityInfoList = new List<EnmityInfo>();

        for (int i = 0; i < members.Count; i++)
        {
            CharacterState memberState = members[i].characterState;
            long enmityValue = memberState.enmity.TryGetValue(towards, out long value) ? value : 0;

            // Add the EnmityInfo struct to the list
            enmityInfoList.Add(new EnmityInfo(memberState.characterName, memberState, towards, (int)enmityValue));
        }

        // Sort the list based on the enmity value in descending order
        enmityInfoList.Sort((a, b) => b.enmity.CompareTo(a.enmity));

        return enmityInfoList;
    }

    public List<CharacterState> GetEnmityList(CharacterState towards)
    {
        if (members == null || members.Count == 0 || towards == null)
        {
            return new List<CharacterState>(); // Return an empty list if there are no members
        }

        // Create a copy of the members list
        List<CharacterState> sortedMembers = new List<CharacterState>();
        sortedMembers.Clear();

        for (int i = 0; i < members.Count; i++)
        {
            if (members[i].characterState == null)
                continue;

            sortedMembers.Add(members[i].characterState);
        }

        if (sortedMembers == null || sortedMembers.Count < 1)
        {
            return new List<CharacterState>();
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

    public bool HasAnyEnmity(CharacterState towards)
    {
        if (members == null || members.Count == 0)
        {
            return false; // or handle the case where there are no members
        }

        List<CharacterState> enmityList = GetEnmityList(towards);

        if (enmityList == null || enmityList.Count < 1)
            return false;

        if (enmityList[0].enmity.TryGetValue(towards, out long enmity))
        {
            if (enmity > 0)
                return true;
        }

        return false;
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

    public PartyMember? GetMember(CharacterState characterState)
    {
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i].characterState == characterState && members[i].characterState.characterName == characterState.characterName)
                return members[i];
        }

        return null;
    }

    public List<PartyMember> GetPrioritySortedList(StatusEffectData statusEffectData)
    {
        List<PartyMember> sortedMembers = new List<PartyMember>();

        for (int i = 0; i < members.Count; i++)
        {
            if (members[i].characterState.HasAnyVersionOfEffect(statusEffectData.statusName))
            {
                sortedMembers.Add(members[i]);
            }
        }

        if (priorityOrder.Count > 0)
        {
            sortedMembers.Sort((a, b) => priorityOrder.IndexOf(a.role).CompareTo(priorityOrder.IndexOf(b.role)));
        }

        return sortedMembers;
    }

    public void UpdatePartyList()
    {
        int i_active = 0;
        for (int i = 0; i < members.Count; i++)
        {
            PartyMember member = members[i];

            if (member.characterState == null)
                continue;

            //Debug.Log($"update partylist {gameObject.name}");

            int letter = i_active;
            if (letter > maxLetters)
                letter = maxLetters;
            else if (letter < 0)
                letter = 0;
            members[i].characterState.characterLetter = letter;
            member.letter = members[i].characterState.characterLetter;
            member.role = member.characterState.role;
            members[i].hudElement.gameObject.SetActive(members[i].characterState.gameObject.activeSelf);
            members[i] = member;
            if (members[i].hudElement != null)
                members[i].hudElement.characterState = members[i].characterState;

            if (members[i].characterState.gameObject.activeSelf)
            {
                i_active++;
            }
        }

        UpdatePrioritySorting();
    }

    public void UpdatePrioritySorting()
    {
        if (hudPriority != null)
            hudPriority.UpdateSorting();
    }

    [System.Serializable]
    public struct PartyMember
    {
        public string name;
        public PlayerController playerController;
        public AIController aiController;
        public BossController bossController;
        public CharacterState characterState;
        public ActionController actionController;
        public TargetController targetController;
        public HudElement hudElement;
        public int letter;
        public CharacterState.Role role;

        public PartyMember(string name, PlayerController playerController, AIController aiController, BossController bossController, CharacterState characterState, ActionController actionController, TargetController targetController, HudElement hudElement, int letter)
        {
            this.name = name;
            this.playerController = playerController;
            this.aiController = aiController;
            this.bossController = bossController;
            this.characterState = characterState;
            this.actionController = actionController;
            this.targetController = targetController;
            this.hudElement = hudElement;
            this.letter = letter;
            if (characterState != null)
                role = characterState.role;
            else
                role = CharacterState.Role.unassigned;
        }
    }

    [System.Serializable]
    public struct EnmityInfo
    {
        public string name;
        public CharacterState state;
        public CharacterState towards;
        public int enmity;

        public EnmityInfo(string name, CharacterState state, CharacterState towards, int enmity)
        {
            this.name = name;
            this.state = state;
            this.towards = towards;
            this.enmity = enmity;
        }
    }
}
