using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using PermissionSystem.Loader;
using PermissionSystem.Core;

namespace PermissionSystem 
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PermissionsManager : UdonSharpBehaviour
    {
        [SerializeField] public Role[] Roles;
        [SerializeField] public PermissionGroup[] Groups;
        [HideInInspector]  public PermissionContainerBase[] Permissions;
        [SerializeField] public ManagedBehaviour[] AllContainers;
        [SerializeField] private PermissionLoader permissionLoader;
    
        void Start()
        {
            Permissions = Utils.mergePermissionContainerBaseArrays(Roles, Groups);
            
            foreach (ManagedBehaviour container in AllContainers)
            {
                container.SetManager(this);
                container.PreStart();
            }
            if (permissionLoader != null)
            {
                permissionLoader.permissionManager = this;
                permissionLoader.SendCustomEventDelayedSeconds("RequestDataLoad", 0.5f, VRC.Udon.Common.Enums.EventTiming.Update);
            }
        }

        public Role GetRoleByName(string roleName)
        {
            foreach (Role role in Roles)
            {
                if (role != null)
                {
                    if (role.permissionName == roleName)
                    {
                        return role;
                    }
                }
            }
            return null;
        }

        public PermissionGroup GetGroupByName(string groupName)
        {
            foreach (PermissionGroup group in Groups)
            {
                if (group != null)
                {
                    if (group.permissionName == groupName)
                    {
                        return group;
                    }
                }
            }
            return null;
        }

       public PermissionContainerBase GetPermissionByName(string permissionName)
        {
            foreach (PermissionContainerBase permission in Permissions)
            {
                if (permission != null)
                {
                    if (permission.permissionName == permissionName)
                    {
                        return permission;
                    }
                }
            }
            return null;
        }

        public void test()
        {
            Debug.Log("Test successful");
        }
    }
}
