using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace PermissionSystem.Core
{
    /// <summary>
    /// Base class for behaviours that need to check permissions.
    /// Extends PermissionUpdateListener to receive permission updates and adds permission checking functionality.
    /// Use this for UI elements, interactables, and other permission consumers.
    /// </summary>
    public abstract class PermissionAwareBehaviour : ManagedBehaviour
    {
        [Header("Permission Settings")]
        [Tooltip("Required permission containers (Roles/Groups) that a player must be a member of")]
        [SerializeField] protected PermissionContainerBase[] requiredPermissions;

        /// <summary>
        /// Check if a player has any of the required permissions.
        /// Returns true if no permissions are set (accessible to everyone).
        /// </summary>
        protected bool HasPermission(VRCPlayerApi player)
        {
            // No permissions = accessible to everyone
            if (requiredPermissions == null || requiredPermissions.Length == 0)
            {
                return true;
            }

            // Check if player is a member of any required permission container
            string playerName = player.displayName;
            foreach (PermissionContainerBase permission in requiredPermissions)
            {
                if (permission != null && permission.IsMember(playerName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the local player has any of the required permissions.
        /// </summary>
        protected bool HasPermission()
        {
            return HasPermission(Networking.LocalPlayer);
        }


        /// <summary>
        /// Get the required permission containers
        /// </summary>
        public PermissionContainerBase[] GetRequiredPermissions()
        {
            return requiredPermissions;
        }

        /// <summary>
        /// Set the required permission containers
        /// </summary>
        public void SetRequiredPermissions(PermissionContainerBase[] permissions)
        {
            requiredPermissions = permissions;
        }
    }
}

