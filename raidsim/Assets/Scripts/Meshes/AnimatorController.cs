using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorController : MonoBehaviour
{
    public bool log = false;

    private Animator animator;
    private Dictionary<string, int> paramHashes = new Dictionary<string, int>();
    private int resetAnimatorParameterHash = Animator.StringToHash("Reset");
    private int actionLockedAnimatorParameterHash = Animator.StringToHash("ActionLocked");
    private int castingAnimatorParameterHash = Animator.StringToHash("Casting");
    private int visibleAnimatorParameterHash = Animator.StringToHash("Visible");
    private int spawnAnimatorParameterHash = Animator.StringToHash("Spawn");
    private int killedAnimatorParameterHash = Animator.StringToHash("Killed");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (FightTimeline.Instance != null)
            FightTimeline.Instance.onReset.AddListener(ResetAnimator);
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

    public void ResetAnimator()
    {
        Utilities.FunctionTimer.StopTimer($"AnimatorController_{gameObject.name}_Reset_ResetTrigger");
        animator.SetBool(actionLockedAnimatorParameterHash, false);
        animator.SetBool(castingAnimatorParameterHash, false);
        animator.SetBool(visibleAnimatorParameterHash, true);
        animator.SetBool(spawnAnimatorParameterHash, false);
        animator.SetBool(killedAnimatorParameterHash, false);
        animator.SetTrigger(resetAnimatorParameterHash);
        if (log)
            Debug.Log($"[AnimatorController] ResetAnimator called with hash '{resetAnimatorParameterHash}'");
        Utilities.FunctionTimer.Create(this, () => animator.ResetTrigger(resetAnimatorParameterHash), 0.1f, $"AnimatorController_{gameObject.name}_Reset_ResetTrigger", true, true);
    }
}
