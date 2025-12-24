using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CubeController : UdonSharpBehaviour
{
    [SerializeField] private BoolSyncController boolController; // Reference to the synced script
    [SerializeField] private GameObject cube; // The cube to toggle

    public override void OnDeserialization()
    {
        if (boolController == null || cube == null) return;

        // Set cube active based on syncedBool
        cube.SetActive(boolController.syncedBool);
        Debug.Log("[CubeController] Cube active set to: " + boolController.syncedBool);
    }
}
