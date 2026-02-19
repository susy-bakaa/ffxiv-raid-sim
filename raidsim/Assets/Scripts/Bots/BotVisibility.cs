// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using UnityEngine;

namespace dev.susybaka.raidsim.Bots
{
    [RequireComponent(typeof(CharacterState))]
    public class BotVisibility : MonoBehaviour
    {
        private FightTimeline timeline;
        private CharacterState characterState;

        public int botVisibilityId = 6006;

        private GameObject visualEffectParent;
        private GameObject minimapMarker;
        private bool showStatusPopups;
        private bool showNameplate;
        private bool showCharacterName;
        private bool showNameplateHealthBar;

        private void Awake()
        {
            timeline = FightTimeline.Instance;
            characterState = GetComponent<CharacterState>();
            visualEffectParent = characterState?.transform.Find("VisualEffects")?.gameObject;
            minimapMarker = characterState?.transform.GetComponentInChildren<MinimapWorldObject>()?.gameObject;
            if (characterState != null)
            {
                showStatusPopups = characterState.showStatusPopups;
                showNameplate = !characterState.hideNameplate;
                showCharacterName = characterState.showCharacterName;
                showNameplateHealthBar = characterState.showNameplateHealthBar;
            }
        }

        public void UpdateVisibility()
        {
            if (timeline == null)
                return;

            if (timeline.TryGetRandomEventResult(botVisibilityId, out int value))
            {
                if (value == 0) // model invisible
                {
                    characterState.ToggleNameplate(showNameplate);
                    characterState.showCharacterName = false;
                    characterState.showNameplateHealthBar = false;
                    characterState.UpdateCharacterName();
                    characterState.RefreshUserInterface();
                    characterState.hidden.SetFlag("BotVisibility", true);
                    characterState.UpdateVisibility();
                    characterState.ghost = false;
                    minimapMarker?.SetActive(true);
                }
                else if (value > 0) // whole character is invisible
                {
                    characterState.ToggleNameplate(false);
                    characterState.showCharacterName = showCharacterName;
                    characterState.showNameplateHealthBar = showNameplateHealthBar;
                    characterState.UpdateCharacterName();
                    characterState.RefreshUserInterface();
                    characterState.hidden.SetFlag("BotVisibility", true);
                    characterState.UpdateVisibility();
                    characterState.ghost = true;
                    minimapMarker?.SetActive(false);
                }
            }
            else // Visible
            {
                characterState.ToggleNameplate(showNameplate);
                characterState.showCharacterName = showCharacterName;
                characterState.showNameplateHealthBar = showNameplateHealthBar;
                characterState.UpdateCharacterName();
                characterState.RefreshUserInterface();
                characterState.hidden.SetFlag("BotVisibility", false);
                characterState.UpdateVisibility();
                characterState.ghost = false;
                minimapMarker?.SetActive(true);
            }

            UpdateCharacter();
        }

        private void UpdateCharacter()
        {
            if (characterState == null)
                return;

            if (characterState.ghost)
            {
                if (visualEffectParent != null)
                {
                    visualEffectParent.SetActive(false);
                }
                characterState.showStatusPopups = false;
            }
            else
            {
                if (visualEffectParent != null)
                {
                    visualEffectParent.SetActive(true);
                }
                characterState.showStatusPopups = showStatusPopups;
            }
        }
    }
}