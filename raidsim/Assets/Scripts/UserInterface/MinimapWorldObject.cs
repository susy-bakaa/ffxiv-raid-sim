// Source and credit for most of the original code: https://github.com/ZackOfAllTrad3s/Minimap
// License: Apache-2.0 license
// This script is a modified version of the original minimap manager to fit the needs of this project.
// To Learn more, check out the THIRD-PARTY-NOTICES.txt file in the root directory of this project.
using dev.susybaka.raidsim.Characters;
using dev.susybaka.Shared;
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

        private CharacterState state;
        private bool started = false;

        public string ObjectName { get { return objectName; } }
        public bool UseAlternativeIconWhenOutsideView { get { return useAlternativeIconWhenOutsideView; } }
        public Sprite MinimapIconSprite { get { return minimapIconSprite; } }
        public Sprite MinimapArrowSprite { get { return minimapArrowSprite; } }
        public MinimapIcon ExistingIcon { get { return existingIcon; } }
        public Transform OverrideTransform { get { return overrideTransform; } }
        public bool OverridePosition { get { return overridePosition; } }
        public bool OverrideRotation { get { return overrideRotation; } }
        public bool AlwaysFollowObjectRotation { get { return alwaysFollowObjectRotation; } }
        public CharacterState State { get { return state; } }

        private void Start()
        {
            transform.TryGetComponentInParents(out state);

            if (string.IsNullOrEmpty(objectName))
            {
                if (state != null)
                    objectName = state.gameObject.name;
                else
                    objectName = gameObject.name;
            }

            if (MinimapHandler.Instance == null)
                return;
            
            MinimapHandler.Instance.RegisterMinimapWorldObject(this);

            started = true;
        }

        private void OnEnable()
        {
            if (MinimapHandler.Instance == null || !started)
                return;

            MinimapHandler.Instance.RegisterMinimapWorldObject(this);
        }

        private void OnDisable()
        {
            if (MinimapHandler.Instance == null)
                return;

            MinimapHandler.Instance.RemoveMinimapWorldObject(this);
        }

        private void OnDestroy()
        {
            if (MinimapHandler.Instance == null)
                return;

            MinimapHandler.Instance.RemoveMinimapWorldObject(this);
        }
    }
}
