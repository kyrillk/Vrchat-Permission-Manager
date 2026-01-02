using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PermissionSystem;
using PermissionSystem.Core;

namespace PermissionSystem.Editor
{
    /// <summary>
    /// Custom editor for all PermissionAwareBehaviour derived classes.
    /// Shows a nice UI for the requiredPermissions array with dropdowns.
    /// </summary>
    [CustomEditor(typeof(PermissionAwareBehaviour), true)]
    [CanEditMultipleObjects]
    public class PermissionAwareBehaviourEditor : UnityEditor.Editor
    {
        private SerializedProperty _requiredPermissionsProp;
        private List<PermissionContainerBase> _availableContainers;
        private string[] _containerNames;
        private int _addIndex;
        private bool _permissionsFoldout = true;

        protected virtual void OnEnable()
        {
            _requiredPermissionsProp = serializedObject.FindProperty("requiredPermissions");
            RefreshAvailableContainers();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Draw the permissions section first
            DrawPermissionsSection();
            
            EditorGUILayout.Space();
            
            // Draw the rest of the inspector (excluding requiredPermissions)
            DrawPropertiesExcluding(serializedObject, "requiredPermissions", "m_Script");
            
            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawPermissionsSection()
        {
            RefreshAvailableContainers();
            
            _permissionsFoldout = EditorGUILayout.Foldout(_permissionsFoldout, $"Required Permissions ({_requiredPermissionsProp.arraySize})", true);
            
            if (!_permissionsFoldout) return;
            
            EditorGUI.indentLevel++;
            
            // Draw existing elements
            for (int i = 0; i < _requiredPermissionsProp.arraySize; i++)
            {
                var element = _requiredPermissionsProp.GetArrayElementAtIndex(i);
                var container = element.objectReferenceValue as PermissionContainerBase;
                
                EditorGUILayout.BeginHorizontal();
                
                // Show current value with dropdown to change
                int currentIndex = GetContainerIndex(container);
                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUILayout.Popup(currentIndex, _containerNames);
                if (EditorGUI.EndChangeCheck() && newIndex > 0 && newIndex <= _availableContainers.Count)
                {
                    element.objectReferenceValue = _availableContainers[newIndex - 1];
                }
                
                // Remove button
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    _requiredPermissionsProp.DeleteArrayElementAtIndex(i);
                    // DeleteArrayElementAtIndex sets to null first, call again to remove
                    if (_requiredPermissionsProp.arraySize > i && 
                        _requiredPermissionsProp.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    {
                        _requiredPermissionsProp.DeleteArrayElementAtIndex(i);
                    }
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            // Add new element section
            EditorGUILayout.BeginHorizontal();
            _addIndex = EditorGUILayout.Popup(_addIndex, _containerNames);
            
            if (GUILayout.Button("Add", GUILayout.Width(50)))
            {
                if (_addIndex > 0 && _addIndex <= _availableContainers.Count)
                {
                    var containerToAdd = _availableContainers[_addIndex - 1];
                    
                    // Check if already in array
                    bool alreadyExists = false;
                    for (int i = 0; i < _requiredPermissionsProp.arraySize; i++)
                    {
                        if (_requiredPermissionsProp.GetArrayElementAtIndex(i).objectReferenceValue == containerToAdd)
                        {
                            alreadyExists = true;
                            break;
                        }
                    }
                    
                    if (!alreadyExists)
                    {
                        int idx = _requiredPermissionsProp.arraySize;
                        _requiredPermissionsProp.InsertArrayElementAtIndex(idx);
                        _requiredPermissionsProp.GetArrayElementAtIndex(idx).objectReferenceValue = containerToAdd;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }


        private void RefreshAvailableContainers()
        {
            _availableContainers = new List<PermissionContainerBase>();
            
            // Find all Roles
            var roles = Object.FindObjectsOfType<Role>(true);
            foreach (var role in roles)
            {
                _availableContainers.Add(role);
            }
            
            // Find all PermissionGroups
            var groups = Object.FindObjectsOfType<PermissionGroup>(true);
            foreach (var group in groups)
            {
                _availableContainers.Add(group);
            }
            
            // Build names array
            _containerNames = new string[_availableContainers.Count + 1];
            _containerNames[0] = "(Select Permission)";
            for (int i = 0; i < _availableContainers.Count; i++)
            {
                var container = _availableContainers[i];
                string typeName = container is Role ? "Role" : "Group";
                string permName = !string.IsNullOrEmpty(container.permissionName) ? container.permissionName : container.name;
                _containerNames[i + 1] = $"[{typeName}] {permName}";
            }
        }

        private int GetContainerIndex(PermissionContainerBase container)
        {
            if (container == null) return 0;
            
            for (int i = 0; i < _availableContainers.Count; i++)
            {
                if (_availableContainers[i] == container)
                {
                    return i + 1;
                }
            }
            return 0;
        }
    }
}

