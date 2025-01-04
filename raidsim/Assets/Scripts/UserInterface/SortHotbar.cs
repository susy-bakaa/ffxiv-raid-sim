using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalData;

public class SortHotbar : MonoBehaviour
{
    private CharacterState character;
    private List<CharacterAction> actions = new List<CharacterAction>();
    private Coroutine ieUpdateSortingDelayed;

    void Start()
    {
        actions.Clear();
        foreach (Transform child in transform)
        {
            CharacterAction action = child.GetComponent<CharacterAction>();
            if (action != null)
            {
                actions.Add(action);
            }
        }

        if (actions != null && actions.Count > 0)
            character = actions[0].GetCharacter();

        UpdateSorting();
    }

    public void UpdateSorting()
    {
        UpdateSortingInternal();
        if (ieUpdateSortingDelayed == null)
            ieUpdateSortingDelayed = StartCoroutine(IE_UpdateSortingDelayed(new WaitForSecondsRealtime(0.05f)));
    }

    private IEnumerator IE_UpdateSortingDelayed(WaitForSecondsRealtime wait)
    {
        yield return wait;
        UpdateSortingInternal();
        ieUpdateSortingDelayed = null;
    }

    private void UpdateSortingInternal()
    {
        if (actions != null && actions.Count > 0)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].unavailable)
                    actions[i].gameObject.SetActive(false);
                else
                    actions[i].gameObject.SetActive(true);

                foreach (Role role in actions[i].availableForRoles)
                {
                    if (role == character.role)
                    {
                        actions[i].gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }
    }
}
