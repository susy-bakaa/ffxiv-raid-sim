// Source and credit for most of the original code: https://github.com/ZackOfAllTrad3s/Minimap
// License: Apache-2.0 license
// This script is a modified version of the original minimap manager to fit the needs of this project.
// To Learn more, check out the THIRD-PARTY-NOTICES.txt file in the root directory of this project.
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace dev.susybaka.raidsim.UI 
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MinimapIcon : MonoBehaviour
    {
        CanvasGroup group;

        public RectTransform rectTransform;
        public Image iconImage;
        public RectTransform iconRectTransform;
        public Image arrowImage;
        public RectTransform arrowRectTransform;
        public Image alternativeIconImage;
        public RectTransform alternativeIconRectTransform;
        public MinimapWorldObject worldObject;
        public int priority = 0;
        public bool isOutsideView = false;
        public bool clampToView = true;
        public bool useAlternativeIconWhenOutsideView = false;
        public bool alwaysFollowObjectRotation = false;

        private bool wasOutsideView = false;
        private WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.5f);

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
        }

        private IEnumerator Start()
        {
            yield return wait;
            isOutsideView = false;
            wasOutsideView = false;
            if (iconRectTransform != null)
                iconRectTransform.gameObject.SetActive(true);
            if (alternativeIconRectTransform != null)
                alternativeIconRectTransform.gameObject.SetActive(false);
            if (arrowRectTransform != null)
                arrowRectTransform.gameObject.SetActive(false);
            group.alpha = 0f;
        }

        private void Update()
        {
            if (MinimapHandler.Instance == null || !MinimapHandler.Instance.visible)
            {
                isOutsideView = false;
                wasOutsideView = false;
                if (iconRectTransform != null)
                    iconRectTransform.gameObject.SetActive(true);
                if (alternativeIconRectTransform != null)
                    alternativeIconRectTransform.gameObject.SetActive(false);
                if (arrowRectTransform != null)
                    arrowRectTransform.gameObject.SetActive(false);
                group.alpha = 0f;
                return;
            }

            if (clampToView && isOutsideView && !wasOutsideView)
            {
                wasOutsideView = isOutsideView;
                if (useAlternativeIconWhenOutsideView)
                {
                    iconRectTransform.gameObject.SetActive(false);
                    if (alternativeIconRectTransform != null)
                        alternativeIconRectTransform.gameObject.SetActive(true);
                    if (arrowRectTransform != null)
                        arrowRectTransform.gameObject.SetActive(true);
                }
                else
                {
                    iconRectTransform.gameObject.SetActive(true);
                    if (alternativeIconRectTransform != null)
                        alternativeIconRectTransform.gameObject.SetActive(false);
                    if (arrowRectTransform != null)
                        arrowRectTransform.gameObject.SetActive(false);
                }
            }
            else if (clampToView && !isOutsideView && wasOutsideView)
            {
                wasOutsideView = isOutsideView;
                if (useAlternativeIconWhenOutsideView)
                {
                    iconRectTransform.gameObject.SetActive(true);
                    if (alternativeIconRectTransform != null)
                        alternativeIconRectTransform.gameObject.SetActive(false);
                    if (arrowRectTransform != null)
                        arrowRectTransform.gameObject.SetActive(false);
                }
                else
                {
                    iconRectTransform.gameObject.SetActive(true);
                    if (alternativeIconRectTransform != null)
                        alternativeIconRectTransform.gameObject.SetActive(false);
                    if (arrowRectTransform != null)
                        arrowRectTransform.gameObject.SetActive(false);
                }
            }

            if (group != null && worldObject != null)
            {
                if (worldObject.State != null && (worldObject.State.untargetable.value || worldObject.State.hidePartyListEntry))
                {
                    group.alpha = 0f;
                    return;
                }
            }

            group.alpha = 1f;
        }
    }
}
