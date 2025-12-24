
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
    public class PermissionGroup : PermissionContainer
    {
        [Tooltip("Array of roles that make up this permission group")]
        Role[] roles;

        public override void _Start()
        {
            if (roles == null || roles.Length == 0)
            {
                logWarning($"PermissionGroup '{permissionName}' has no roles assigned.");

                foreach (Role role in roles)
                {
                    if (role.manager == null)
                    {
                        role.SetManager(manager);
                    }
                    role.AddUpdateListener(this);
                }
            }
        }

        /// <summary>
        /// Check if a specific player is a member of any role in this group
        /// </summary>
        /// <param name="playerName">The display name of the player to check</param>
        /// <returns>True if the player is a member of at least one role in the group</returns>
        public override bool IsMember(string playerName)
        {
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
            // Temporary list to store unique members
            string[] tempMembers = new string[0];

            foreach (Role role in roles)
            {
                string[] roleMembers = role.GetMembers();
                if (roleMembers == null) continue;

                for (int i = 0; i < roleMembers.Length; i++)
                {
                    string member = roleMembers[i];
                    bool alreadyAdded = false;

                    // Check if this member is already in tempMembers
                    for (int j = 0; j < tempMembers.Length; j++)
                    {
                        if (tempMembers[j] == member)
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }

                    // Add if not already in the list
                    if (!alreadyAdded)
                    {
                        tempMembers = Utils.AddToStringArray(tempMembers, member);
                    }
                }
            }

            return tempMembers;
        }        
    }
}