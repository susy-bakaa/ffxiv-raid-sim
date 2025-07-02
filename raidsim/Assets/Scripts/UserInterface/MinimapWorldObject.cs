// Source and credit for most of the original code: https://github.com/ZackOfAllTrad3s/Minimap
// License: Apache-2.0 license
// This script is a modified version of the original minimap world object to fit the needs of this project.
using UnityEngine;
using UnityEngine.Serialization;

namespace dev.susybaka.raidsim.UI
{
    public class MinimapWorldObject : MonoBehaviour
    {
        [SerializeField] private string objectName = string.Empty;
        [SerializeField] private bool useAlternativeIconWhenOutsideView = false;
        [SerializeField, FormerlySerializedAs("minimapIcon")] private Sprite minimapIconSprite;
        [SerializeField] private Sprite minimapArrowSprite;
        [SerializeField] private MinimapIcon existingIcon;
        [SerializeField] private Transform overrideTransform;
        [SerializeField] private bool overridePosition = false;
        [SerializeField] private bool overrideRotation = false;
        [SerializeField] private bool alwaysFollowObjectRotation = false;
        public int priority = 0;

        public string ObjectName { get { return objectName; } }
        public bool UseAlternativeIconWhenOutsideView { get { return useAlternativeIconWhenOutsideView; } }
        public Sprite MinimapIconSprite { get { return minimapIconSprite; } }
        public Sprite MinimapArrowSprite { get { return minimapArrowSprite; } }
        public MinimapIcon ExistingIcon { get { return existingIcon; } }
        public Transform OverrideTransform { get { return overrideTransform; } }
        public bool OverridePosition { get { return overridePosition; } }
        public bool OverrideRotation { get { return overrideRotation; } }
        public bool AlwaysFollowObjectRotation { get { return alwaysFollowObjectRotation; } }

        private void Start()
        {
            if (string.IsNullOrEmpty(objectName))
            {
                objectName = gameObject.name;
            }

            if (MinimapHandler.Instance == null)
                return;

            MinimapHandler.Instance.RegisterMinimapWorldObject(this);
        }

        private void OnDestroy()
        {
            if (MinimapHandler.Instance == null)
                return;

            MinimapHandler.Instance.RemoveMinimapWorldObject(this);
        }
    }
}
