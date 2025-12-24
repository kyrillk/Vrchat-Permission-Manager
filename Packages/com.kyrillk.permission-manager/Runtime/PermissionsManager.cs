using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using PermissionSystem.Loader;


namespace PermissionSystem 
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PermissionsManager : UdonSharpBehaviour
    {
        [SerializeField] public Role[] Roles;
        [SerializeField] public PermissionGroup[] Groups;
        [HideInInspector]  public PermissionContainer[] Permissions;
        [SerializeField] public PermissionContainerBase[] AllContainers;
        [SerializeField] private PermissionLoader permissionLoader;
    
        void Start()
        {
            Permissions = Utils.mergePermissionContainerBaseArrays(Roles, Groups);

            foreach (PermissionContainer permission in Permissions)
            {
                permission.manager = this;
                permission.preStart();
            }
            foreach (PermissionContainerBase container in AllContainers)
            {
                container.SetManager(this);
                container.preStart();
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

        public PermissionContainer GetPermissionByName(string permissionName)
        {
            foreach (PermissionContainer permission in Permissions)
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
