
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using PermissionSystem.Core;

namespace PermissionSystem.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerEntry : ManagedBehaviour
    {
        private VRCPlayerApi player;
        
        [SerializeField] private RoleHandler[] roleHandlers;
        [SerializeField] private TextMeshProUGUI playerNameText;

        private void updateRolesStatus()
        {
            if (roleHandlers == null)
            {
                foreach (var handler in GetComponents<RoleHandler>())
                {
                    handler.updateRoleStatus();
                }
            }
            else
            {
                foreach (var handler in roleHandlers)
                {
                    handler.updateRoleStatus();
                }
            }
        }

        public override void OnPermissionsUpdated(){
            updateRolesStatus();
        }

        public void SetPlayer(VRCPlayerApi newPlayer)
        {
            player = newPlayer;
            if (playerNameText != null)
            {
                playerNameText.text = player.displayName;
            }

            foreach (var handler in roleHandlers)
            {
                if (manager == null)
                {
                    Debug.LogWarning("PermissionsManager is not assigned in PlayerEntry.");
                    return;
                }
                handler.SetManager(manager);
                handler.setUpRoleHandler();
            }
        }

        public VRCPlayerApi getPlayer()
        {
            return player;
        }
    }
}