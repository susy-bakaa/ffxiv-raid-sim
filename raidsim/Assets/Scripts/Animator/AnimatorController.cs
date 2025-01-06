using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorController : MonoBehaviour
{
    public bool log = false;

    private Animator animator;
    private Dictionary<string, int> paramHashes = new Dictionary<string, int>();

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetBoolTrue(string name)
    {
        int hash = GetHash(name);
        animator.SetBool(hash, true);
        if (log)
            Debug.Log($"[AnimatorController] SetBoolTrue called with name '{name}', hash '{hash}'");
    }

    public void SetBoolFalse(string name)
    {
        int hash = GetHash(name);
        animator.SetBool(hash, false);
        if (log)
            Debug.Log($"[AnimatorController] SetBoolFalse called with name '{name}', hash '{hash}'");
    }

    public void SetTrigger(string name)
    {
        int hash = GetHash(name);
        animator.SetTrigger(hash);
        if (log)
            Debug.Log($"[AnimatorController] SetTrigger called with name '{name}', hash '{hash}'");
    }

    public void CrossFadeInFixedTime(string name)
    {
        int hash = GetHash(name);
        animator.CrossFadeInFixedTime(hash, 0.2f);
        if (log)
            Debug.Log($"[AnimatorController] CrossFadeInFixedTime called with name '{name}', hash '{hash}'");
    }

    private int GetHash(string name)
    {
        if (!paramHashes.TryGetValue(name, out int hash))
        {
            hash = Animator.StringToHash(name);
            paramHashes[name] = hash;
        }
        return hash;
    }
}
