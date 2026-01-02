using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PermissionSystem;
using PermissionSystem.Core;

namespace PermissionSystem.Editor
{
    /// <summary>
    /// Custom editor for PermissionDisplay.
    /// Hides requiredPermissions and shows a dropdown to select one role or group.
    /// </summary>
    [CustomEditor(typeof(PermissionDisplay))]
    public class PermissionDisplayEditor : UnityEditor.Editor
    {
        private SerializedProperty _roleProp;
        private SerializedProperty _textProp;
        
        private List<PermissionContainerBase> _availableContainers;
        private string[] _containerNames;
        private int _selectedIndex;

        private void OnEnable()
        {
            _roleProp = serializedObject.FindProperty("role");
            _textProp = serializedObject.FindProperty("text");
            
            RefreshAvailableContainers();
            UpdateSelectedIndex();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            RefreshAvailableContainers();
            
            EditorGUILayout.LabelField("Display Settings", EditorStyles.boldLabel);
            
            // Role/Group dropdown
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Role/Group", GUILayout.Width(EditorGUIUtility.labelWidth));
            
            EditorGUI.BeginChangeCheck();
            _selectedIndex = EditorGUILayout.Popup(_selectedIndex, _containerNames);
            if (EditorGUI.EndChangeCheck())
            {
                if (_selectedIndex > 0 && _selectedIndex <= _availableContainers.Count)
                {
                    _roleProp.objectReferenceValue = _availableContainers[_selectedIndex - 1];
                }
                else
                {
                    _roleProp.objectReferenceValue = null;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Text field
            EditorGUILayout.PropertyField(_textProp, new GUIContent("Text Display"));
            
            serializedObject.ApplyModifiedProperties();
        }

        private void RefreshAvailableContainers()
        {
            _availableContainers = new List<PermissionContainerBase>();
            
            // Find all Roles
            var roles = FindObjectsOfType<Role>(true);
            foreach (var role in roles)
            {
                _availableContainers.Add(role);
            }
            
            // Find all PermissionGroups
            var groups = FindObjectsOfType<PermissionGroup>(true);
            foreach (var group in groups)
            {
                _availableContainers.Add(group);
            }
            
            // Build names array
            _containerNames = new string[_availableContainers.Count + 1];
            _containerNames[0] = "(None)";
            for (int i = 0; i < _availableContainers.Count; i++)
            {
                var container = _availableContainers[i];
                string typeName = container is Role ? "Role" : "Group";
                string permName = !string.IsNullOrEmpty(container.permissionName) ? container.permissionName : container.name;
                _containerNames[i + 1] = $"[{typeName}] {permName}";
            }
        }

        private void UpdateSelectedIndex()
        {
            var currentRole = _roleProp.objectReferenceValue as PermissionContainerBase;
            if (currentRole == null)
            {
                _selectedIndex = 0;
                return;
            }
            
            for (int i = 0; i < _availableContainers.Count; i++)
            {
                if (_availableContainers[i] == currentRole)
                {
                    _selectedIndex = i + 1;
                    return;
                }
            }
            _selectedIndex = 0;
        }
    }
}

