using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using System.Text;

namespace PermissionSystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PermissionDisplay : PermissionContainerBase
    {
        [Header("Display Settings")]
        [SerializeField] private GameObject displayObject;
        [SerializeField] private PermissionContainer role;
        [SerializeField] private TextMeshProUGUI text;
        protected override string Prefix => "PermissionDisplay " + (role != null ? $"({role.permissionName})" : "");

        public override void _Start()
        {
            UpdateDisplay();
            role.AddUpdateListener(this);
        }

        private void UpdateDisplay()
        {
            if (displayObject == null || role == null || text == null) return;
            string[] members = role.GetMembers();

            var sb = new StringBuilder();
            foreach (string member in members)
            {
                sb.AppendLine(member);
            }

            text.text = sb.ToString();

            logInfo($"Updated role '{role.name}' — Members: {role.GetMemberCount()}");
        }

        public override void PermissionsUpdate()
        {
            UpdateDisplay();
        }
    }
}