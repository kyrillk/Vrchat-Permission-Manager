
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using PermissionSystem.Core;

namespace PermissionSystem
{
    /// <summary>
    /// Toggles GameObjects based on whether the local player has the required permissions.
    /// Enables specified GameObjects if the player has the permission, disables others.
    /// Extends PermissionAwareBehaviour to check permissions and react to permission changes.
    /// </summary>  

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PermissionHasRoleToggle : PermissionAwareBehaviour
    {
        [Header("Toggle Settings")]
        [Tooltip("GameObjects to enable when the player has the required permissions")]
        [SerializeField] private GameObject[] EnabledWhenHasRole;
        
        [Tooltip("GameObjects to disable when the player has the required permissions")]
        [SerializeField] private GameObject[] DisabledWhenHasRole;

        protected override void OnManagedStart()
        {
            // Register this component to receive updates when permissions change
            if (requiredPermissions != null)
            {
                foreach (PermissionContainerBase permission in requiredPermissions)
                {
                    if (permission != null)
                    {
                        permission.AddUpdateListener(this);
                    }
                }
            }
            
            UpdateObjects();
        }

        public override void OnPermissionsUpdated()
        {
            UpdateObjects();
        }

        private void UpdateObjects()
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (localPlayer == null) return;

            bool hasPermission = HasPermission(localPlayer);

            if (EnabledWhenHasRole != null)
            {
                foreach (GameObject toEnabledWhenHasRole in EnabledWhenHasRole)
                {
                    if (toEnabledWhenHasRole != null)
                    {
                        toEnabledWhenHasRole.SetActive(hasPermission);
                    }
                }
            }

            if (DisabledWhenHasRole != null)
            {
                foreach (GameObject toDisabledWhenHasRole in DisabledWhenHasRole)
                {
                    if (toDisabledWhenHasRole != null)
                    {
                        toDisabledWhenHasRole.SetActive(!hasPermission);
                    }
                }
            }
        }
    }
}