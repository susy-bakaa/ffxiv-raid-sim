using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GlobalData;
using static PartyList;

public class RoleSelector : MonoBehaviour
{
    TMP_Dropdown dropdown;

    public PartyList partyList;
    public bool autoObtainPlayer;
    public PartyMember player;
    public PartyMember[] bots;
    public BotNode[] spots;
    public List<RoleSelectorPair> roleSelectorPairs = new List<RoleSelectorPair>();
    private int lastSelected = 0;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (autoObtainPlayer && partyList != null && partyList.members != null && partyList.members.Count > 0)
        {
            player = partyList.members[0];
        }
        if (roleSelectorPairs != null && roleSelectorPairs.Count > 0)
        {
            for (int i = 0; i < roleSelectorPairs.Count; i++)
            {
                RoleSelectorPair temp = roleSelectorPairs[i];

                if (roleSelectorPairs[i].gameObject != null)
                    temp.name = roleSelectorPairs[i].gameObject.name;

                roleSelectorPairs[i] = temp;
            }
        }
    }
#endif

    void Awake()
    {
        if (autoObtainPlayer && partyList != null && partyList.members != null && partyList.members.Count > 0)
        {
            player = partyList.members[0];
        }
    }

    void Start()
    {
        dropdown = GetComponentInChildren<TMP_Dropdown>();
        Select(0);
        if (FightTimeline.Instance != null)
            FightTimeline.Instance.onReset.AddListener(() => Select(lastSelected));
    }

    void Update()
    {
        dropdown.interactable = !FightTimeline.Instance.playing;
    }

    public void Select(int value)
    {
        if (player.playerController == null)
            Debug.LogError("PlayerController is unassigned!");

        player.characterState.role = (Role)value;

        int botNameIndex = 0;
        bool botDisabled = false;

        for (int i = 0; i < bots.Length; i++)
        {
            if (bots[i].characterState.role == player.characterState.role && !botDisabled)
            {
                botDisabled = true;
                bots[i].characterState.characterName = "Disabled";
                bots[i].characterState.gameObject.SetActive(false);
            }
            else
            {
                bots[i].characterState.characterName = $"AI{botNameIndex + 1}";
                bots[i].characterState.gameObject.SetActive(true);
                botNameIndex++;
            }
        }

        if (roleSelectorPairs != null && roleSelectorPairs.Count > 0)
        {
            for (int i = 0; i < roleSelectorPairs.Count; i++)
            {
                if (roleSelectorPairs[i].roles.Contains((Role)value))
                {
                    roleSelectorPairs[i].gameObject.SetActive(true);
                }
                else
                {
                    roleSelectorPairs[i].gameObject.SetActive(false);
                }
            }
        }

        lastSelected = value;

        partyList.UpdatePartyList();
    }

    [System.Serializable]
    public struct RoleSelectorPair
    {
        public string name;
        public List<Role> roles;
        public GameObject gameObject;

        public RoleSelectorPair(string name, List<Role> role, GameObject gameObject)
        {
            this.name = name;
            this.roles = role;
            this.gameObject = gameObject;
        }
    }
}