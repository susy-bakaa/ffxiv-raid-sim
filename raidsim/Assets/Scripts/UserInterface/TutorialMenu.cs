using UnityEngine;
using dev.susybaka.raidsim.Inputs;

namespace dev.susybaka.raidsim.UI
{
    public class TutorialMenu : HudWindow
    {
        [SerializeField] private UserInput input;

        protected override void Awake()
        {
            base.Awake();
        }

        public void Show()
        {
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = true;

            if (input == null)
                return;

            input.inputEnabled = false;
            input.movementInputEnabled = false;
            input.zoomInputEnabled = false;
            input.rotationInputEnabled = false;
            input.targetRaycastInputEnabled = false;
        }

        public void Hide()
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            if (input == null)
                return;

            input.inputEnabled = true;
            input.movementInputEnabled = true;
            input.zoomInputEnabled = true;
            input.rotationInputEnabled = true;
            input.targetRaycastInputEnabled = true;
        }

        public void Toggle(bool state)
        {
            if (!state)
                Show();
            else
                Hide();
        }
    }
}