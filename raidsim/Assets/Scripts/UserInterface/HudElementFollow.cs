using UnityEngine;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Targeting;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(HudElement))]
    public class HudElementFollow : MonoBehaviour
    {
        Camera m_camera;
        HudElement element;
        RectTransform rect;
        TargetController targetController;

        public Transform target;
        public bool constantFollow = false;
        public bool followCharacterStateTarget = false;
        public bool hideWhenNoTarget = false;

        private bool follow = true;
        
        private void Awake()
        {
            element = GetComponent<HudElement>();
            rect = GetComponent<RectTransform>();
            m_camera = Camera.main;
            if (element != null)
                element.onInitialize.AddListener(Initialize);
        }

        private void Update()
        {
            if (target != null && follow)
            {
                Vector3 screenPoint = m_camera.WorldToScreenPoint(target.position);
                rect.position = screenPoint;
                if (!constantFollow)
                    follow = false;
            }
        }

        private void OnDestroy()
        {
            if (element != null)
                element.onInitialize.RemoveListener(Initialize);
            if (targetController != null)
                targetController.onTarget.RemoveListener(SetTarget);
        }

        public void Initialize()
        {
            follow = true;
            if (element != null && element.characterState != null)
            {
                if (!followCharacterStateTarget)
                {
                    if (element.characterState.statusPopupPivot != null)
                        target = element.characterState.statusPopupPivot;
                    else
                        target = element.characterState.transform;
                }
                else if (element.characterState.targetController != null)
                {
                    targetController = element.characterState.targetController;
                    targetController.onTarget.AddListener(SetTarget);
                }
            }
        }

        public void SetTarget(TargetNode targetNode)
        {
            if (targetNode == null)
            {
                if (hideWhenNoTarget)
                {
                    element.hidden = true;
                }
                target = null;
                return;
            }
            else
            {
                if (hideWhenNoTarget)
                {
                    element.hidden = false;
                }
            }

            if (targetNode.TryGetCharacterState(out CharacterState c))
            {
                if (c.statusPopupPivot != null)
                    target = c.statusPopupPivot;
                else
                    target = c.transform;
            }
            else
            {
                target = targetNode.transform;
            }
        }
    }
}