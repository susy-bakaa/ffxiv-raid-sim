// Source and credit for 90% of the code: https://github.com/ZackOfAllTrad3s/Minimap
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using NaughtyAttributes;
#endif

namespace dev.susybaka.raidsim.UI
{
    public enum MinimapMode
    {
        Mini, Fullscreen
    }
    [RequireComponent(typeof(CanvasGroup))]
    public class MinimapHandler : MonoBehaviour
    {
        public static MinimapHandler Instance;
        private CanvasGroup canvasGroup;
        private UserInput userInput;

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
        [SerializeField] private bool rotateMapInstead = false;

        Matrix4x4 transformationMatrix;

        private MinimapMode currentMiniMapMode = MinimapMode.Mini;
        private MinimapIcon followIcon;
        private Vector2 scrollViewDefaultSize;
        private Vector2 scrollViewDefaultPosition;
        Dictionary<MinimapWorldObject, MinimapIcon> miniMapWorldObjectsLookup = new Dictionary<MinimapWorldObject, MinimapIcon>();
        private const string minimapToggleKeyName = "ToggleMinimapKey";

#if UNITY_EDITOR
        private Vector2 previousWorldSize = Vector2.zero;

        [Button]
        public void RecalculateTransformationMatrix()
        {
            CalculateTransformationMatrix();
        }

        private void OnValidate()
        {
            if (autoCalculateMin)
                worldMin = new Vector2(worldSize.x / 2, worldSize.y / 2) * -1;

            if (worldSize != previousWorldSize)
            {
                previousWorldSize = worldSize;
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

            if (userInput != null)
            {
                if (userInput.keys != null && userInput.keys.Count > 0)
                {
                    foreach (UserInput.InputBinding binding in userInput.keys)
                    {
                        if (binding.name == minimapToggleKeyName)
                        {
                            binding.onInput.AddListener(ToggleVisible);
                        }
                    }
                }
            }

            ToggleVisible(visible);

            zoomIn.onClick.AddListener(ZoomIn);
            zoomOut.onClick.AddListener(ZoomOut);
            settingsButton.onClick.AddListener(ToggleMinimapLock);
        }

        private void Start()
        {
            CalculateTransformationMatrix();
        }

        private void Update()
        {
            /*if (Input.GetKeyDown(KeyCode.M))
            {
                SetMinimapMode(currentMiniMapMode == MinimapMode.Mini ? MinimapMode.Fullscreen : MinimapMode.Mini);
            }*/

            //float zoom = Input.GetAxis("Mouse ScrollWheel");
            //ZoomMap(zoom);

            if (rotateMapInstead && playerCameraTransform != null)
            {
                float rotationY = playerCameraTransform.eulerAngles.y;
                contentRectTransform.localRotation = Quaternion.Euler(0, 0, rotationY);
            }
            else
            {
                contentRectTransform.localRotation = Quaternion.identity;
            }

            UpdateMiniMapIcons();
            CenterMapOnIcon();

            if (followIcon == null)
                return;

            coordinatesText.text = $"X: {coordinateTarget.transform.position.x:F1}, Y: {coordinateTarget.transform.position.z:F1}".Replace(',', '.');
        }

        private void OnDestroy()
        {
            if (userInput != null)
            {
                if (userInput.keys != null && userInput.keys.Count > 0)
                {
                    foreach (UserInput.InputBinding binding in userInput.keys)
                    {
                        if (binding.name == minimapToggleKeyName)
                        {
                            binding.onInput.RemoveListener(ToggleVisible);
                        }
                    }
                }
            }
        }

        public void ToggleVisible()
        {
            if (!gameObject.activeSelf)
                return;

            visible = !visible;
            ToggleVisible(visible);
        }

        public void ToggleVisible(bool state)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = state ? 1f : 0f;
                canvasGroup.interactable = state;
                canvasGroup.blocksRaycasts = state;
            }
        }

        public void ZoomIn()
        {
            ZoomMap(1f);
        }

        public void ZoomOut()
        {
            ZoomMap(-1f);
        }

        public void RegisterMinimapWorldObject(MinimapWorldObject miniMapWorldObject, bool followObject = false)
        {
            if (miniMapWorldObject == null)
                return;

            MinimapIcon minimapIcon;

            if (miniMapWorldObject.ExistingIcon == null)
            {
                minimapIcon = Instantiate(minimapIconPrefab);
                minimapIcon.gameObject.name = $"{miniMapWorldObject.ObjectName}_MinimapIcon";
            }
            else
            {
                minimapIcon = miniMapWorldObject.ExistingIcon;
            }
            
            minimapIcon.transform.SetParent(contentRectTransform);
            for (int i = 0; i < minimapIcon.IconImages.Length; i++)
            {
                minimapIcon.IconImages[i].sprite = miniMapWorldObject.MinimapIconSprite;
            }
            if (minimapIcon.ArrowImage != null && miniMapWorldObject.MinimapArrowSprite != null)
                minimapIcon.ArrowImage.sprite = miniMapWorldObject.MinimapArrowSprite;
            minimapIcon.WorldObject = miniMapWorldObject;
            minimapIcon.useAlternativeIconWhenOutsideView = miniMapWorldObject.UseAlternativeIconWhenOutsideView;
            miniMapWorldObjectsLookup[miniMapWorldObject] = minimapIcon;

            if (followObject)
                followIcon = minimapIcon;

            // Sort children by priority (higher numbers above others)
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < contentRectTransform.childCount; i++)
            {
                children.Add(contentRectTransform.GetChild(i));
            }

            children.Sort((a, b) =>
            {
                int priorityA = int.MinValue;
                int priorityB = int.MinValue;

                if (a.TryGetComponent(out MinimapIcon iconA))
                {
                    if (iconA.WorldObject != null)
                        priorityA = iconA.WorldObject.priority;
                    else
                        priorityA = iconA.priority;
                }
                if (b.TryGetComponent(out MinimapIcon iconB))
                {
                    if (iconB.WorldObject != null)
                        priorityB = iconB.WorldObject.priority;
                    else
                        priorityB = iconB.priority;
                }
                // Higher priority first
                return priorityA.CompareTo(priorityB);
            });

            for (int i = 0; i < children.Count; i++)
            {
                children[i].SetSiblingIndex(i);
            }
        }

        public void RemoveMinimapWorldObject(MinimapWorldObject minimapWorldObject)
        {
            if (miniMapWorldObjectsLookup.TryGetValue(minimapWorldObject, out MinimapIcon icon))
            {
                miniMapWorldObjectsLookup.Remove(minimapWorldObject);
                Destroy(icon.gameObject);
            }
        }


        private Vector2 halfVector2 = new Vector2(0.5f, 0.5f);
        public void SetMinimapMode(MinimapMode mode)
        {
            const float defaultScaleWhenFullScreen = 1.3f; // 1.3f looks good here but it could be anything

            if (mode == currentMiniMapMode)
                return;

            switch (mode)
            {
                case MinimapMode.Mini:
                    scrollViewRectTransform.sizeDelta = scrollViewDefaultSize;
                    scrollViewRectTransform.anchorMin = Vector2.one;
                    scrollViewRectTransform.anchorMax = Vector2.one;
                    scrollViewRectTransform.pivot = Vector2.one;
                    scrollViewRectTransform.anchoredPosition = scrollViewDefaultPosition;
                    currentMiniMapMode = MinimapMode.Mini;
                    break;
                case MinimapMode.Fullscreen:
                    scrollViewRectTransform.sizeDelta = fullScreenDimensions;
                    scrollViewRectTransform.anchorMin = halfVector2;
                    scrollViewRectTransform.anchorMax = halfVector2;
                    scrollViewRectTransform.pivot = halfVector2;
                    scrollViewRectTransform.anchoredPosition = Vector2.zero;
                    currentMiniMapMode = MinimapMode.Fullscreen;
                    contentRectTransform.transform.localScale = Vector3.one * defaultScaleWhenFullScreen;
                    break;
            }
        }

        private void ZoomMap(float zoom)
        {
            if (zoom == 0)
                return;

            float currentMapScale = contentRectTransform.localScale.x;
            // we need to scale the zoom speed by the current map scale to keep the zooming linear
            float zoomAmount = (zoom > 0 ? zoomSpeed : -zoomSpeed) * currentMapScale;
            float newScale = currentMapScale + zoomAmount;
            float clampedScale = Mathf.Clamp(newScale, minZoom, maxZoom);
            contentRectTransform.localScale = Vector3.one * clampedScale;
        }

        private void CenterMapOnIcon()
        {
            if (followIcon == null)
                return;

            float mapScale = contentRectTransform.localScale.x;
            Vector2 playerPos = WorldPositionToMapPosition(playerTransform.position);

            // If we're rotating the map, adjust the offset to counter the rotation
            if (rotateMapInstead && playerCameraTransform != null)
            {
                float rotationY = playerCameraTransform.eulerAngles.y;
                Quaternion rotation = Quaternion.Euler(0, 0, rotationY);

                // Undo the rotation for the centering
                Vector2 rotatedOffset = rotation * playerPos;
                contentRectTransform.anchoredPosition = -rotatedOffset * mapScale;
                contentRectTransform.localRotation = rotation;
                mapBorderRectTransform.localRotation = rotation;
            }
            else
            {
                contentRectTransform.anchoredPosition = -playerPos * mapScale;
                contentRectTransform.localRotation = Quaternion.identity;
                mapBorderRectTransform.localRotation = Quaternion.identity;
            }
        }

        /*private void CenterMapOnIcon()
        {
            if (followIcon == null)
                return;

            float mapScale = contentRectTransform.localScale.x;
            Vector2 playerPos = followIcon.RectTransform.anchoredPosition;

            // If rotating the map, center normally (rotation is visual only)
            contentRectTransform.anchoredPosition = -playerPos * mapScale;
        }*/

        /*private void CenterMapOnIcon()
        {
            if (followIcon != null)
            {
                float mapScale = contentRectTransform.transform.localScale.x;
                // we simply move the map in the opposite direction the player moved, scaled by the mapscale
                contentRectTransform.anchoredPosition = (-followIcon.RectTransform.anchoredPosition * mapScale);
            }
        }*/

        private void UpdateMiniMapIcons()
        {
            // scale icons by the inverse of the mapscale to keep them a consitent size
            float iconScale = 1 / contentRectTransform.transform.localScale.x;
            foreach (var kvp in miniMapWorldObjectsLookup)
            {
                var miniMapWorldObject = kvp.Key;
                var miniMapIcon = kvp.Value;

                Vector2 mapPosition;

                if (miniMapWorldObject.OverrideTransform == null)
                {
                    mapPosition = WorldPositionToMapPosition(miniMapWorldObject.transform.position);
                }
                else
                {
                    if (miniMapWorldObject.OverridePosition)
                    {
                        mapPosition = WorldPositionToMapPosition(miniMapWorldObject.OverrideTransform.position);
                    }
                    else
                    {
                        mapPosition = WorldPositionToMapPosition(miniMapWorldObject.transform.position);
                    }
                }

                //miniMapIcon.RectTransform.anchoredPosition = mapPosition;

                Vector3 rotation;

                if (miniMapWorldObject.OverrideTransform == null)
                {
                    rotation = miniMapWorldObject.transform.rotation.eulerAngles;
                }
                else
                {
                    if (miniMapWorldObject.OverrideRotation)
                    {
                        rotation = miniMapWorldObject.OverrideTransform.rotation.eulerAngles;
                    }
                    else
                    {
                        rotation = miniMapWorldObject.transform.rotation.eulerAngles;
                    }
                }

                /*if (rotateMapInsteadOfIcon && miniMapWorldObject == followIcon.WorldObject)
                {
                    miniMapIcon.IconRectTransform.localRotation = Quaternion.identity;
                    if (miniMapIcon.useAlternativeIconWhenOutsideView)
                        miniMapIcon.alternativeIconRectTransform.localRotation = Quaternion.identity;
                }
                else
                {
                    miniMapIcon.IconRectTransform.localRotation = Quaternion.AngleAxis(-rotation.y, Vector3.forward);
                    if (miniMapIcon.useAlternativeIconWhenOutsideView)
                        miniMapIcon.alternativeIconRectTransform.localRotation = Quaternion.AngleAxis(-rotation.y, Vector3.forward);
                }*/

                miniMapIcon.IconRectTransform.localRotation = GetIconRotation(miniMapWorldObject, rotation.y);
                if (miniMapIcon.alternativeIconRectTransform != null)
                    miniMapIcon.alternativeIconRectTransform.localRotation = GetIconRotation(miniMapWorldObject, rotation.y);

                Vector2 center = -contentRectTransform.anchoredPosition / contentRectTransform.localScale.x;
                Vector2 offset = mapPosition - center;

                float zoomScale = contentRectTransform.localScale.x;
                float baseRadius = Mathf.Min(scrollViewRectTransform.rect.width, scrollViewRectTransform.rect.height) * 0.5f;
                float maxRadius = baseRadius * iconMaxRadiusModifier / zoomScale;

                // Clamp only if it's beyond the visible minimap area
                bool isClamped = offset.magnitude > maxRadius;
                Vector2 clampedOffset = isClamped
                    ? offset.normalized * maxRadius
                    : offset;

                Vector2 finalPosition = center + clampedOffset;
                miniMapIcon.RectTransform.anchoredPosition = finalPosition;
                //if (miniMapIcon.useAlternativeIconWhenOutsideView)
                //    miniMapIcon.alternativeIconRectTransform.anchoredPosition = finalPosition;

                miniMapIcon.isOutsideView = isClamped;

                // Rotate toward the off-map direction if needed
                if (miniMapIcon.useAlternativeIconWhenOutsideView)
                {
                    if (offset.magnitude > maxRadius)
                    {
                        float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
                        miniMapIcon.alternativeIconRectTransform.localRotation = Quaternion.Euler(0, 0, angle - 90f);

                        // Counter-rotate the icon image itself to keep it appearing upright
                        miniMapIcon.alternativeIconImage.rectTransform.localRotation = Quaternion.Euler(0, 0, -(angle - 90f));
                    }
                    else if (!rotateMapInstead || miniMapWorldObject != followIcon.WorldObject)
                    {
                        // Use normal world rotation
                        miniMapIcon.alternativeIconRectTransform.localRotation = Quaternion.AngleAxis(-rotation.y, Vector3.forward);
                    }
                    else
                    {
                        miniMapIcon.alternativeIconRectTransform.localRotation = Quaternion.identity;
                    }
                }

                Vector3 scale = Vector3.one * iconScale;

                miniMapIcon.IconRectTransform.localScale = scale;
                if (miniMapIcon.useAlternativeIconWhenOutsideView)
                    miniMapIcon.alternativeIconRectTransform.localScale = scale;
            }
        }

        private Vector2 WorldPositionToMapPosition(Vector3 worldPos)
        {
            var pos = new Vector2(worldPos.x, worldPos.z);
            return transformationMatrix.MultiplyPoint3x4(pos);
        }

        // Updated with code from the video description
        private void CalculateTransformationMatrix()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            Vector2 minimapSize = contentRectTransform.rect.size;

            Vector2 scaleRatio = new Vector2(minimapSize.x / worldSize.x, minimapSize.y / worldSize.y);
            Vector2 translation = -worldMin * scaleRatio - (minimapSize * 0.5f);
            
            transformationMatrix = Matrix4x4.TRS(translation, Quaternion.identity, new Vector3(scaleRatio.x, scaleRatio.y, 1f));

            //  {scaleRatio.x,   0,           0,   translation.x},
            //  {  0,        scaleRatio.y,    0,   translation.y},
            //  {  0,            0,           1,            0},
            //  {  0,            0,           0,            1}
        }

        private Vector2 ClampToCircle(Vector2 pos, float radius)
        {
            if (pos.magnitude > radius)
                return pos.normalized * radius;
            return pos;
        }

        private Vector2 ClampToRect(Vector2 position, Vector2 bounds)
        {
            return new Vector2(
                Mathf.Clamp(position.x, -bounds.x, bounds.x),
                Mathf.Clamp(position.y, -bounds.y, bounds.y)
            );
        }

        private Quaternion GetIconRotation(MinimapWorldObject obj, float objectRotationY)
        {
            if (!rotateMapInstead)
                return Quaternion.Euler(0, 0, -objectRotationY);

            if (obj == followIcon.WorldObject)
                return Quaternion.Euler(0, 0, -objectRotationY);

            float minimapRotation = playerCameraTransform.eulerAngles.y;
            return Quaternion.Euler(0, 0, minimapRotation);
        }

        public void ToggleMinimapLock()
        {
            rotateMapInstead = !rotateMapInstead;
        }
    }
}