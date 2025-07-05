using UnityEngine;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.Editor
{
    [RequireComponent(typeof(CharacterState))]
    public class CharacterStateDebugger : MonoBehaviour
    {
        public bool m_enabled = true;
        public CharacterState characterState;

        private void Awake()
        {
            characterState = GetComponent<CharacterState>();
            if (FightTimeline.Instance.log)
                m_enabled = true;
        }

        private void Update()
        {
            Debug.Log($"[CharacterStateDebugger.{characterState.characterName} ({characterState.gameObject})] untargetable: '{characterState.untargetable.value}', disabled: '{characterState.disabled}'");
        }
    }
}