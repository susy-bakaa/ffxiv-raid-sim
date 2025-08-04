using UnityEngine;

namespace dev.susybaka.raidsim.Animations
{
    public class OnExitState : StateMachineBehaviour
    {
        public enum StateType { ToggleChild, SetAnimatorParameter }

        public StateType m_type = StateType.SetAnimatorParameter;
        public int childIndex;
        public string parameterName = string.Empty;
        public bool toggleState;

        private int parameterHash;
        private bool hashSet = false;

        //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //
        //}

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!hashSet)
            {
                parameterHash = Animator.StringToHash(parameterName);
                hashSet = true;
            }

            if (m_type == StateType.ToggleChild)
            {
                animator.transform.GetChild(childIndex).gameObject.SetActive(toggleState);
            }
            else
            {
                animator.SetBool(parameterHash, toggleState);
            }
        }

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