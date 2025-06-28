using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace dev.susybaka.raidsim.UI 
{
    public class MinimapIcon : MonoBehaviour
    {
        public Image Image;
        public RectTransform RectTransform;
        public RectTransform IconRectTransform;
        public MinimapWorldObject WorldObject;
        public int priority = 0;
    }
}
