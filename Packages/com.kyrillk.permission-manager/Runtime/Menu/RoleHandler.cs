
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


namespace PermissionSystem.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RoleHandler : PermissionInteractable
    {
        [SerializeField] private string roleName;
        private Role addingRole;
        [SerializeField] private PlayerEntry roleAssignMenu;
        protected override string Prefix => "RoleHandler " + (addingRole != null ? $"({addingRole.permissionName})" : "");
        private bool isInRole = false;
        protected override void OnPermissionGranted()
        {
            if (addingRole == null || roleAssignMenu == null)
            {
                logWarning("RoleHandler is not properly set up.");
                return;
            }
            var player = roleAssignMenu.getPlayer();
            if (isInRole)
            {
                addingRole.removeMember(player.displayName);
                isInRole = false;
                return;
            }
            addingRole.addMember(player.displayName);
            isInRole = true;
        }

        public void updateRoleStatus()
        {
            if (addingRole == null || roleAssignMenu == null)
            {
                logWarning("RoleHandler is not properly set up.");
                return;
            } 
            isInRole = addingRole.IsMember(roleAssignMenu.getPlayer().displayName);
        }

        private void addToListeners ()
        {
            if (addingRole == null)
            {
                logWarning("RoleHandler is not properly set up.");
                return;
            }
            addingRole.AddUpdateListener(this); // instance method call
        }

        public void setUpRoleHandler()
        {
            if (manager == null) return;
            addingRole = manager.GetRoleByName(roleName);
            if (addingRole == null)
            {
                logWarning("RoleHandler could not find role: " + roleName);
                return;
            }
            logInfo("Setting up RoleHandler for role: " + roleName);
            addToListeners();
            updateRoleStatus();
        }
    }
}