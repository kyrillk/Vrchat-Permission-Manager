using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Data;

namespace PermissionSystem.Core
{
    /// <summary>
    /// Base class for UdonSharp behaviours that are managed by PermissionsManager.
    /// Provides basic lifecycle hooks and manager access without permission-specific logic.
    /// Use this when you need manager integration but don't need permission checking.
    /// </summary>
    public abstract class ManagedBehaviour : UdonSharpBehaviour
    {
        [Header("Manager Settings")]
        [Tooltip("The PermissionsManager that controls this behaviour")]
        [HideInInspector] public PermissionsManager manager;

        protected virtual string LogPrefix => GetType().Name;

        public void SetManager(PermissionsManager manager)
        {
            this.manager = manager;
        }
        
        public virtual void OnPermissionsUpdated()
        {
        }

        /// <summary>
        /// Called by the PermissionsManager to initialize this behaviour.
        /// </summary>
        public void PreStart()
        {
            OnManagedStart();
        }

        public virtual void OnDataLoaded(VRC.SDK3.Data.DataDictionary data){}

        /// <summary>
        /// Override this for custom initialization logic. Called after manager is set.
        /// </summary>
        protected virtual void OnManagedStart() { }
        
        /// <summary>
        /// Override this for custom update logic.
        /// </summary>
        public virtual void OnManagedUpdate() { }

        /// <summary>
        /// Synchronizes the Udon behaviour by taking ownership if needed and requesting serialization.
        /// </summary>
        protected void SyncBehaviour()
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            RequestSerialization();
        }

        #region Logging Helpers
        protected void LogInfo(string message)
        {
            Debug.Log($"<color=cyan>[{LogPrefix}]</color> {message}");
        }

        protected void LogWarning(string message)
        {
            Debug.LogWarning($"<color=yellow>[{LogPrefix}]</color> {message}");
        }

        protected void LogError(string message)
        {
            Debug.LogError($"<color=red>[{LogPrefix}]</color> {message}");
        }
        #endregion
    }
}

