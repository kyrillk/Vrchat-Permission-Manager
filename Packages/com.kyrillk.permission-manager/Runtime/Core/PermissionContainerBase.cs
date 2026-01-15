using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace PermissionSystem.Core
{
    /// <summary>
    /// Base class for permission containers (Roles, Groups).
    /// These define WHO has permissions, not who needs to check them.
    /// Extends ManagedBehaviour to integrate with the PermissionsManager.
    /// </summary>
    public abstract class PermissionContainerBase : ManagedBehaviour
    {
        [Tooltip("Display name for this permission container")]  
        public string permissionName = "New Permission Container";

        protected override string LogPrefix => permissionName;

        private ManagedBehaviour[] updateListeners;

        /// <summary>
        /// Check if a specific player name is a member of this container
        /// </summary>
        public abstract bool IsMember(string playerName);

        /// <summary>
        /// Check if a specific VRCPlayerApi is a member of this container
        /// </summary>
        public virtual bool IsMember(VRCPlayerApi player)
        {
            return IsMember(player.displayName);
        }

        /// <summary>
        /// Check if the local player is a member of this container
        /// </summary>
        public abstract bool IsMember();
        
        /// <summary>
        /// Get all members in this container
        /// </summary>
        public abstract string[] GetMembers();

        /// <summary>
        /// Get the total number of members in this container
        /// </summary>
        public virtual int GetMemberCount()
        {
            string[] members = GetMembers();
            if (members == null) return 0;
            return members.Length;
        }

        /// <summary>
        /// Notify all registered listeners that permissions have been updated
        /// </summary>
        protected void NotifyPermissionsUpdated()
        {
            if (updateListeners == null) return;

            LogInfo($"Notifying {updateListeners.Length} listeners of permission update");

            foreach (var updateListener in updateListeners)
            {
                updateListener.OnPermissionsUpdated();
            }
        }
        
        /// <summary>
        /// Register a listener to be notified when permissions change.
        /// Listener must have a public OnPermissionsUpdated() method.
        /// Works with PermissionAwareBehaviour or PermissionUpdateListener.
        /// </summary>
        public void AddUpdateListener(ManagedBehaviour listener)
        {
            if (listener == null) return;

            if (updateListeners == null)
            {
                updateListeners = new ManagedBehaviour[] { listener };
            }
            else
            {
                // Check if already registered
                for (int i = 0; i < updateListeners.Length; i++)
                {
                    if (updateListeners[i] == listener) return;
                }

                // Add to array
                ManagedBehaviour[] newArray = new ManagedBehaviour[updateListeners.Length + 1];
                updateListeners.CopyTo(newArray, 0);
                newArray[updateListeners.Length] = listener;
                updateListeners = newArray;
            }
            
            // Immediately notify the new listener
            listener.OnPermissionsUpdated();
        }

        /// <summary>
        /// Unregister a listener from permission change notifications
        /// </summary>
        public void RemoveUpdateListener(ManagedBehaviour listener)
        {
            if (listener == null || updateListeners == null) return;

            // Find and remove listener
            for (int i = 0; i < updateListeners.Length; i++)
            {
                if (updateListeners[i] == listener)
                {
                    ManagedBehaviour[] newArray = new ManagedBehaviour[updateListeners.Length - 1];
                    if (i > 0)
                    {
                        System.Array.Copy(updateListeners, 0, newArray, 0, i);
                    }
                    if (i < updateListeners.Length - 1)
                    {
                        System.Array.Copy(updateListeners, i + 1, newArray, i, updateListeners.Length - i - 1);
                    }
                    updateListeners = newArray;
                    return;
                }
            }
        }
    }
}

