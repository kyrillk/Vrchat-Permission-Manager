using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using TMPro;
using UnityEngine.UI;

namespace PermissionSystem
{
    /// <summary>
    /// Displays a floating indicator above a player's head.
    /// Uses VRChat's Player Object system - each instance is automatically assigned to a player.
    /// The owner player is determined via Networking.GetOwner().
    /// External scripts can set the icon image and color.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FloatingOverheadBuyIndicator : UdonSharpBehaviour
    {
        [Header("UI Components")]
        [Tooltip("TextMeshPro component for player name display")]
        [SerializeField] private TextMeshProUGUI playerNameText;

        [Tooltip("Image component for the tag icon")]
        public Image tagIcon;

        [Header("Position Settings")]
        [Tooltip("How far above the player's head the indicator floats")]
        public float heightAboveHead = 1f;

        [Header("Visibility Settings")]
        [Tooltip("Whether the local player can see their own indicator")]
        public bool showIndicatorAboveLocalPlayer = true;

        [Tooltip("Whether indicators are shown above other players")]
        public bool showIndicatorAboveAllPlayers = true;

        [Tooltip("Maximum distance at which the indicator is visible")]
        public float maxVisibleDistance = 20f;

        private int _nextIndexToUpdate;
        private int _updatesThisFrame;


        [Header("Style Settings")]
        [Tooltip("Current tag style definition")]
        public TagStyleDefinition tagStyle;

        // The player who owns this indicator (auto-assigned via Player Object system)
        private VRCPlayerApi _ownerPlayer;
        private bool _isInitialized;
        private CanvasGroup _canvasGroup;

        private void Start()
        {
            // Try to get CanvasGroup for fading, add one if not present
            _canvasGroup = GetComponent<CanvasGroup>();
            
            // Initialize with owner if already assigned
            InitializeOwner();
        }

        /// <summary>
        /// Called when the owner of this object changes (Player Object assignment)
        /// </summary>
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            InitializeOwner();
        }

        private void InitializeOwner()
        {
            _ownerPlayer = Networking.GetOwner(gameObject);
            
            if (!Utilities.IsValid(_ownerPlayer))
            {
                gameObject.SetActive(false);
                return;
            }

            _isInitialized = true;

            // Set player name
            if (playerNameText != null)
            {
                playerNameText.text = _ownerPlayer.displayName;
            }

            // Apply initial style if set
            if (tagStyle != null)
            {
                ApplyStyle();
            }

            UpdateVisibilityState();
        }

        // public override void PostLateUpdate()
        // {
        //     if (!_isInitialized || !Utilities.IsValid(_ownerPlayer)) return;
        //
        //     // Check visibility conditions
        //     bool shouldShow = ShouldShowIndicator();
        //      
        //     if (!shouldShow)
        //     {
        //         if (gameObject.activeSelf)
        //             gameObject.SetActive(false);
        //         return;
        //     }
        // }

        // Running this in PostLateUpdate to make sure the player's position, IK pose and Animator is updated before we try to set the indicator's position.
        public override void PostLateUpdate()
        {
            if (!Utilities.IsValid(_ownerPlayer)) return;

            // Position above the player's head
            Vector3 headPos = _ownerPlayer.GetBonePosition(HumanBodyBones.Head);
            if (headPos.Equals(Vector3.zero))
            {
                // Fallback to player position if head bone not available
                headPos = _ownerPlayer.GetPosition() + Vector3.up * 1.5f;
            }
            transform.position = headPos + Vector3.up * heightAboveHead;

            // Billboard - always face the local player's camera with text readable (not mirrored)
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (Utilities.IsValid(localPlayer))
            {
                Vector3 cameraPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                Vector3 lookDir = transform.position - cameraPos;
                lookDir.y = 0; // Keep upright
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(lookDir);
                }
            }
        }

        private bool ShouldShowIndicator()
        {
            if (!Utilities.IsValid(_ownerPlayer)) return false;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return false;

            // Check if this is the local player's indicator
            if (_ownerPlayer.isLocal)
            {
                return showIndicatorAboveLocalPlayer;
            }

            // Check if other player indicators are enabled
            if (!showIndicatorAboveAllPlayers) return false;

            // Check distance
            float distance = Vector3.Distance(localPlayer.GetPosition(), _ownerPlayer.GetPosition());
            if (distance > maxVisibleDistance) return false;

            return true;
        }

        private void UpdateVisibilityState()
        {
            bool shouldShow = ShouldShowIndicator();
            gameObject.SetActive(shouldShow);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (Utilities.IsValid(_ownerPlayer) && player.playerId == _ownerPlayer.playerId)
            {
                _isInitialized = false;
                gameObject.SetActive(false);
            }
        }

        #region Public API - Set Icon and Color

        /// <summary>
        /// Set the icon sprite displayed on this tag
        /// </summary>
        public void SetIcon(Sprite icon)
        {
            if (tagIcon != null)
            {
                tagIcon.sprite = icon;
                tagIcon.enabled = icon != null;
            }
        }

        /// <summary>
        /// Set the icon color/tint
        /// </summary>
        public void SetIconColor(Color color)
        {
            if (tagIcon != null)
            {
                tagIcon.color = color;
            }
        }

        /// <summary>
        /// Set both icon and color at once
        /// </summary>
        public void SetIconAndColor(Sprite icon, Color color)
        {
            SetIcon(icon);
            SetIconColor(color);
        }

        /// <summary>
        /// Set the player name text color
        /// </summary>
        public void SetNameColor(Color color)
        {
            if (playerNameText != null)
            {
                playerNameText.color = color;
            }
        }

        /// <summary>
        /// Set the background color (requires Image component on this GameObject)
        /// </summary>
        public void SetBackgroundColor(Color color)
        {
            Image background = GetComponent<Image>();
            if (background != null)
            {
                background.color = color;
            }
        }

        /// <summary>
        /// Get the player who owns this indicator
        /// </summary>
        public VRCPlayerApi GetOwnerPlayer()
        {
            return _ownerPlayer;
        }

        /// <summary>
        /// Check if this indicator belongs to the local player
        /// </summary>
        public bool IsLocalPlayerIndicator()
        {
            return Utilities.IsValid(_ownerPlayer) && _ownerPlayer.isLocal;
        }

        #endregion

        #region Visibility Settings

        public void SetShowIndicatorAboveAllPlayers(bool value)
        {
            showIndicatorAboveAllPlayers = value;
            UpdateVisibilityState();
        }

        public void SetShowIndicatorAboveLocalPlayer(bool value)
        {
            showIndicatorAboveLocalPlayer = value;
            UpdateVisibilityState();
        }

        public void SetMaxVisibleDistance(float distance)
        {
            maxVisibleDistance = distance;
        }
        

        #endregion

        #region Tag Style Methods

        /// <summary>
        /// Set the tag style and update the indicator appearance
        /// </summary>
        public void SetTagStyle(TagStyleDefinition newStyle)
        {
            tagStyle = newStyle;
            ApplyStyle();
        }

        /// <summary>
        /// Get the current tag style
        /// </summary>
        public TagStyleDefinition GetTagStyle()
        {
            return tagStyle;
        }

        /// <summary>
        /// Apply the current style to the indicator
        /// </summary>
        public void ApplyStyle()
        {
            if (tagStyle == null) return;

            // Apply icon settings
            if (tagIcon != null)
            {
                if (tagStyle.tagIcon != null)
                {
                    tagIcon.sprite = tagStyle.tagIcon;
                    tagIcon.enabled = true;
                }
                tagIcon.color = tagStyle.iconColor;
                tagIcon.transform.localScale = Vector3.one * tagStyle.iconScale;
            }

            // Apply text settings
            if (playerNameText != null)
            {
                // playerNameText.fontSize = tagStyle.nameFontSize;
                playerNameText.color = tagStyle.nameColor;
                // Note: TMP outline is controlled via material properties or component settings
                // outlineColor and outlineWidth need to be set through the material if needed
            }

            // Apply background settings
            Image background = GetComponent<Image>();
            if (background != null)
            {
                if (tagStyle.backgroundSprite != null)
                    background.sprite = tagStyle.backgroundSprite;
                background.color = tagStyle.backgroundColor;

                RectTransform rt = GetComponent<RectTransform>();
                if (rt != null)
                    rt.sizeDelta = tagStyle.backgroundSize;
            }

            // Apply visibility settings from style
            maxVisibleDistance = tagStyle.maxVisibleDistance;
        }

        /// <summary>
        /// Force refresh the style
        /// </summary>
        public void RefreshStyle()
        {
            ApplyStyle();
        }

        #endregion
    }
}


