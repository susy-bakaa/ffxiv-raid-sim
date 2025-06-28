using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dev.susybaka.raidsim.UI
{
    public class MinimapWorldObject : MonoBehaviour
    {
        [SerializeField] private bool followObject = false;
        [SerializeField] private Sprite minimapIcon;
        public Sprite MinimapIcon => minimapIcon;
        [SerializeField] private MinimapIcon existingIcon;
        public MinimapIcon ExistingIcon => existingIcon;
        [SerializeField] private Transform overrideTransform;
        public Transform OverrideTransform => overrideTransform;
        [SerializeField] private bool overridePosition = false;
        public bool OverridePosition => overridePosition;
        [SerializeField] private bool overrideRotation = false;
        public bool OverrideRotation => overrideRotation;
        public int priority = 0;

        private void Start()
        {
            if (MinimapHandler.Instance == null)
                return;

            MinimapHandler.Instance.RegisterMinimapWorldObject(this, followObject);
        }

        private void OnDestroy()
        {
            if (MinimapHandler.Instance == null)
                return;

            MinimapHandler.Instance.RemoveMinimapWorldObject(this);
        }
    }
}
