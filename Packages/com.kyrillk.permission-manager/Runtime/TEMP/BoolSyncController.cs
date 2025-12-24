using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BoolSyncController : UdonSharpBehaviour
{
    [UdonSynced] public bool syncedBool = false; // The synced variable

    // Called by a button
    public void Interact()
    {
        // Flip the value
        syncedBool = !syncedBool;
        Debug.Log("[BoolSyncController] Bool is now: " + syncedBool);

        // Ensure local player owns the object before syncing
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        Debug.Log("[BoolSyncController] OnDeserialization called. Bool is: " + syncedBool);
    }
}
