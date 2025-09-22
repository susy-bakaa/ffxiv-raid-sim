// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;

namespace dev.susybaka.raidsim.Animations
{
    public class OnEnterState : StateMachineBehaviour
    {
        public enum StateType { ToggleChild, SetAnimatorParameter }

        public StateType m_type = StateType.SetAnimatorParameter;
        public int childIndex;
        public string parameterName = string.Empty;
        public bool toggleState;
        public bool log = false;

        private int parameterHash;
        private bool hashSet = false;

        //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!hashSet)
            {
                parameterHash = Animator.StringToHash(parameterName);
                hashSet = true;
            }

            if (m_type == StateType.ToggleChild)
            {
                if (log)
                    Debug.Log($"ToggleChild '{animator.gameObject.name}' childIndex {childIndex} ({animator.transform.GetChild(childIndex).gameObject.name}) childCount {animator.transform.childCount} toggleState {toggleState}");
                animator.transform.GetChild(childIndex).gameObject.SetActive(toggleState);
            }
            else
            {
                animator.SetBool(parameterHash, toggleState);
            }
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }
}