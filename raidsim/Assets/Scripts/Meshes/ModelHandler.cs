using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ModelHandler : MonoBehaviour
{
    public int currentModelIndex = 0;
    private int originalModelIndex = -1;
    public List<GameObject> models = new List<GameObject>();
    public UnityEvent<Animator> onCharacterModelSwapped;

    private CharacterState characterState;
    private Animator activeAnimator;

    private void Awake()
    {
        transform.TryGetComponentInParents(out characterState);

        originalModelIndex = currentModelIndex;

        for (int i = 0; i < transform.childCount; i++)
        {
            models.Add(transform.GetChild(i).gameObject);
        }

        if (FightTimeline.Instance != null)
        {
            FightTimeline.Instance.onReset.AddListener(ResetModelState);
        }
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < models.Count; i++)
        {
            if (models[i] == null)
                continue;

            models[i].SetActive(i == currentModelIndex);
            if (models[i].activeSelf)
            {
                models[i].TryGetComponent(out activeAnimator);
            }
        }
    }

    public void ResetModelState()
    {
        if (characterState != null)
            ResetCharacterModel();
        else
            ResetModel();
    }

    public void ResetCharacterModel()
    {
        for (int i = 0; i < models.Count; i++)
        {
            if (models[i] == null)
                continue;

            models[i].SetActive(i == originalModelIndex);
            if (models[i].activeSelf)
            {
                models[i].TryGetComponent(out activeAnimator);
            }
        }

        if (activeAnimator != null)
        {
            SetAnimator(activeAnimator);
            onCharacterModelSwapped.Invoke(activeAnimator);
        }
    }

    public void ResetModel()
    {
        for (int i = 0; i < models.Count; i++)
        {
            models[i].SetActive(i == originalModelIndex);
            if (models[i].activeSelf)
            {
                models[i].TryGetComponent(out activeAnimator);
            }
        }
    }

    public void UpdateModels()
    {
        models.Clear();
        Debug.Log($"UpdateModels {transform.childCount}");
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            models.Add(child.gameObject);
        }

        for (int i = 0; i < models.Count; i++)
        {
            models[i].SetActive(i == currentModelIndex);
            if (models[i].activeSelf)
            {
                models[i].TryGetComponent(out activeAnimator);
            }
        }

        if (activeAnimator != null)
        {
            SetAnimator(activeAnimator);
            onCharacterModelSwapped.Invoke(activeAnimator);
        }
    }

    public void SwitchActiveCharacterModel(int index)
    {
        if (index < 0 || index >= models.Count)
            return;

        if (characterState == null)
            return;

        for (int i = 0; i < models.Count; i++)
        {
            models[i].SetActive(i == index);
            if (models[i].activeSelf)
            {
                models[i].TryGetComponent(out activeAnimator);
            }
        }

        if (activeAnimator != null)
        {
            SetAnimator(activeAnimator);
            onCharacterModelSwapped.Invoke(activeAnimator);
        }    
    }

    public void SwitchActiveModel(int index)
    {
        if (index < 0 || index >= models.Count)
            return;

        for (int i = 0; i < models.Count; i++)
        {
            models[i].SetActive(i == index);
        }
    }

    public void SetAnimator(Animator animator)
    {
        if (characterState == null)
            return;

        characterState.SetAnimator(animator);
    }

    public void SetTrigger(string trigger)
    {
        if (activeAnimator == null)
            return;

        activeAnimator.SetTrigger(trigger);
    }
}
