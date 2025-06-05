using System.Collections;
using System.Collections.Generic;
using Bayat.Games.Animation.Utilities;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class AnimatorController : MonoBehaviour
{
    public bool log = false;
    public UnityEvent<Animator> onAnimatorReset;

    private Animator animator;
    private Dictionary<string, int> paramHashes = new Dictionary<string, int>();
    private int resetAnimatorParameterHash = Animator.StringToHash("Reset");
    private int actionLockedAnimatorParameterHash = Animator.StringToHash("ActionLocked");
    private int castingAnimatorParameterHash = Animator.StringToHash("Casting");
    private int visibleAnimatorParameterHash = Animator.StringToHash("Visible");
    private int spawnAnimatorParameterHash = Animator.StringToHash("Spawn");
    private int killedAnimatorParameterHash = Animator.StringToHash("Killed");
    private int despawnAnimatorParameterHash = Animator.StringToHash("Despawn");

    private void Awake()
    {
        animator = GetComponent<Animator>();

        animator.AddAnimatorUsage();

        if (FightTimeline.Instance != null)
            FightTimeline.Instance.onReset.AddListener(ResetAnimator);

        onAnimatorReset.Invoke(animator);
    }

    public void SetBoolTrue(string name)
    {
        int hash = GetHash(name);
        SetBool(hash, true);
    }

    public void SetBoolFalse(string name)
    {
        int hash = GetHash(name);
        SetBool(hash, false);
    }

    public void SetBool(int hash, bool value)
    {
        animator.SetBoolSafe(hash, value);
        if (log)
        {
            if (value)
                Debug.Log($"[AnimatorController] SetBoolTrue called with name '{name}', hash '{hash}'");
            else
                Debug.Log($"[AnimatorController] SetBoolFalse called with name '{name}', hash '{hash}'");
        }
    }

    public void SetTrigger(string name)
    {
        int hash = GetHash(name);
        SetTrigger(hash);
    }

    public void SetTrigger(int hash)
    {
        animator.SetTriggerSafe(hash);
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
        Utilities.FunctionTimer.StopTimer($"AnimatorController_{gameObject.name}_Reset_ResetTriggerSafe");
        animator.SetBoolSafe(actionLockedAnimatorParameterHash, false);
        animator.SetBoolSafe(castingAnimatorParameterHash, false);
        animator.SetBoolSafe(visibleAnimatorParameterHash, true);
        animator.SetBoolSafe(spawnAnimatorParameterHash, false);
        animator.SetBoolSafe(killedAnimatorParameterHash, false);
        animator.SetBoolSafe(despawnAnimatorParameterHash, false);
        animator.SetTriggerSafe(resetAnimatorParameterHash);
        if (log)
            Debug.Log($"[AnimatorController] ResetAnimator called with hash '{resetAnimatorParameterHash}'");
        Utilities.FunctionTimer.Create(this, () => animator.ResetTriggerSafe(resetAnimatorParameterHash), 0.1f, $"AnimatorController_{gameObject.name}_Reset_ResetTriggerSafe", true, true);
        onAnimatorReset.Invoke(animator);
    }
}
