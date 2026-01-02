using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using PermissionSystem;
using System.Net.NetworkInformation;

namespace PermissionSystem.Loader
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class PermissionLoader : UdonSharpBehaviour
    {
        [Header("Permission System Settings")]
        [Tooltip("Reference to the Permission Manager")]
        public PermissionsManager permissionManager;


        [Header("Debug")]
        [Tooltip("Enable detailed logging")]
        public bool enableDebugLogging = false;

        public abstract void ChangeUrl(VRCUrl url);
        public abstract void RequestDataLoad();

        /// <summary>
        /// Loads roles and members from parsed data. Can be overridden for custom logic.
        /// </summary>
        protected virtual void LoadRolesFromData(DataDictionary data)
        {
            // Check if permission manager is configured
            if (permissionManager == null)
            {
                LogWarning("Permission manager is not configured!");
                return;
            }

            LogSuccess("Data loaded successfully! Notifying all managed behaviours...");
            
            // Notify all managed behaviours that data has been loaded, passing the data to them
            NotifyDataLoaded(data);
        }

        /// <summary>
        /// Notify all managed behaviours that data has been loaded
        /// </summary>
        protected void NotifyDataLoaded(DataDictionary data)
        {
            if (permissionManager == null) return;
            
            // Notify all roles
            if (permissionManager.Roles != null)
            {
                foreach (var role in permissionManager.Roles)
                {
                    if (role != null)
                    {
                        role.OnDataLoaded(data);
                    }
                }
            }
            
            // Notify all groups
            if (permissionManager.Groups != null)
            {
                foreach (var group in permissionManager.Groups)
                {
                    if (group != null)
                    {
                        group.OnDataLoaded(data);
                    }
                }
            }
            
            // Notify all other containers
            if (permissionManager.AllContainers != null)
            {
                foreach (var container in permissionManager.AllContainers)
                {
                    if (container != null)
                    {
                        container.OnDataLoaded(data);
                    }
                }
            }
        }


        // Logging helpers
        /// <summary>
        /// Debug log helper. Can be used by subclasses.
        /// </summary>
        protected void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"<color=cyan>[PermissionLoader]</color> {message}");
            }
        }

        /// <summary>
        /// Success log helper. Can be used by subclasses.
        /// </summary>
        protected void LogSuccess(string message)
        {
            Debug.Log($"<color=green>[PermissionLoader]</color> {message}");
        }

        /// <summary>
        /// Warning log helper. Can be used by subclasses.
        /// </summary>
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"<color=yellow>[PermissionLoader]</color> {message}");
        }

        /// <summary>
        /// Error log helper. Can be used by subclasses.
        /// </summary>
        protected void LogError(string message)
        {
            Debug.LogError($"<color=red>[PermissionLoader]</color> {message}");
        }
    }

    // RoleMapping class removed; replaced by two parallel arrays
}