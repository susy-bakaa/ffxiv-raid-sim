using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace dev.susybaka.raidsim.UI
{
    public class MinimapWorldObject : MonoBehaviour
    {
        [SerializeField] private string objectName = string.Empty;
        public string ObjectName => objectName;
        [SerializeField] private bool followObject = false;
        //[SerializeField] private MinimapIcon minimapIconPrefab;
        //public MinimapIcon MinimapIconPrefab => minimapIconPrefab;
        [SerializeField] private bool useAlternativeIconWhenOutsideView = false;
        public bool UseAlternativeIconWhenOutsideView => useAlternativeIconWhenOutsideView;
        [SerializeField, FormerlySerializedAs("minimapIcon")] private Sprite minimapIconSprite;
        public Sprite MinimapIconSprite => minimapIconSprite;
        [SerializeField] private Sprite minimapArrowSprite;
        public Sprite MinimapArrowSprite => minimapArrowSprite;
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
            if (string.IsNullOrEmpty(objectName))
            {
                objectName = gameObject.name;
            }

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
