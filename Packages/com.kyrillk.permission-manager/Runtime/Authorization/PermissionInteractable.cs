using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


namespace PermissionSystem
{
    /// <summary>
    /// Base class for interactable objects that require permission checks.
    /// Extends PermissionContainerBase to inherit permission checking functionality.
    /// When a player interacts, automatically checks if they have required permissions
    /// and calls OnPermissionGranted or OnPermissionDenied accordingly.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class PermissionInteractable : Core.PermissionAwareBehaviour
    {
        override protected string LogPrefix => "PermissionInteractable";
        
        /// <summary>
        /// Called when permissions are updated. Override to react to permission changes.
        /// </summary>
        public override void OnPermissionsUpdated()
        {
            // Base implementation does nothing, derived classes can override
        }
        
        /// <summary>
        /// Called when a player interacts with this object.
        /// Automatically checks permissions and delegates to OnPermissionGranted or OnPermissionDenied.
        /// </summary>
        public override void Interact()
        {
            VRCPlayerApi player = Networking.LocalPlayer;
            if (manager == null || requiredPermissions == null)
            {
                LogError("Permission manager is NULL");
                return;
            }


            
            if (!HasPermission(player))
            {
                OnPermissionDenied(player);
                return;
            }

            OnPermissionGranted(player);
        }

        /// <summary>
        /// Called when the player has the required permissions. Override this for player-specific logic.
        /// By default, calls the parameterless OnPermissionGranted method.
        /// </summary>
        /// <param name="player">The player who interacted</param>
        protected virtual void OnPermissionGranted(VRCPlayerApi player)
        {
            LogInfo("Permission granted for " + player.displayName);
            OnPermissionGranted();
        }
        
        
        
        /// <summary>
        /// Implement this method to define what happens when permission is granted.
        /// This is called after permission checks pass.
        /// </summary>
        protected abstract void OnPermissionGranted();

        /// <summary>
        /// Override this method to define what happens when permission is denied.
        /// By default, does nothing (silent denial).
        /// </summary>
        /// <param name="player">The player who was denied</param>
        protected virtual void OnPermissionDenied(VRCPlayerApi player)
        {
            LogInfo("Permission denied for " + player.displayName);
        }
    }
}
