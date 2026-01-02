﻿using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using System.Text;
using PermissionSystem.Core;

namespace PermissionSystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PermissionDisplay : PermissionAwareBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private PermissionContainerBase role;
        [SerializeField] private TextMeshProUGUI text;
        
        //protected PermissionContainerBase[] requiredPermissions;        

        protected override void OnManagedStart()
        {
            UpdateDisplay();
            if (role != null)
            {
                role.AddUpdateListener(this);
            }
        }

        private void UpdateDisplay()
        {
            if (role == null || text == null) return;
            string[] members = role.GetMembers();

            var sb = new StringBuilder();
            foreach (string member in members)
            {
                sb.AppendLine(member);
            }

            text.text = sb.ToString();

            LogInfo($"Updated role '{role.permissionName}' — Members: {role.GetMemberCount()}");
        }

        public override void OnPermissionsUpdated()
        {
            UpdateDisplay();
        }
    }
}