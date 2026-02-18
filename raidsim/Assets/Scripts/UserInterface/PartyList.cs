// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using NaughtyAttributes;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.StatusEffects;
using dev.susybaka.raidsim.Targeting;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.UI
{
    public class PartyList : MonoBehaviour
    {
        public List<PartyMember> members = new List<PartyMember>();
        public GameObject partyMemberHudPrefab;
        public string spriteAsset = "letters_1";
        public int maxLetters = 7;
        public bool assignLetters = false;
        public bool assignRandomOrder = false;
        public bool assignIconsAutomatically = true;
        [HideIf("assignIconsAutomatically")] public int assignIcon = 6;
        public List<Role> priorityOrder = new List<Role>();

        private List<TextMeshProUGUI> names = new List<TextMeshProUGUI>();
        private HudElementPriority hudPriority;
        private bool originalMembersSet = false;
        private List<PartyMember> originalMembers;
        private CharacterState playerState;
        private TargetController playerTargeting;
        private PartyMember player;
        private bool setupDone = false;
        public bool SetupDone { get { return setupDone; } }
        private readonly List<CharacterState> _shuffledMembers = new List<CharacterState>();
        private readonly List<EnmityInfo> _enmityInfoList = new List<EnmityInfo>();

#if UNITY_EDITOR
        // Redundant now with the changes to partylist hud elements?
        /*[Button("Relink Party Member Hud Variables")]
        public void RefreshPartyMemberVariables()
        {
            for (int i = 0; i < members.Count; i++)
            {
                RelinkCharacterStateToHudElement(i);
            }
        }*/

        private void OnValidate()
        {
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].characterState != null)
                {
                    PartyMember temp = members[i];

                    temp.name = temp.characterState.characterName;
                    temp.sector = temp.characterState.sector;

                    members[i] = temp;
                }
            }
        }
#endif

        private void Awake()
        {
            hudPriority = GetComponent<HudElementPriority>();

            if (playerState == null)
                playerState = Utilities.FindAnyByName("Player").GetComponent<CharacterState>();
            if (playerState != null)
                playerTargeting = playerState.GetComponent<TargetController>();

            if (partyMemberHudPrefab != null)
            {
                setupDone = false;
                SpawnPartyListHudElements();
            }
            else
                setupDone = true;

            names.Clear();
            for (int i = 0; i < members.Count; i++)
            {
                names.Add(members[i].helper.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>());
            }
        }

        private void Start()
        {
            if (members == null || members.Count <= 0)
                return;

            if (playerState != null)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    if (members[i].characterState == playerState)
                    {
                        player = members[i];
                        break;
                    }
                }
            }

            if (assignLetters)
            {
                int assignedLetter = 0;
                for (int i = 0; i < members.Count; i++)
                {
                    if (members[i].characterState == null)
                        continue;

                    PartyMember member = members[i];
                    member.letter = assignedLetter;
                    member.characterState.characterLetter = assignedLetter;
                    member.characterState.letterSpriteAsset = spriteAsset;
                    members[i] = member;
                    assignedLetter++;
                    if (assignedLetter > maxLetters)
                        assignedLetter = maxLetters;

                    //Debug.Log($"member {member.name} index {i} letter {member.letter} char.letter {member.characterState.characterLetter} assignedLetter {assignedLetter}");
                }
            }
            if (assignRandomOrder)
            {
                RandomizePartyListOrder();
            }
        }

        private void SpawnPartyListHudElements()
        {
            if (transform.childCount > 0)
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }

            for (int i = 0; i < members.Count; i++)
            {
                PartyMemberHelper memberHelper = Instantiate(partyMemberHudPrefab, transform).GetComponent<PartyMemberHelper>();
                int index = i;
                PartyMember member = members[i];
                
                memberHelper.name = $"{member.characterState.gameObject.name}_Hud";
                memberHelper.HudElement.priority = member.letter;
                memberHelper.HudElement.characterState = member.characterState;
                if (memberHelper.HudElement.targetButton != null && playerTargeting != null)
                    memberHelper.HudElement.targetButton.onClick.AddListener(() => SetPlayerTargetToPartyMember(index));
                member.helper = memberHelper;
                members[i] = member;
                RelinkCharacterStateToHudElement(index);
            }
            setupDone = true;
            UpdatePartyList();
        }

        private void RelinkCharacterStateToHudElement(int index)
        {
            if (members[index].characterState != null && members[index].helper != null && members[index].actionController != null)
            {
                members[index].characterState.characterNameTextParty = members[index].helper.NameText;
                members[index].characterState.healthBarParty = members[index].helper.HealthBarSlider;
                members[index].characterState.overShieldBarParty = members[index].helper.OverShieldBarSlider;
                members[index].characterState.shieldBarParty = members[index].helper.ShieldBarSlider;
                members[index].characterState.healthBarTextParty = members[index].helper.HealthBarText;
                members[index].characterState.statusEffectIconParentParty = members[index].helper.StatusEffectHolder;
                members[index].actionController.castBarParty = members[index].helper.CastBarSlider;
                members[index].actionController.castNameTextParty = members[index].helper.CastNameText;
                if (members[index].helper.PartyIcon != null)
                {
                    members[index].helper.PartyIcon.member = members[index].characterState;
                    members[index].helper.PartyIcon.chooseAutomatically = assignIconsAutomatically;
                    if (!assignIconsAutomatically)
                        members[index].helper.PartyIcon.icon = assignIcon;
                }
                TargetNode node = members[index].characterState.GetComponentInChildren<TargetNode>();
                if (node != null)
                {
                    node.highlightGroups = new CanvasGroup[1];
                    node.highlightGroups[0] = members[index].helper.transform.GetChild(0).Find("Highlight").GetComponent<CanvasGroup>();
                }
                
                if (!Application.isPlaying)
                    return;

                members[index].characterState.RefreshUserInterface();
                members[index].actionController.RefreshUserInterface();
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
            for (int i = 0; i < members.Count; i++)
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

            // Create a copy of the members list that consists of CharacterState objects
            _shuffledMembers.Clear();
            if (_shuffledMembers.Capacity < members.Count)
                _shuffledMembers.Capacity = members.Count;

            for (int i = 0; i < members.Count; i++)
            {
                _shuffledMembers.Add(members[i].characterState);
            }

            // Randomize the copy
            _shuffledMembers.ShufflePCG(FightTimeline.Instance.random.Stream("PartyList_GetLowestHealthMember_Shuffle_PartyMembers"));

            // Find the member with the lowest health in the shuffled list
            CharacterState lowestHealthMember = _shuffledMembers[0];

            for (int i = 1; i < _shuffledMembers.Count; i++)
            {
                if (_shuffledMembers[i].health < lowestHealthMember.health)
                {
                    lowestHealthMember = _shuffledMembers[i];
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

            // Create a copy of the members list that consists of CharacterState objects
            _shuffledMembers.Clear();
            if (_shuffledMembers.Capacity < members.Count)
                _shuffledMembers.Capacity = members.Count;

            for (int i = 0; i < members.Count; i++)
            {
                _shuffledMembers.Add(members[i].characterState);
            }

            // Randomize the copy
            _shuffledMembers.ShufflePCG(FightTimeline.Instance.random.Stream("PartyList_GetHighestHealthMember_Shuffle_PartyMembers"));

            // Find the member with the highest health in the shuffled list
            CharacterState highestHealthMember = _shuffledMembers[0];

            for (int i = 1; i < _shuffledMembers.Count; i++)
            {
                if (_shuffledMembers[i].health > highestHealthMember.health)
                {
                    highestHealthMember = _shuffledMembers[i];
                }
            }

            return highestHealthMember;
        }

        public CharacterState GetNearestMember(Vector3 position)
        {
            if (members == null || members.Count == 0)
            {
                return null; // or handle the case where there are no members
            }

            CharacterState nearestMember = null;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].characterState == null || !members[i].characterState.gameObject.activeSelf || members[i].characterState.disabled)
                    continue;

                CharacterState memberState = members[i].characterState;
                float distance = Vector3.Distance(memberState.transform.position, position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestMember = memberState;
                }
            }
            return nearestMember;
        }

        public CharacterState GetFurthestMember(Vector3 position)
        {
            if (members == null || members.Count == 0)
            {
                return null; // or handle the case where there are no members
            }
            CharacterState furthestMember = null;
            float furthestDistance = float.MinValue;
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].characterState == null || !members[i].characterState.gameObject.activeSelf || members[i].characterState.disabled)
                    continue;

                CharacterState memberState = members[i].characterState;
                float distance = Vector3.Distance(memberState.transform.position, position);
                if (distance > furthestDistance)
                {
                    furthestDistance = distance;
                    furthestMember = memberState;
                }
            }
            return furthestMember;
        }

        public List<EnmityInfo> GetEnmityValuesList(CharacterState towards)
        {
            if (members == null || members.Count == 0 || towards == null)
            {
                return new List<EnmityInfo>(); // Return an empty list if there are no members
            }

            // Clear the EnmityInfo list and set its capacity to the number of members
            _enmityInfoList.Clear();
            _enmityInfoList.Capacity = members.Count;

            for (int i = 0; i < members.Count; i++)
            {
                CharacterState memberState = members[i].characterState;
                long enmityValue = memberState.enmity.TryGetValue(towards, out long value) ? value : 0;

                // Add the EnmityInfo struct to the list
                _enmityInfoList.Add(new EnmityInfo(memberState.characterName, memberState, towards, (int)enmityValue));
            }

            // Sort the list based on the enmity value in descending order
            _enmityInfoList.Sort((a, b) => b.enmity.CompareTo(a.enmity));

            return _enmityInfoList;
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
            if (members == null || members.Count <= 0)
                return;

            int letterIndex = 0;
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].characterState == null)
                    continue;

                PartyMember member = members[i];

                int letter = letterIndex;
                if (letter > maxLetters)
                    letter = maxLetters;
                else if (letter < 0)
                    letter = 0;
                members[i].characterState.characterLetter = letter;
                member.letter = members[i].characterState.characterLetter;
                member.role = member.characterState.role;
                member.sector = member.characterState.sector;
                if (assignLetters)
                    member.helper.HudElement.priority = letter;
                members[i].helper.HudElement.gameObject.SetActive(members[i].characterState.gameObject.activeSelf);
                if (members[i].helper.HudElement != null)
                    members[i].helper.HudElement.characterState = members[i].characterState;
                member.helper.PartyIcon?.UpdateIcon();
                members[i] = member;

                if (members[i].characterState.gameObject.activeSelf)
                {
                    letterIndex++;
                }
                else if (playerState != null && !string.IsNullOrEmpty(player.name))
                {
                    playerState.sector = members[i].sector;
                    PartyMember p = player;
                    p.sector = members[i].sector;
                    player = p;
                }
            }

            UpdatePrioritySorting();
        }

        public void UpdatePrioritySorting()
        {
            if (hudPriority != null)
                hudPriority.UpdateSorting();
        }

        public void RandomizePartyListOrder()
        {
            List<int> availableLetters = new List<int>();

            if (!originalMembersSet)
            {
                originalMembers = new List<PartyMember>(members);
                originalMembersSet = true;
            }

            for (int i = 0; i < maxLetters; i++)
            {
                availableLetters.Add(i);
            }

            for (int i = 0; i < members.Count; i++)
            {
                PartyMember member = members[i];

                if (availableLetters.Count > 0)
                {
                    int randomIndex = Random.Range(0, availableLetters.Count);
                    member.letter = availableLetters[randomIndex];
                    availableLetters.RemoveAt(randomIndex);
                }
                else
                {
                    member.letter = maxLetters;
                }

                members[i] = member;
            }

            // Sort members based on their letter
            members.Sort((a, b) => a.letter.CompareTo(b.letter));

            UpdatePartyList();
        }

        public void ResetPartyListOrder()
        {
            if (originalMembersSet && originalMembers != null && originalMembers.Count > 0)
            {
                members = new List<PartyMember>(originalMembers);
                UpdatePartyList();
            }
        }

        public void SetPlayerTargetToPartyMember(int index)
        {
            if (playerTargeting == null)
                return;
            if (members == null || members.Count < 1 || index >= members.Count || index < 0)
                return;
            if (members[index].targetController == null)
                return;

            playerTargeting.SetTarget(members[index].targetController.self);
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
            public PartyMemberHelper helper;
            public int letter;
            public Role role;
            public Sector sector;

            public PartyMember(string name, PlayerController playerController, AIController aiController, BossController bossController, CharacterState characterState, ActionController actionController, TargetController targetController, PartyMemberHelper helper, int letter)
            {
                this.name = name;
                this.playerController = playerController;
                this.aiController = aiController;
                this.bossController = bossController;
                this.characterState = characterState;
                this.actionController = actionController;
                this.targetController = targetController;
                this.helper = helper;
                this.letter = letter;
                if (characterState != null)
                    role = characterState.role;
                else
                    role = Role.unassigned;
                if (characterState != null)
                    sector = characterState.sector;
                else
                    sector = Sector.N;
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

}