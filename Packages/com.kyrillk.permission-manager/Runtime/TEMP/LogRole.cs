
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace PermissionSystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class LogRole : UdonSharpBehaviour
{
    [SerializeField] private PermissionContainer permission;

            public override void Interact()
            {
                if (permission == null)
                {
                    Debug.LogWarning("PermissionContainer is not assigned.");
                    return;
                }

                string[] members = permission.GetMembers();

                if (members == null || members.Length == 0)
                {
                    Debug.Log("Permission '" + permission.permissionName + "' has no members.");
                    return;
                }

                Debug.Log("Members of permission '" + permission.permissionName + "':");

                for (int i = 0; i < members.Length; i++)
                {
                    Debug.Log("- " + members[i]);
                }
            }
        }
}