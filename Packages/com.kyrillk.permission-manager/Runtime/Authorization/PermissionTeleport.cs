
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace PermissionSystem
{
    /// <summary>
    /// A permission-controlled teleport interactable.
    /// Extends PermissionInteractable - only players with required permissions can teleport.
    /// Teleports the player to a specified destination Transform when interacted with.
    /// </summary>
    public class PermissionTeleport : PermissionInteractable
    {
        [Header("Set the destination Transform in the Inspector")]
        public Transform teleportDestination;
        protected override void OnPermissionGranted()
        {
            VRCPlayerApi player = Networking.LocalPlayer;

            if (player != null && teleportDestination != null)
            {
                // Teleport player to target
                player.TeleportTo(
                    teleportDestination.position,
                    teleportDestination.rotation
                );
            }
        }
    }
}
