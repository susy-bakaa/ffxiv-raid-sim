using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryModel : MonoBehaviour
{
    ActionController actionController;
    [SerializeField] private List<AnimationTell> animationTells = new List<AnimationTell>();

    private void Awake()
    {
        transform.TryGetComponentInParents(out actionController);
    }

    private void Update()
    {
        if (actionController == null)
            return;

        if (actionController.LastAction == null)
        {
            if (Utilities.RateLimiter(58))
            {
                for (int i = 0; i < animationTells.Count; i++)
                {
                    animationTells[i].gameObject.SetActive(!animationTells[i].state);
                }
            }
            return;
        }

        if (animationTells == null || animationTells.Count < 1)
            return;

        for (int i = 0; i < animationTells.Count; i++)
        {
            if (actionController.LastAction == animationTells[i].action)
            {
                animationTells[i].gameObject.SetActive(animationTells[i].state);
            }
            else
            {
                animationTells[i].gameObject.SetActive(!animationTells[i].state);
            }
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < animationTells.Count; i++)
        {
            animationTells[i].gameObject.SetActive(true);
        }
    }

    [System.Serializable]
    private struct AnimationTell
    {
        public string name;
        public CharacterAction action;
        public GameObject gameObject;
        public bool state;
    }
}
