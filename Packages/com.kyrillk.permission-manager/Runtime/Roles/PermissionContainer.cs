
using System.Security.Cryptography.X509Certificates;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace PermissionSystem 
{
    /// <summary>
    /// Base class for permission containers like Roles and PermissionGroups.
    /// Extends PermissionContainerBase to inherit permission checking and manager functionality.
    /// </summary>
    public abstract class PermissionContainer : PermissionContainerBase
    {
        [Tooltip("Display name for this permission container")]  
        public string permissionName = "New Permission Container";

        [HideInInspector] protected override string Prefix => permissionName;

        private PermissionContainerBase[] updateListeners;

        /// <summary>
        /// Check if a specific player name is a member of this container
        /// </summary>
        public abstract bool IsMember(string playerName);
        
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

        public override void PermissionsUpdate()
        {
            // Notify all registered listeners about the permission update
            if (updateListeners == null) return;

            for (int i = 0; i < updateListeners.Length; i++)
            {
                if (updateListeners[i] != null)
                {
                    updateListeners[i].PermissionsUpdate();
                }
            }
        }
        
        /// <summary>
        /// Register a behaviour to be notified when this role's data is deserialized (members change)
        /// </summary>
        /// <param name="listener">The PermissionContainerBase to notify on deserialization</param>
        public void AddUpdateListener(PermissionContainerBase listener)
        {
            if (listener == null) return;

            updateListeners = Utils.AddToPermissionContainerBaseArray(updateListeners, listener);
            listener.PermissionsUpdate();
        }

        /// <summary>
        /// Unregister a behaviour from deserialization notifications
        /// </summary>
        /// <param name="listener">The PermissionContainerBase to stop notifying</param>
        public void RemoveUpdateListener(PermissionContainerBase listener)
        {
            if (listener == null) return;
            updateListeners = Utils.RemoveFromPermissionContainerBaseArray(updateListeners, listener);
        }


        // public abstract 
    }
}