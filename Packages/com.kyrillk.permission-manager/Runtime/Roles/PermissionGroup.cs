
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

        public override void _Start()
        {
            if (RequiredMembership == null || RequiredMembership.Length == 0) {
                logWarning($"PermissionGroup '{permissionName}' has no roles assigned.");
                return;
            }


            foreach (Role role in RequiredMembership)
            {
                if (role.manager == null)
                {
                    role.SetManager(manager);
                }
                role.AddUpdateListener(this);
            }
        }

        /// <summary>
        /// Check if a specific player is a member of any role in this group
        /// </summary>
        /// <param name="playerName">The display name of the player to check</param>
        /// <returns>True if the player is a member of at least one role in the group</returns>
        public override bool IsMember(string playerName)
        {
            if (RequiredMembership == null || RequiredMembership.Length == 0) return false;
            
            foreach (Role role in RequiredMembership)
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
            if (RequiredMembership == null || RequiredMembership.Length == 0) return false;
            
            foreach (Role role in RequiredMembership)
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
            if (RequiredMembership == null || RequiredMembership.Length == 0) return new string[0];
            
            // Temporary list to store unique members
            string[] tempMembers = new string[0];

            foreach (Role role in RequiredMembership)
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