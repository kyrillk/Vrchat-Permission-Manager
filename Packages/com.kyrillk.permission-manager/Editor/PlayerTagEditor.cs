using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PermissionSystem;
using PermissionSystem.Core;

namespace PermissionSystem.Editor
{
    /// <summary>
    /// Custom editor for PlayerTag that shows entries where each entry
    /// has a permission and a sprite that can be configured together.
    /// </summary>
    [CustomEditor(typeof(PlayerTag))]
    [CanEditMultipleObjects]
    public class PlayerTagEditor : UnityEditor.Editor
    {
        private SerializedProperty _requiredPermissionsProp;
        private SerializedProperty _tagIconArrayProp;
        private bool _entriesFoldout = true;
        
        private List<PermissionContainerBase> _availableContainers;
        private string[] _containerNames;
        private int _addIndex;
        
        private int _indexToDelete = -1;

        private void OnEnable()
        {
            _requiredPermissionsProp = serializedObject.FindProperty("requiredPermissions");
            _tagIconArrayProp = serializedObject.FindProperty("tagIconArray");
            RefreshAvailableContainers();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Draw the entries section
            DrawEntriesSection();
            
            EditorGUILayout.Space();
            
            // Draw the rest of the inspector (excluding already drawn properties)
            DrawPropertiesExcluding(serializedObject, 
                "requiredPermissions", 
                "tagIconArray", 
                "m_Script");
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEntriesSection()
        {
            RefreshAvailableContainers();
            
            _entriesFoldout = EditorGUILayout.Foldout(_entriesFoldout, $"Permission Tag Entries ({_requiredPermissionsProp.arraySize})", true);
            
            if (!_entriesFoldout) return;
            
            EditorGUI.indentLevel++;
            
            // Ensure arrays are synced
            SyncArraySizes();
            
            // Reset delete index
            _indexToDelete = -1;
            
            int entryCount = _requiredPermissionsProp.arraySize;
            
            if (entryCount == 0)
            {
                EditorGUILayout.HelpBox("No entries. Add an entry below to configure permission-sprite pairs.", MessageType.Info);
            }
            
            // Draw each entry
            for (int i = 0; i < entryCount; i++)
            {
                DrawEntry(i);
            }
            
            // Handle deletion after loop to avoid modifying collection while iterating
            if (_indexToDelete >= 0)
            {
                DeleteEntryAtIndex(_indexToDelete);
                _indexToDelete = -1;
            }
            
            EditorGUILayout.Space(5);
            
            // Add new entry section
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Add New Entry:", GUILayout.Width(100));
            _addIndex = EditorGUILayout.Popup(_addIndex, _containerNames);
            
            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                if (_addIndex > 0 && _addIndex <= _availableContainers.Count)
                {
                    var containerToAdd = _availableContainers[_addIndex - 1];
                    
                    // Add new entry
                    int idx = _requiredPermissionsProp.arraySize;
                    _requiredPermissionsProp.InsertArrayElementAtIndex(idx);
                    _requiredPermissionsProp.GetArrayElementAtIndex(idx).objectReferenceValue = containerToAdd;
                    
                    _tagIconArrayProp.InsertArrayElementAtIndex(idx);
                    _tagIconArrayProp.GetArrayElementAtIndex(idx).objectReferenceValue = null;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }

        private void DrawEntry(int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Entry {index}", EditorStyles.boldLabel, GUILayout.Width(60));
            GUILayout.FlexibleSpace();
            
            // Move up button
            using (new EditorGUI.DisabledScope(index == 0))
            {
                if (GUILayout.Button("▲", GUILayout.Width(25)))
                {
                    _requiredPermissionsProp.MoveArrayElement(index, index - 1);
                    _tagIconArrayProp.MoveArrayElement(index, index - 1);
                }
            }
            
            // Move down button
            using (new EditorGUI.DisabledScope(index == _requiredPermissionsProp.arraySize - 1))
            {
                if (GUILayout.Button("▼", GUILayout.Width(25)))
                {
                    _requiredPermissionsProp.MoveArrayElement(index, index + 1);
                    _tagIconArrayProp.MoveArrayElement(index, index + 1);
                }
            }
            
            // Remove button
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                _indexToDelete = index;
            }
            EditorGUILayout.EndHorizontal();
            
            // Permission dropdown
            var permissionElement = _requiredPermissionsProp.GetArrayElementAtIndex(index);
            var container = permissionElement.objectReferenceValue as PermissionContainerBase;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Permission", GUILayout.Width(80));
            
            int currentIndex = GetContainerIndex(container);
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup(currentIndex, _containerNames);
            if (EditorGUI.EndChangeCheck() && newIndex > 0 && newIndex <= _availableContainers.Count)
            {
                permissionElement.objectReferenceValue = _availableContainers[newIndex - 1];
            }
            EditorGUILayout.EndHorizontal();
            
            // Sprite field
            if (index < _tagIconArrayProp.arraySize)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Sprite", GUILayout.Width(80));
                var spriteElement = _tagIconArrayProp.GetArrayElementAtIndex(index);
                EditorGUILayout.PropertyField(spriteElement, GUIContent.none);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private void SyncArraySizes()
        {
            int permCount = _requiredPermissionsProp.arraySize;
            
            // Sync tagIconArray to match requiredPermissions
            while (_tagIconArrayProp.arraySize < permCount)
            {
                _tagIconArrayProp.InsertArrayElementAtIndex(_tagIconArrayProp.arraySize);
                _tagIconArrayProp.GetArrayElementAtIndex(_tagIconArrayProp.arraySize - 1).objectReferenceValue = null;
            }
            
            while (_tagIconArrayProp.arraySize > permCount)
            {
                _tagIconArrayProp.DeleteArrayElementAtIndex(_tagIconArrayProp.arraySize - 1);
            }
        }

        private void DeleteEntryAtIndex(int index)
        {
            // For object reference arrays in Unity:
            // - If element is NOT null: DeleteArrayElementAtIndex sets it to null (doesn't remove)
            // - If element IS null: DeleteArrayElementAtIndex actually removes it
            // So we need to null the reference first, then delete
            
            // Delete from requiredPermissions array first
            if (index < _requiredPermissionsProp.arraySize)
            {
                // First, set to null if not already null
                _requiredPermissionsProp.GetArrayElementAtIndex(index).objectReferenceValue = null;
                // Now delete the element (this will actually remove it since it's null)
                _requiredPermissionsProp.DeleteArrayElementAtIndex(index);
            }
            
            // Delete from tagIconArray at the same index
            if (index < _tagIconArrayProp.arraySize)
            {
                // First, set to null if not already null
                _tagIconArrayProp.GetArrayElementAtIndex(index).objectReferenceValue = null;
                // Now delete the element (this will actually remove it since it's null)
                _tagIconArrayProp.DeleteArrayElementAtIndex(index);
            }
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
