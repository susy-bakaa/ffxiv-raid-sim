using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    Animator animator;
    CharacterState state;

    void Awake()
    {
        animator = GetComponent<Animator>();    
        state = GetComponent<CharacterState>();
    }

    void Update()
    {
        animator.SetBool("Dead", state.dead);
    }
}
