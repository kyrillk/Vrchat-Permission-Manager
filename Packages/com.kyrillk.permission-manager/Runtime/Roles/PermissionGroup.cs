using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace PermissionSystem
{
    /// <summary>
    /// Groups multiple Roles together into a single permission container.
    /// A player is considered a member if they belong to any of the grouped roles.
    /// Useful for creating composite permissions (e.g., "Staff" = Admin + Moderator roles).
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PermissionGroup : Core.PermissionContainerBase
    {
        [Tooltip("Array of roles that make up this permission group")] [SerializeField]
        private Role[] roles;


        protected override void OnManagedStart()
        {
            if (roles == null || roles.Length == 0)
            {
                LogWarning($"PermissionGroup '{permissionName}' has no roles assigned.");
                return;
            }

            foreach (Role role in roles)
            {
                role.AddUpdateListener(this);
            }
        }

        /// <summary>
        /// Called when any role in this group updates. Propagates the update to listeners.
        /// </summary>
        public override void OnPermissionsUpdated()
        {
            NotifyPermissionsUpdated();
        }

        /// <summary>
        /// Check if a specific player is a member of any role in this group
        /// </summary>
        /// <param name="playerName">The display name of the player to check</param>
        /// <returns>True if the player is a member of at least one role in the group</returns>
        public override bool IsMember(string playerName)
        {
            if (roles == null || roles.Length == 0) return false;

            foreach (Role role in roles)
            {
                if (role.IsMember(playerName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the local player is a member of any role in this group
        /// </summary>
        /// <returns>True if the local player is a member of at least one role in the group</returns>
        public override bool IsMember()
        {
            if (roles == null || roles.Length == 0) return false;

            foreach (Role role in roles)
            {
                if (role.IsMember())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get all unique members across all roles in this group
        /// </summary>
        /// <returns>Array of unique player display names who are members of any role in the group</returns>
        public override string[] GetMembers()
        {
            if (roles == null || roles.Length == 0) return new string[0];

            // First pass: count total members across all roles (with duplicates)
            int totalCount = 0;
            foreach (Role role in roles)
            {
                string[] roleMembers = role.GetMembers();
                if (roleMembers != null)
                {
                    totalCount += roleMembers.Length;
                }
            }

            if (totalCount == 0) return new string[0];

            // Pre-allocate array with worst-case size
            string[] allMembers = new string[totalCount];
            int uniqueCount = 0;

            // Second pass: collect unique members
            foreach (Role role in roles)
            {
                string[] roleMembers = role.GetMembers();
                if (roleMembers == null) continue;

                for (int i = 0; i < roleMembers.Length; i++)
                {
                    string member = roleMembers[i];
                    bool alreadyAdded = false;

                    // Check if this member is already in allMembers
                    for (int j = 0; j < uniqueCount; j++)
                    {
                        if (allMembers[j] == member)
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }

                    // Add if not already in the list
                    if (!alreadyAdded)
                    {
                        allMembers[uniqueCount] = member;
                        uniqueCount++;
                    }
                }
            }

            // If all members were unique, return the array as-is
            if (uniqueCount == totalCount) return allMembers;

            // Otherwise, trim to actual size
            string[] result = new string[uniqueCount];
            for (int i = 0; i < uniqueCount; i++)
            {
                result[i] = allMembers[i];
            }

            return result;
        }
    }
}
