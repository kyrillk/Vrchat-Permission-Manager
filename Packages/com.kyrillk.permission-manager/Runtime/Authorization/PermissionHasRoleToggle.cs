
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


namespace PermissionSystem
{
    /// <summary>
    /// Toggles GameObjects based on whether the local player has the required role.
    /// Enables specified GameObjects if the player has the role, disables others.
    /// </summary>  

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PermissionHasRoleToggle : PermissionContainerBase
    {
        [SerializeField] private GameObject[] EnabledWhenHasRole;
        [SerializeField] private GameObject[] DisabledWhenHasRole;
        public override void _Start()
        {
            foreach (PermissionContainer RequiredMembership in RequiredMembership)
            {
                RequiredMembership.AddUpdateListener(this);
            }
            updateObjects();
        }

        public override void PermissionsUpdate()
        {
            updateObjects();
        }

        private void updateObjects()
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (localPlayer == null) return;

            bool hasPermission = HasRequiredPermission(localPlayer);

            foreach (GameObject toEnabledWhenHasRole in EnabledWhenHasRole)
            {
                toEnabledWhenHasRole.SetActive(hasPermission);
            }

            foreach (GameObject toDisabledWhenHasRole in DisabledWhenHasRole)
            {
                toDisabledWhenHasRole.SetActive(!hasPermission);
            }
        }
    }
}