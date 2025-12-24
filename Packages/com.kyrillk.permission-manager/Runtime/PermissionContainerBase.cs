using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace PermissionSystem
{
    /// <summary>
    /// Base class for all permission-managed UdonSharp behaviours.
    /// Provides core functionality for permission checking, manager access, and synchronization.
    /// All permission-aware objects in the system should inherit from this class.
    /// </summary>
    public abstract class PermissionContainerBase : UdonSharpBehaviour
    {
        [Header("Permission Settings")]
        [Tooltip("The PermissionsManager that controls this behaviour")]
        [HideInInspector] public PermissionsManager manager;
        
        [Tooltip("Required permission containers (Roles/Groups) that a player must be a member of to interact")]
        [SerializeField] protected PermissionContainer[] RequiredMembership;
        protected virtual string Prefix => "temp";

        /// <summary>
        /// Called when the permissions for this behaviour are updated.
        /// Override this method to handle permission changes.
        /// </summary>
        public virtual void PermissionsUpdate(){}

        /// <summary>
        /// Called by the PermissionsManager to initialize this behaviour.
        /// Sets up the logger and calls the virtual _Start method.
        /// </summary>
        public void preStart()
        {
            _Start();
        }
        
        /// <summary>
        /// Override this for custom start logic. Called after manager and logger are initialized.
        /// </summary>
        public virtual void _Start(){}
        
        /// <summary>
        /// Override this for custom update logic.
        /// </summary>
        public virtual void _Update(){}

        /// <summary>
        /// Check if a player has any of the required permissions to access this behaviour.
        /// Returns true if no permissions are set (accessible to everyone).
        /// Returns true if player is a member of at least one required permission container.
        /// </summary>
        /// <param name="player">The player to check permissions for</param>
        /// <returns>True if the player has required permissions or no permissions are set</returns>
        protected bool HasRequiredPermission(VRCPlayerApi player)
        {
            logInfo("Checking permissions for " + player.displayName);

            // If no permissions are set, accessible to everyone
            if (RequiredMembership == null || RequiredMembership.Length == 0)
            {
                return true;
            }

            // Check if player is a member of any required permission container
            foreach (PermissionContainer permission in RequiredMembership)
            {
                if (permission != null && permission.IsMember(player.displayName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Synchronizes the Udon behaviour by taking ownership if needed and requesting serialization.
        /// Use this when synced variables need to be updated across the network.
        /// </summary>
        public void Sync()
        {
            if (Networking.IsOwner(gameObject) == false)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            RequestSerialization();
        }
        
        /// <summary>
        /// Gets the current PermissionsManager assigned to this behaviour
        /// </summary>
        /// <returns>The PermissionsManager instance</returns>
        public PermissionsManager GetManager()
        {
            return manager;
        }

        /// <summary>
        /// Sets the PermissionsManager for this behaviour
        /// </summary>
        /// <param name="newManager">The new PermissionsManager to assign</param>
        public void SetManager(PermissionsManager newManager)
        {
            manager = newManager;
        }

        /// <summary>
        /// Test method for debugging purposes
        /// </summary>
        public void test()
        {
            Debug.Log("Test successful");
        }

        protected void logInfo(string message)
        {
            Debug.Log($"<color=cyan>[{Prefix}]</color> {message}");
        }

        protected void logWarning(string message)
        {
            Debug.LogWarning($"<color=yellow>[{Prefix}]</color> {message}");
        }

        protected void logError(string message)
        {
            Debug.LogError($"<color=red>[{Prefix}]</color> {message}");
        }
    }
}