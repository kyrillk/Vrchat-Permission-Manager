using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using PermissionSystem.Loader;

namespace VRCLinking.Modules
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PermissionLoaderVrcLinkingExtendVrcLinking : VrcLinkingModuleBase
    {
        private PermissionLoaderVrcLinking permissionLoaderVRCLinking;
        public override void OnDataLoaded()
        {
            permissionLoaderVRCLinking = GetComponent<PermissionLoaderVrcLinking>();
            if (permissionLoaderVRCLinking == null)
            {
                Debug.LogError("No PermissionLoaderVrcLinking found on the same GameObject!");
                return;
            }
            permissionLoaderVRCLinking.LoadData();
        }
    }
}