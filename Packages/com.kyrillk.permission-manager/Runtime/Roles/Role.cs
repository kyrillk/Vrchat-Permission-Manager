using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace PermissionSystem
{
    /// <summary>
    /// Represents a role in the permission system that can have members assigned to it.
    /// Roles are the fundamental unit of permissions - players can be added/removed from roles.
    /// Extends PermissionContainer to provide member management functionality.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Role : Core.PermissionContainerBase
    {
        [Tooltip("Display color for this role in UI elements")]
        public Color32 roleColor = new Color32(52,152,219,255);

        [Tooltip("If true, all players are automatically considered members of this role")]
        [SerializeField] private bool assignDefault = false;

        //[SerializeField] private TagSettings tag;

        private VRCPlayerApi localPlayer;
        private bool isLocalInRole;
        
        [Tooltip("Array of player display names who are members of this role")]
        [SerializeField, UdonSynced] private string[] members;

    
        /// <summary>
        /// Check if the local player is a member of this role
        /// </summary>
        public override bool IsMember()
        {
            return isLocalInRole;
        }

        /// <summary>
        /// Check if a specific player is a member of this role
        /// </summary>
        /// <param name="playerName">The display name of the player to check</param>
        /// <returns>True if the player is a member or if assignDefault is true</returns>
        public override bool IsMember(string playerName)
        {
            if (assignDefault) return true;
            if (members == null) return false;
            for (int i = 0; i < members.Length; i++)
            {
                if (members[i] == playerName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Add a player to this role. Automatically syncs the change.
        /// </summary>
        /// <param name="playerName">The display name of the player to add</param>
        public void addMember(string playerName)
        {
            if (members == null) members = new string[0];
            if (IsMember(playerName))
            {
                // Already a member
                return;
            }

            members = Utils.AddToStringArray(members, playerName);
            NotifyPermissionsUpdated();

            SyncBehaviour();
            LogInfo("Adding member " + playerName + " to role " + permissionName);
        }
        
        /// <summary>
        /// Remove a player from this role. Automatically syncs the change.
        /// </summary>
        /// <param name="playerName">The display name of the player to remove</param>
        public void removeMember(string playerName)
        {
            members = Utils.RemoveFromStringArray(members, playerName);
            NotifyPermissionsUpdated();

            SyncBehaviour();
            LogInfo("Removing member " + playerName + " from role " + permissionName);
        }

        /// <summary>
        /// Get all members of this role
        /// </summary>
        /// <returns>Array of player display names who are members</returns>
        public override string[] GetMembers()
        {
            if (assignDefault)
            {
                int count = VRCPlayerApi.GetPlayerCount();
                var players = new VRCPlayerApi[count];
                VRCPlayerApi.GetPlayers(players);

                string[] names = new string[count];
                for (int i = 0; i < count; i++)
                {
                    names[i] = players[i].displayName;
                }

                return names;
            }

            return members;
        }

        protected override void OnManagedStart()
        {
            localPlayer = Networking.LocalPlayer;
            isLocalInRole = IsMember(localPlayer.displayName);
        }

        public override void OnDeserialization()
        {
            isLocalInRole = IsMember(localPlayer.displayName);

            NotifyPermissionsUpdated();
        }




    }
}