
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

using UdonSharp;
using UnityEngine;
using VRC.Economy;
using VRC.SDK3.Data;
using VRC.SDKBase;
using PermissionSystem.Core;
using UnityEngine.UI;
using TMPro;

namespace PermissionSystem
{
    /// <summary>
    /// Displays floating indicators above players' heads based on permission membership.
    /// Extends PermissionContainerBase to use permission checking for showing indicators.
    /// Only players who are members of the required permissions will have an indicator displayed.
    /// </summary>

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerTag : PermissionAwareBehaviour
    {
        [Header("UI Components")]
        [SerializeField, Tooltip("TextMeshPro component for player name display")]
        private TextMeshProUGUI playerNameText;
        
        [SerializeField, Tooltip("Image component for the tag icon")]
        private Image tagIcon;
        [SerializeField, Tooltip("array of image for tag icon")]
        private Sprite[] tagIconArray;

        [SerializeField, Tooltip("Object with canvas, child of script")]
        private GameObject tag;
        
        [UdonSynced,SerializeField, Tooltip("Image component for the tag icon")]
        private int tagStyle = 0;
        
        [UdonSynced, Tooltip("How far above the players head you want the Indicator to float.")]
        public float heightAboveHead = 1f;

        [SerializeField, Tooltip("Whether or not you should see an indicator above yourself if you own the product.")]
        public bool showAboveLocalPlayer = true;
        
        [SerializeField, Tooltip("Whether or not you should see an indicator above OtherPlayers.")]
        public bool showAboveAllPlayers = true;

        [Tooltip("Max amount of indicators to update per frame." +
                 "This helps performance but can make the indicator look choppy if you set it too low if there are a lot of players in the instance who own the product.")]
        [Range(1, 100)]
        public int maxUpdatesPerFrame = 10;
        private int _nextIndexToUpdate;
        private int _updatesThisFrame;
        protected override string LogPrefix => "FloatingOverheadBuyIndicator";

        private VRCPlayerApi _ownerPlayer;
        private bool hasPermission;
        
        // Fallback Start() to ensure initialization even if PermissionsManager doesn't call OnManagedStart
        protected void Start()
        {
            if (requiredPermissions == null || requiredPermissions.Length == 0)
            {
                LogWarning("FloatingOverheadBuyIndicator: No required membership set. The indicator will be shown for all players.");
            }
            else
            {
                foreach (PermissionContainerBase membership in requiredPermissions)
                {
                    membership.AddUpdateListener(this);
                }
            }
            
            InitializeOwner();
        }
        
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
            
            // Set player name
            if (playerNameText != null)
            {
                playerNameText.text = _ownerPlayer.displayName;
            }
            hasPermission = HasPermission(_ownerPlayer);
            UpdateIcon();
        }
        

        // Running this in PostLateUpdate to make sure the player's position, IK pose and Animator is updated before we try to set the indicator's position.
        public override void PostLateUpdate()
        {
            if (!Utilities.IsValid(_ownerPlayer)) return;

            if (!ShouldShowIndicator())
            {
                if (tag.activeSelf)
                    tag.SetActive(false);
                return;
            }
            
            if (!tag.activeSelf)
                tag.SetActive(true);

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
                // Look TOWARD the camera (cameraPos - transform.position) so text is readable, not reversed
                Vector3 lookDir = cameraPos - transform.position;
                lookDir.y = 0; // Keep upright
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(lookDir);
                }
            }
        }
        
        protected bool ShouldShowIndicator()
        {
            if (!Utilities.IsValid(_ownerPlayer)) return false;

            if (!hasPermission) return false;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return false;

            // Check if this is the local player's indicator
            if (_ownerPlayer.isLocal)
            {
                return showAboveLocalPlayer;
            }

            // Check if other player indicators are enabled
            if (!showAboveAllPlayers) return false;
            return true;
        }
        
        public override void OnPermissionsUpdated()
        {
            UpdateIcon();
        }

        protected void UpdateIcon()
        {
            if (!Utilities.IsValid(_ownerPlayer)) return;
            if (tagStyle < 0 || !requiredPermissions[tagStyle].IsMember(_ownerPlayer))
            {
                hasPermission = false;
                return;
            }

            hasPermission = true;
            tagIcon.sprite = tagIconArray[tagStyle];
        }

        public void setshowAboveAllPlayers(bool value)
        {
            showAboveAllPlayers = value;
        }

        public void setshowAboveLocalPlayer(bool value)
        {
            showAboveLocalPlayer = value;
        }

        public void settagStyle(int tagStyle)
        {
            if (_ownerPlayer.isLocal) return;
            if (tagStyle >= 0)
            {
                if (!requiredPermissions[tagStyle].IsMember(_ownerPlayer)) return;
            }
            
            this.tagStyle = tagStyle;
            SyncBehaviour();
            UpdateIcon();
        }

        public override void OnDeserialization()
        {
            UpdateIcon();
        }
    }
}