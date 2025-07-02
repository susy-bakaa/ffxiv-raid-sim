using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using static GlobalData;

namespace dev.susybaka.raidsim.UI 
{
    public class MinimapIcon : MonoBehaviour
    {
        public RectTransform RectTransform;
        public Image[] IconImages;
        public RectTransform IconRectTransform;
        public Image ArrowImage;
        public Image alternativeIconImage;
        public RectTransform alternativeIconRectTransform;
        public MinimapWorldObject WorldObject;
        public int priority = 0;
        public bool isOutsideView = false;
        private bool wasOutsideView = false;
        public bool clampToView = true;
        public bool useAlternativeIconWhenOutsideView = false;

        private WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.5f);

        private IEnumerator Start()
        {
            yield return wait;
            isOutsideView = false;
            wasOutsideView = false;
            if (IconRectTransform != null)
                IconRectTransform.gameObject.SetActive(true);
            if (alternativeIconRectTransform != null)
                alternativeIconRectTransform.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (clampToView && isOutsideView && !wasOutsideView)
            {
                wasOutsideView = isOutsideView;
                if (useAlternativeIconWhenOutsideView)
                {
                    IconRectTransform.gameObject.SetActive(false);
                    if (alternativeIconRectTransform != null)
                        alternativeIconRectTransform.gameObject.SetActive(true);
                }
                else
                {
                    IconRectTransform.gameObject.SetActive(true);
                    if (alternativeIconRectTransform != null)
                        alternativeIconRectTransform.gameObject.SetActive(false);
                }
            }
            else if (clampToView && !isOutsideView && wasOutsideView)
            {
                wasOutsideView = isOutsideView;
                if (useAlternativeIconWhenOutsideView)
                {
                    IconRectTransform.gameObject.SetActive(true);
                    if (alternativeIconRectTransform != null)
                        alternativeIconRectTransform.gameObject.SetActive(false);
                }
                else
                {
                    IconRectTransform.gameObject.SetActive(true);
                    if (alternativeIconRectTransform != null)
                        alternativeIconRectTransform.gameObject.SetActive(false);
                }
            }
        }
    }
}
