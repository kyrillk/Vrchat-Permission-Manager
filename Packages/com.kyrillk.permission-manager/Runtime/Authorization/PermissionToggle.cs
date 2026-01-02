
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace PermissionSystem
{   
    /// <summary>
    /// A permission-controlled toggle switch that can activate/deactivate GameObjects.
    /// Extends PermissionInteractable - only players with required permissions can toggle.
    /// Can be set to sync globally (affects all players) or locally (only the player who toggled).
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PermissionToggle : PermissionInteractable
    {
        [Tooltip("GameObjects to toggle on/off when interacted with")]
        [SerializeField] public GameObject[] Targets;
        
        [Tooltip("Initial active state of the targets on start")]
        [SerializeField] public bool Start = false;
        
        [Tooltip("If true, toggle state is synced to all players. If false, only local player sees the change.")]
        [SerializeField] private bool IsGlobal = false;
        
        [UdonSynced] private bool IsAcive = false;

        protected override void OnManagedStart()
        {
            IsAcive = Start;
            foreach (GameObject target in Targets)
            {
                target.SetActive(IsAcive);
            }
        }

        /// <summary>
        /// Toggle the active state of all target GameObjects.
        /// If IsGlobal is true, syncs the change to all players.
        /// </summary>
        protected override void OnPermissionGranted()
        {
            IsAcive = !IsAcive;
            foreach (GameObject target in Targets)
            {
                target.SetActive(IsAcive);
            }
            if (IsGlobal)
            {
                SyncBehaviour();
            }
        }

        /// <summary>
        /// Called when synced data is received. Updates target GameObject states if global sync is enabled.
        /// </summary>
        public void OnDeserialization()
        {
            if (IsGlobal)
            {
                foreach (GameObject target in Targets)
                {
                    target.SetActive(IsAcive);
                }
            }
        }
    }
}
