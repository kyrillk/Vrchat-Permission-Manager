using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class LookAtLocalPlayerHead : UdonSharpBehaviour
{
    [Tooltip("Optional offset added to the head position")]
    public Vector3 headOffset;

    [Tooltip("If true, only rotates on Y axis (no up/down tilt)")]
    public bool yAxisOnly = false;

    void Update()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (localPlayer == null) return;

        Vector3 headPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        headPos += headOffset;

        if (yAxisOnly)
        {
            Vector3 lookDir = headPos - transform.position;
            lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
        else
        {
            transform.LookAt(headPos);
        }
    }
}
