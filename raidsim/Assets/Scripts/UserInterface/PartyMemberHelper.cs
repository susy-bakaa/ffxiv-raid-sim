// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(HudElement))]
    public class PartyMemberHelper : MonoBehaviour
    {
        private HudElement hudElement;

        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI healthBarText;
        [SerializeField] private Slider healthBarSlider;
        [SerializeField] private Slider shieldBarSlider;
        [SerializeField] private Slider overShieldBarSlider;
        [SerializeField] private TextMeshProUGUI castNameText;
        [SerializeField] private Slider castBarSlider;
        [SerializeField] private RectTransform statusEffectHolder;
        [SerializeField] private PartyIcon partyIcon;

        public HudElement HudElement { get { return hudElement; } }
        public TextMeshProUGUI NameText { get { return nameText; } }
        public TextMeshProUGUI HealthBarText { get { return healthBarText; } }
        public Slider HealthBarSlider { get { return healthBarSlider; } }
        public Slider ShieldBarSlider { get { return shieldBarSlider; } }
        public Slider OverShieldBarSlider { get { return overShieldBarSlider; } }
        public TextMeshProUGUI CastNameText { get { return castNameText; } }
        public Slider CastBarSlider { get { return castBarSlider; } }
        public RectTransform StatusEffectHolder { get { return statusEffectHolder; } }
        public PartyIcon PartyIcon { get { return partyIcon; } }

        private void Awake()
        {
            hudElement = GetComponent<HudElement>();
        }
    }
}