// Source and credit for most of the original code: https://github.com/ZackOfAllTrad3s/Minimap
// License: Apache-2.0 license
// This script is a modified version of the original minimap manager to fit the needs of this project.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using NaughtyAttributes;
#endif
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MinimapHandler : MonoBehaviour
    {
        // Enum to define the different modes of the minimap
        public enum MinimapMode { Mini, Fullscreen }

        // References to other components and singleton instance
        public static MinimapHandler Instance;
        private CanvasGroup canvasGroup;
        private UserInput userInput;

        // Public properties and serialized fields for the minimap configuration
        public bool visible = true;
        [SerializeField] private Button zoomIn;
        [SerializeField] private Button zoomOut;
        [SerializeField] private Button settingsButton;
        [SerializeField] private TextMeshProUGUI coordinatesText;
        [SerializeField] private Transform coordinateTarget;

        [SerializeField] private Vector2 worldSize;
        [SerializeField] private Vector2 worldMin = Vector2.zero;
        [SerializeField] private bool autoCalculateMin = true;

        [SerializeField] private Vector2 fullScreenDimensions = new Vector2(1000, 1000);
        [SerializeField] private float zoomSpeed = 0.1f;
        [SerializeField] private float maxZoom = 10f;
        [SerializeField] private float minZoom = 1f;
        [SerializeField] private float iconMaxRadiusModifier = 0.5f;
        [SerializeField] private RectTransform scrollViewRectTransform;
        [SerializeField] private RectTransform contentRectTransform;
        [SerializeField] private RectTransform mapBorderRectTransform;
        [SerializeField] private MinimapIcon minimapIconPrefab;
        [SerializeField] private Transform playerCameraTransform;
        [SerializeField] private Transform playerTransform;
        public bool rotateMapInstead = false;

        // Private fields for internal state management
        private Matrix4x4 transformationMatrix;
        private MinimapMode currentMiniMapMode = MinimapMode.Mini;
        private Vector2 scrollViewDefaultSize;
        private Vector2 scrollViewDefaultPosition;
        private Dictionary<MinimapWorldObject, MinimapIcon> miniMapWorldObjectsLookup = new Dictionary<MinimapWorldObject, MinimapIcon>();
        private const string minimapToggleKeyName = "ToggleMinimapKey";

        // Editor only section to recalculate the transformation matrix and handle validation checks
#if UNITY_EDITOR
        private Vector2 previousWorldSize = Vector2.zero;
        private Vector2 previousWorldMin = Vector2.zero;
        [Button]
        public void RecalculateTransformationMatrix() => CalculateTransformationMatrix();
        private void OnValidate()
        {
            if (autoCalculateMin)
                worldMin = new Vector2(worldSize.x / 2, worldSize.y / 2) * -1;
            if ((worldSize != previousWorldSize) || (worldMin != previousWorldMin))
            {
                previousWorldSize = worldSize;
                previousWorldMin = worldMin;
                CalculateTransformationMatrix();
            }
        }
#endif

        private void Awake()
        {
            Instance = this;
            scrollViewDefaultSize = scrollViewRectTransform.sizeDelta;
            scrollViewDefaultPosition = scrollViewRectTransform.anchoredPosition;
            if (autoCalculateMin)
                worldMin = new Vector2(worldSize.x / 2, worldSize.y / 2) * -1;

            canvasGroup = GetComponent<CanvasGroup>();
            userInput = FindObjectOfType<UserInput>();
            if (userInput?.keys != null)
            {
                foreach (var binding in userInput.keys)
                    if (binding.name == minimapToggleKeyName)
                        binding.onInput.AddListener(ToggleVisible);
            }

            ToggleVisible(visible);
            zoomIn.onClick.AddListener(ZoomIn);
            zoomOut.onClick.AddListener(ZoomOut);
            settingsButton.onClick.AddListener(ToggleMinimapLock);

            if (GlobalVariables.modifiedMinimapZoom)
            {
                contentRectTransform.localScale = GlobalVariables.minimapZoom;
            }
            rotateMapInstead = GlobalVariables.rotateMinimap;
        }

        private void Start() => CalculateTransformationMatrix();

        private void Update()
        {
            if (!visible)
                return;

            UpdateMiniMapIcons();
            CenterMapOnPlayer();

            if (coordinateTarget == null)
                return;
            
            coordinatesText.text = $"X: {coordinateTarget.position.x:F1}, Y: {coordinateTarget.position.z:F1}".Replace(',', '.');
        }

        private void OnDestroy()
        {
            if (userInput?.keys != null)
            {
                foreach (var binding in userInput.keys)
                    if (binding.name == minimapToggleKeyName)
                        binding.onInput.RemoveListener(ToggleVisible);
            }
        }

        // These two toggle the visibility of the minimap by changing the alpha of the CanvasGroup component
        // These functions are required to be able to toggle the minimap visibility with a keybind
        public void ToggleVisible()
        {
            if (!gameObject.activeSelf)
                return;
            visible = !visible;
            ToggleVisible(visible);
        }

        public void ToggleVisible(bool state)
        {
            canvasGroup.alpha = state ? 1f : 0f;
            canvasGroup.interactable = state;
            canvasGroup.blocksRaycasts = state;
        }

        // Zooms the minimap in or out based on the function
        // These are required to be able to zoom the minimap in and out with UI buttons
        public void ZoomIn() => ZoomMap(1f);
        public void ZoomOut() => ZoomMap(-1f);

        // Toggles the minimap's lock state, allowing it to either rotate with the player's camera or remain locked with north facing upwards.
        // This function is required to switch between the two modes of minimap rotation behavior with an UI button
        public void ToggleMinimapLock()
        {
            rotateMapInstead = !rotateMapInstead;
            GlobalVariables.rotateMinimap = rotateMapInstead;
        }

        /// <summary>
        /// Registers a MinimapWorldObject to the minimap by creating or using an existing MinimapIcon.
        /// The icon is configured with the world object's properties, such as sprites and behaviors.
        /// Ensures the icons are sorted by priority for proper rendering order.
        /// <param name="worldObject">The MinimapWorldObject to register. If null, the function exits early.</param>
        /// </summary>
        public void RegisterMinimapWorldObject(MinimapWorldObject worldObject)
        {
            if (worldObject == null)
                return;

            if (miniMapWorldObjectsLookup.ContainsKey(worldObject))
                return;

            MinimapIcon icon = worldObject.ExistingIcon ?? Instantiate(minimapIconPrefab);
            if (worldObject.ExistingIcon == null)
                icon.gameObject.name = worldObject.ObjectName + "_MinimapIcon";
            icon.transform.SetParent(contentRectTransform, false);
            icon.iconImage.sprite = worldObject.MinimapIconSprite;
            if (icon.alternativeIconImage && worldObject.MinimapIconSprite != null)
                icon.alternativeIconImage.sprite = worldObject.MinimapIconSprite;
            if (icon.arrowImage && worldObject.MinimapArrowSprite != null)
                icon.arrowImage.sprite = worldObject.MinimapArrowSprite;
            icon.worldObject = worldObject;
            icon.useAlternativeIconWhenOutsideView = worldObject.UseAlternativeIconWhenOutsideView;
            icon.alwaysFollowObjectRotation = worldObject.AlwaysFollowObjectRotation;
            miniMapWorldObjectsLookup[worldObject] = icon;

            // Sort the children of the content RectTransform by their priority
            // This is done to ensure that icons are rendered in the correct order based on their priorities
            contentRectTransform.SortChildrenByMinimapIconPriority();
        }

        /// <summary>
        /// Removes a specified MinimapWorldObject from the minimap and destroys its associated icon.
        /// If the world object exists in the minimap lookup, it is removed and its icon GameObject is destroyed.
        /// <param name="worldObject">The worldObject that gets removed. If not set the function exits early.</param>
        /// </summary>
        public void RemoveMinimapWorldObject(MinimapWorldObject worldObject)
        {
            if (worldObject == null)
                return;

            if (miniMapWorldObjectsLookup.TryGetValue(worldObject, out var icon))
            {
                miniMapWorldObjectsLookup.Remove(worldObject);
                Destroy(icon.gameObject);
            }
        }

        // Currently not in use and not needed necessarily for our use case, but kept for future reference just in case
        public void SetMinimapMode(MinimapMode mode)
        {
            if (mode == currentMiniMapMode)
                return;
            const float fullScreenScale = 1.3f;
            if (mode == MinimapMode.Mini)
            {
                scrollViewRectTransform.sizeDelta = scrollViewDefaultSize;
                scrollViewRectTransform.anchorMin = scrollViewRectTransform.anchorMax = scrollViewRectTransform.pivot = Vector2.one;
                scrollViewRectTransform.anchoredPosition = scrollViewDefaultPosition;
                contentRectTransform.localScale = Vector3.one;
            }
            else
            {
                scrollViewRectTransform.sizeDelta = fullScreenDimensions;
                scrollViewRectTransform.anchorMin = scrollViewRectTransform.anchorMax = scrollViewRectTransform.pivot = Vector2.one * 0.5f;
                scrollViewRectTransform.anchoredPosition = Vector2.zero;
                contentRectTransform.localScale = Vector3.one * fullScreenScale;
            }
            currentMiniMapMode = mode;
        }

        /// <summary>
        /// Adjusts the zoom level of the minimap by modifying the scale of its content.
        /// The zoom level is increased or decreased based on the provided delta value.
        /// Ensures the zoom level remains within the defined minimum and maximum limits.
        /// <param name="delta">The zoom adjustment value. Positive values zoom in, negative values zoom out.</param>
        /// </summary>
        private void ZoomMap(float delta)
        {
            if (delta == 0)
                return;
            float scale = contentRectTransform.localScale.x;
            float amt = (delta > 0 ? zoomSpeed : -zoomSpeed) * scale;
            contentRectTransform.localScale = Vector3.one * Mathf.Clamp(scale + amt, minZoom, maxZoom);

            GlobalVariables.minimapZoom = contentRectTransform.localScale;
            GlobalVariables.modifiedMinimapZoom = true;
        }

        /// <summary>
        /// Centers the minimap on the player's position and adjusts its rotation based on the player's camera.
        /// If the map is set to rotate instead of locking north, the minimap content and border will rotate
        /// to match the player's camera orientation. Otherwise, the map remains static with north locked upwards.
        /// </summary>
        private void CenterMapOnPlayer()
        {
            if (playerTransform == null || playerCameraTransform == null)
                return;

            float mapScale = contentRectTransform.localScale.x;
            Vector2 playerMapPos = WorldPositionToMapPosition(playerTransform.position);
            Quaternion mapRotation = rotateMapInstead && playerCameraTransform
                ? Quaternion.Euler(0, 0, playerCameraTransform.eulerAngles.y)
                : Quaternion.identity;

            Vector2 rotatedPos = mapRotation * playerMapPos;
            contentRectTransform.anchoredPosition = -rotatedPos * mapScale;
            contentRectTransform.localRotation = mapRotation;
            mapBorderRectTransform.localRotation = mapRotation;
        }

        /// <summary>
        /// Updates the positions, rotations, scales, and visibility states of all minimap icons.
        /// This method calculates the clamping of minimap icons to the viewable area, adjusts their positions
        /// relative to the player's location, and handles icon rotation based on map rotation settings.
        /// It also ensures proper scaling and alignment of icons and their subcomponents, including alternative icons
        /// and directional arrows for objects outside the minimap's view.
        /// </summary>
        private void UpdateMiniMapIcons()
        {
            // Calculates the clamping of the minimap icons to the viewable area of the minimap.
            float mapScale = contentRectTransform.localScale.x;
            float iconScale = 1f / mapScale;
            float baseRadius = Mathf.Min(scrollViewRectTransform.rect.width, scrollViewRectTransform.rect.height) * 0.5f;
            float maxRadius = baseRadius * iconMaxRadiusModifier / mapScale;

            // Checks if the whole map is rotating or if north is locked upwards and there is no rotation applied to the map.
            Quaternion mapRotation = rotateMapInstead && playerCameraTransform
                ? Quaternion.Euler(0, 0, playerCameraTransform.eulerAngles.y)
                : Quaternion.identity;

            // Get the center of the map based on the player's position
            Vector2 centerMap = WorldPositionToMapPosition(playerTransform.position);

            foreach (var kvp in miniMapWorldObjectsLookup)
            {
                MinimapWorldObject worldObject = kvp.Key;
                MinimapIcon icon = kvp.Value;
                Vector2 mapPos = WorldPositionToMapPosition(
                    worldObject.OverrideTransform != null && worldObject.OverridePosition
                        ? worldObject.OverrideTransform.position
                        : worldObject.transform.position);

                // Calculates the raw offset and rotate for clamping
                Vector2 rawOffset = mapPos - centerMap;
                Vector2 offsetToClamp = rotateMapInstead ? (mapRotation * rawOffset) : rawOffset;
                bool isClamped = offsetToClamp.magnitude > maxRadius;
                Vector2 clampedRotatedOffset = isClamped ? offsetToClamp.normalized * maxRadius : offsetToClamp;
                Vector2 finalOffset = rotateMapInstead ? (Quaternion.Inverse(mapRotation) * clampedRotatedOffset) : clampedRotatedOffset;

                // Setting the position and the clamped state of the icon
                icon.rectTransform.anchoredPosition = centerMap + finalOffset;
                icon.isOutsideView = isClamped;

                // Calculates and sets all of the rotations of each icon and their sub components
                if (icon.useAlternativeIconWhenOutsideView && isClamped)
                {
                    float angle = Mathf.Atan2(offsetToClamp.y, offsetToClamp.x) * Mathf.Rad2Deg;
                    float arrowRot = angle - 90f;

                    // So this section is a bit of a mess, but it works.
                    // I designed this whole thing kinda backwards and had so much trouble with it,
                    // because for some reason GitHub Copilot insisted on doing everything with localRotation and Quaternion.Euler,
                    // which was not the play at all with this. I thought there was no way to do it any other way with UI.
                    // Turns out you can just set the global rotation of RectTransforms just like any other Transform,
                    // which handles this painful translation logic for you and there is no need to touch any Quaternions.
                    // So yeah because of this I kinda have tons of these redundant reverse calculations and checks for rotations,
                    // but I don't want to break the existing functionality and refactor it so I'm leaving it as is for now.
                    //
                    // - susy_baka

                    // Rotates the out of view icons to point towards the center of the map,
                    // but keeps the alternative icons pointing upwards on the screen (north).
                    // This is done with various Quaternion.Euler calls and localRotation adjustments.
                    if (!rotateMapInstead)
                    {
                        icon.arrowRectTransform.localRotation = Quaternion.Euler(0, 0, arrowRot);
                        icon.alternativeIconRectTransform.localRotation = Quaternion.Euler(0, 0, arrowRot);
                        icon.alternativeIconImage.rectTransform.localRotation = Quaternion.Euler(0, 0, -arrowRot);
                    }
                    else // If the map is rotating instead, adjustments to the rotation based on the camera's Y axis rotation are needed
                    {
                        float camY = playerCameraTransform.eulerAngles.y;

                        icon.arrowRectTransform.localRotation = Quaternion.Euler(0, 0, arrowRot - camY);

                        icon.alternativeIconRectTransform.localRotation = Quaternion.Euler(0, 0, arrowRot - camY);
                        icon.alternativeIconImage.rectTransform.eulerAngles = Vector3.zero;
                    }
                }

                // Handles setting the Y axis rotation and position based on if there are overrides or not
                float objY = (worldObject.OverrideTransform != null && worldObject.OverrideRotation)
                    ? worldObject.OverrideTransform.eulerAngles.y
                    : worldObject.transform.eulerAngles.y;

                // If rotateMapInstead is true, adjusts the icon's rotation based on the camera's Y axis rotation
                // or if the object always follows its own rotation handles that
                if (rotateMapInstead)
                {
                    if (!worldObject.AlwaysFollowObjectRotation)
                        icon.iconRectTransform.localRotation = Quaternion.Euler(0, 0, -playerCameraTransform.eulerAngles.y);
                    else
                        icon.iconRectTransform.localRotation = Quaternion.Euler(0, 0, -objY);
                }
                else // If rotateMapInstead is false, sets the icon's rotation directly based on it's world objects Y axis rotation
                {
                    icon.iconRectTransform.localRotation = Quaternion.Euler(0, 0, -objY);
                }

                // Calculates and sets all of the scales of each icon and their sub components
                icon.iconRectTransform.localScale = Vector3.one * iconScale;
                if (icon.alternativeIconRectTransform != null)
                    icon.alternativeIconRectTransform.localScale = Vector3.one * iconScale;
                if (icon.arrowRectTransform != null)
                    icon.arrowRectTransform.localScale = Vector3.one * iconScale;
            }
        }

        private Vector2 WorldPositionToMapPosition(Vector3 worldPos)
            => transformationMatrix.MultiplyPoint3x4(new Vector2(worldPos.x, worldPos.z));


        /// <summary>
        /// Calculates the transformation matrix used to convert world coordinates to minimap coordinates.
        /// This matrix accounts for the size and position of the minimap content relative to the world dimensions.
        /// It ensures proper scaling and translation of world positions to fit within the minimap.
        /// </summary>
        private void CalculateTransformationMatrix()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            Vector2 mapSize = contentRectTransform.rect.size;
            Vector2 scaleRatio = new Vector2(mapSize.x / worldSize.x, mapSize.y / worldSize.y);
            Vector2 translation = -worldMin * scaleRatio - mapSize * 0.5f;
            transformationMatrix = Matrix4x4.TRS(translation, Quaternion.identity, new Vector3(scaleRatio.x, scaleRatio.y, 1f));
        }
    }
}
