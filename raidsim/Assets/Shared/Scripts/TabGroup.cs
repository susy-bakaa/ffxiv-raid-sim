// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
#pragma warning disable 436
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dev.susybaka.Shared.UserInterface
{
    [AddComponentMenu("UI/Custom/Tab Group")]
    public class TabGroup : MonoBehaviour
    {
        [Header("Technical")]
        public List<TabButton> tabButtons;
        private TabButton selectedTab;
        public List<GameObject> objectsToSwap;
        public int defaultTab= -1;

        [Header("Visuals")]
        public Color tabIdle;
        public Color tabHover;
        public Color tabActive;

        private readonly WaitForSecondsRealtime _startWait = new WaitForSecondsRealtime(1f);

        private IEnumerator Start()
        {
            if (defaultTab < 0)
                yield break;

            yield return _startWait;

            foreach (var gObj in objectsToSwap)
            {
                gObj.SetActive(false);
            }
            ResetTabs();
            OnTabSelected(tabButtons[defaultTab]);
        }

        public void Subscribe(TabButton button)
        {
            if (tabButtons == null)
            {
                tabButtons = new List<TabButton>();
            }

            if (!tabButtons.Contains(button))
                tabButtons.Add(button);
        }

        public void OnTabEnter(TabButton button)
        {
            ResetTabs();
            if (selectedTab == null || button != selectedTab)
            {
                button.background.color = tabHover;
            }
        }

        public void OnTabExit(TabButton button)
        {
            ResetTabs();
        }

        public void OnTabSelected(TabButton button)
        {
            if (selectedTab != null)
                selectedTab.Deselect();

            selectedTab = button;

            selectedTab.Select();

            ResetTabs();
            button.background.color = tabActive;
            int index = button.transform.GetSiblingIndex();

            if (objectsToSwap.Count > 0)
            {
                for (int i = 0; i < objectsToSwap.Count; i++)
                {
                    if (i == index)
                    {
                        objectsToSwap[i].SetActive(true);
                    }
                    else
                    {
                        objectsToSwap[i].SetActive(false);
                    }
                }
            }
        }

        private void ResetTabs()
        {
            foreach (TabButton button in tabButtons)
            {
                if (selectedTab != null && button == selectedTab)
                    continue;
                if (button == null)
                    continue;

                button.background.color = tabIdle;
                button.Deselect();
            }
        }
    }
}