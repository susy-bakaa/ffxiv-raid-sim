using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.SaveLoad;
using UnityEngine;
using UnityEngine.UI;

namespace dev.susybaka.raidsim.UI 
{
    public class AutomarkerButtonHelper : MonoBehaviour
    {
        Button target;
        ToggleImage toggleImage;
        SaveButton saveButton;

        private void Awake()
        {
            target = GetComponentInChildren<Button>(true);
            toggleImage = GetComponentInChildren<ToggleImage>(true);
            saveButton = GetComponentInChildren<SaveButton>(true);

            if (target != null)
            {
                target.onClick.AddListener(ToggleAutomarker);
            }

            if (FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onUseAutomarkerChanged.AddListener(OnUseAutomarkerChanged);
            }
        }

        private void OnDestroy()
        {
            if (target != null)
            {
                target.onClick.RemoveListener(ToggleAutomarker);
            }
            if (FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onUseAutomarkerChanged.RemoveListener(OnUseAutomarkerChanged);
            }
        }

        private void ToggleAutomarker()
        {
            if (FightTimeline.Instance == null)
                return;

            FightTimeline.Instance.ToggleAutomarker();
        }

        private void OnUseAutomarkerChanged(bool state)
        {
            toggleImage?.Toggle(state);
            saveButton?.SaveValue(state);
        }
    }
}