using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorController : MonoBehaviour
{
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
    }

    public void SetBoolFalse(string name)
    {
        int hash = GetHash(name);
        animator.SetBool(hash, false);
    }

    public void SetTrigger(string name)
    {
        int hash = GetHash(name);
        animator.SetTrigger(hash);
    }

    public void CrossFadeInFixedTime(string name)
    {
        int hash = GetHash(name);
        animator.CrossFadeInFixedTime(hash, 0.2f);
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
