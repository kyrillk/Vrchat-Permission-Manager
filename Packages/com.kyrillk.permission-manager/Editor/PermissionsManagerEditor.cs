using UnityEditor;
using UnityEngine;
using PermissionSystem;
using PermissionSystem.Core;

namespace PermissionSystem.Editor
{
    [CustomEditor(typeof(PermissionsManager))]
    public class PermissionsManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _rolesProp;
        private SerializedProperty _groupsProp;
        private SerializedProperty _allContainersProp;
        private SerializedProperty _permissionLoaderProp;
        
        private string _newRoleName = "New Role";
        private string _newGroupName = "New Group";
        private bool _showRoles = true;
        private bool _showGroups = true;
        private bool _showContainers = true;
        
        // Inline editors for roles and groups
        private UnityEditor.Editor[] _roleEditors;
        private bool[] _roleEditorFoldouts;
        private UnityEditor.Editor[] _groupEditors;
        private bool[] _groupEditorFoldouts;

        private void OnEnable()
        {
            _rolesProp = serializedObject.FindProperty("Roles");
            _groupsProp = serializedObject.FindProperty("Groups");
            _allContainersProp = serializedObject.FindProperty("AllContainers");
            _permissionLoaderProp = serializedObject.FindProperty("permissionLoader");
            
            RefreshEditors();
        }

        private void OnDisable()
        {
            CleanupEditors();
        }

        private void RefreshEditors()
        {
            CleanupEditors();
            
            // Create editors for roles - explicitly use RoleEditor
            int roleCount = _rolesProp.arraySize;
            _roleEditors = new UnityEditor.Editor[roleCount];
            _roleEditorFoldouts = new bool[roleCount];
            
            for (int i = 0; i < roleCount; i++)
            {
                var roleObj = _rolesProp.GetArrayElementAtIndex(i).objectReferenceValue;
                if (roleObj != null)
                {
                    // Explicitly create RoleEditor for Role components
                    _roleEditors[i] = CreateEditor(roleObj, typeof(RoleEditor));
                }
            }
            
            // Create editors for groups
            int groupCount = _groupsProp.arraySize;
            _groupEditors = new UnityEditor.Editor[groupCount];
            _groupEditorFoldouts = new bool[groupCount];
            
            for (int i = 0; i < groupCount; i++)
            {
                var groupObj = _groupsProp.GetArrayElementAtIndex(i).objectReferenceValue;
                if (groupObj != null)
                {
                    _groupEditors[i] = CreateEditor(groupObj);
                }
            }
        }

        private void CleanupEditors()
        {
            if (_roleEditors != null)
            {
                foreach (var editor in _roleEditors)
                {
                    if (editor != null) DestroyImmediate(editor);
                }
            }
            if (_groupEditors != null)
            {
                foreach (var editor in _groupEditors)
                {
                    if (editor != null) DestroyImmediate(editor);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            var manager = (PermissionsManager)target;
            
            DrawAutoPopulateSection(manager);
            EditorGUILayout.Space();
            DrawRolesSection(manager);
            EditorGUILayout.Space();
            DrawGroupsSection(manager);
            EditorGUILayout.Space();
            DrawContainersSection();
            EditorGUILayout.Space();
            DrawLoaderSection();
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAutoPopulateSection(PermissionsManager manager)
        {
            EditorGUILayout.LabelField("Auto-Populate Arrays", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Auto-Populate All"))
            {
                FindAllRoles(manager);
                FindAllGroups(manager);
                FindAllContainers(manager);
                RefreshEditors();
            }
        }

        private void DrawRolesSection(PermissionsManager manager)
        {
            _showRoles = EditorGUILayout.Foldout(_showRoles, $"Roles ({_rolesProp.arraySize})", true);
            if (!_showRoles) return;
            
            EditorGUI.indentLevel++;
            
            // Check if arrays match
            if (_roleEditors == null || _roleEditors.Length != _rolesProp.arraySize)
            {
                RefreshEditors();
            }
            
            for (int i = 0; i < _rolesProp.arraySize; i++)
            {
                var roleRef = _rolesProp.GetArrayElementAtIndex(i).objectReferenceValue as Component;
                if (roleRef == null) continue;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.BeginHorizontal();
                _roleEditorFoldouts[i] = EditorGUILayout.Foldout(_roleEditorFoldouts[i], roleRef.name, true);
                if (GUILayout.Button("Select", GUILayout.Width(50)))
                {
                    Selection.activeObject = roleRef;
                }
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    if (EditorUtility.DisplayDialog("Delete Role", $"Delete '{roleRef.name}' and its GameObject?", "Delete", "Cancel"))
                    {
                        var go = roleRef.gameObject;
                        _rolesProp.DeleteArrayElementAtIndex(i);
                        // DeleteArrayElementAtIndex sets to null first time if not null, call again to remove
                        if (_rolesProp.arraySize > i && _rolesProp.GetArrayElementAtIndex(i).objectReferenceValue == null)
                        {
                            _rolesProp.DeleteArrayElementAtIndex(i);
                        }
                        serializedObject.ApplyModifiedProperties();
                        Undo.DestroyObjectImmediate(go);
                        RefreshEditors();
                        GUIUtility.ExitGUI();
                        return;
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                if (_roleEditorFoldouts[i] && i < _roleEditors.Length && _roleEditors[i] != null)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    _roleEditors[i].OnInspectorGUI();
                    EditorGUILayout.EndVertical();
                }
                
                EditorGUILayout.EndVertical();
            }
            
            // Create new role at bottom of list
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            _newRoleName = EditorGUILayout.TextField(_newRoleName);
            if (GUILayout.Button("Create Role", GUILayout.Width(100)))
            {
                CreateRole(manager, _newRoleName);
                _newRoleName = "New Role";
                RefreshEditors();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }

        private void DrawGroupsSection(PermissionsManager manager)
        {
            _showGroups = EditorGUILayout.Foldout(_showGroups, $"Groups ({_groupsProp.arraySize})", true);
            if (!_showGroups) return;
            
            EditorGUI.indentLevel++;
            
            // Check if arrays match
            if (_groupEditors == null || _groupEditors.Length != _groupsProp.arraySize)
            {
                RefreshEditors();
            }
            
            for (int i = 0; i < _groupsProp.arraySize; i++)
            {
                var groupRef = _groupsProp.GetArrayElementAtIndex(i).objectReferenceValue as Component;
                if (groupRef == null) continue;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.BeginHorizontal();
                _groupEditorFoldouts[i] = EditorGUILayout.Foldout(_groupEditorFoldouts[i], groupRef.name, true);
                if (GUILayout.Button("Select", GUILayout.Width(50)))
                {
                    Selection.activeObject = groupRef;
                }
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    if (EditorUtility.DisplayDialog("Delete Group", $"Delete '{groupRef.name}' and its GameObject?", "Delete", "Cancel"))
                    {
                        var go = groupRef.gameObject;
                        _groupsProp.DeleteArrayElementAtIndex(i);
                        // DeleteArrayElementAtIndex sets to null first time if not null, call again to remove
                        if (_groupsProp.arraySize > i && _groupsProp.GetArrayElementAtIndex(i).objectReferenceValue == null)
                        {
                            _groupsProp.DeleteArrayElementAtIndex(i);
                        }
                        serializedObject.ApplyModifiedProperties();
                        Undo.DestroyObjectImmediate(go);
                        RefreshEditors();
                        GUIUtility.ExitGUI();
                        return;
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                if (_groupEditorFoldouts[i] && i < _groupEditors.Length && _groupEditors[i] != null)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    _groupEditors[i].OnInspectorGUI();
                    EditorGUILayout.EndVertical();
                }
                
                EditorGUILayout.EndVertical();
            }
            
            // Create new group at bottom of list
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            _newGroupName = EditorGUILayout.TextField(_newGroupName);
            if (GUILayout.Button("Create Group", GUILayout.Width(100)))
            {
                CreateGroup(manager, _newGroupName);
                _newGroupName = "New Group";
                RefreshEditors();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }

        private void DrawContainersSection()
        {
            _showContainers = EditorGUILayout.Foldout(_showContainers, $"All Containers ({_allContainersProp.arraySize})", true);
            if (_showContainers)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_allContainersProp, true);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawLoaderSection()
        {
            EditorGUILayout.LabelField("Permission Loader", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_permissionLoaderProp);
        }

        private Transform GetOrCreateChild(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child == null)
            {
                var go = new GameObject(childName);
                go.transform.SetParent(parent);
                go.transform.localPosition = Vector3.zero;
                Undo.RegisterCreatedObjectUndo(go, $"Create {childName}");
                child = go.transform;
            }
            return child;
        }

        private void FindAllRoles(PermissionsManager manager)
        {
            Undo.RecordObject(manager, "Find All Roles");
            
            var roles = FindObjectsOfType<Role>(true);
            
            serializedObject.Update();
            _rolesProp.ClearArray();
            foreach (var role in roles)
            {
                int index = _rolesProp.arraySize;
                _rolesProp.InsertArrayElementAtIndex(index);
                _rolesProp.GetArrayElementAtIndex(index).objectReferenceValue = role;
            }
            serializedObject.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(manager);
            Debug.Log($"[PermissionsManager] Found {roles.Length} roles.");
        }

        private void FindAllGroups(PermissionsManager manager)
        {
            Undo.RecordObject(manager, "Find All Groups");
            
            var groups = FindObjectsOfType<PermissionGroup>(true);
            
            serializedObject.Update();
            _groupsProp.ClearArray();
            foreach (var group in groups)
            {
                int index = _groupsProp.arraySize;
                _groupsProp.InsertArrayElementAtIndex(index);
                _groupsProp.GetArrayElementAtIndex(index).objectReferenceValue = group;
            }
            serializedObject.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(manager);
            Debug.Log($"[PermissionsManager] Found {groups.Length} groups.");
        }

        private void FindAllContainers(PermissionsManager manager)
        {
            Undo.RecordObject(manager, "Find All Containers");
            
            var containers = FindObjectsOfType<ManagedBehaviour>(true);
            
            serializedObject.Update();
            _allContainersProp.ClearArray();
            foreach (var container in containers)
            {
                int index = _allContainersProp.arraySize;
                _allContainersProp.InsertArrayElementAtIndex(index);
                _allContainersProp.GetArrayElementAtIndex(index).objectReferenceValue = container;
            }
            serializedObject.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(manager);
            Debug.Log($"[PermissionsManager] Found {containers.Length} managed behaviours.");
        }

        private void CreateRole(PermissionsManager manager, string roleName)
        {
            // Get or create "Roles" child
            var rolesParent = GetOrCreateChild(manager.transform, "Roles");
            
            // Create new GameObject as child of Roles
            var go = new GameObject(roleName);
            go.transform.SetParent(rolesParent);
            go.transform.localPosition = Vector3.zero;
            
            // Add Role component
            var role = go.AddComponent<Role>();
            
            // Set permission name
            var roleSo = new SerializedObject(role);
            var permNameProp = roleSo.FindProperty("permissionName");
            if (permNameProp != null)
            {
                permNameProp.stringValue = roleName;
                roleSo.ApplyModifiedProperties();
            }
            
            Undo.RegisterCreatedObjectUndo(go, "Create Role");
            
            // Add to roles array
            serializedObject.Update();
            int index = _rolesProp.arraySize;
            _rolesProp.InsertArrayElementAtIndex(index);
            _rolesProp.GetArrayElementAtIndex(index).objectReferenceValue = role;
            serializedObject.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(manager);
            Selection.activeGameObject = go;
            
            Debug.Log($"[PermissionsManager] Created role: {roleName}");
        }

        private void CreateGroup(PermissionsManager manager, string groupName)
        {
            // Get or create "Groups" child
            var groupsParent = GetOrCreateChild(manager.transform, "Groups");
            
            // Create new GameObject as child of Groups
            var go = new GameObject(groupName);
            go.transform.SetParent(groupsParent);
            go.transform.localPosition = Vector3.zero;
            
            // Add PermissionGroup component
            var group = go.AddComponent<PermissionGroup>();
            
            // Set permission name
            var groupSo = new SerializedObject(group);
            var permNameProp = groupSo.FindProperty("permissionName");
            if (permNameProp != null)
            {
                permNameProp.stringValue = groupName;
                groupSo.ApplyModifiedProperties();
            }
            
            Undo.RegisterCreatedObjectUndo(go, "Create Group");
            
            // Add to groups array
            serializedObject.Update();
            int index = _groupsProp.arraySize;
            _groupsProp.InsertArrayElementAtIndex(index);
            _groupsProp.GetArrayElementAtIndex(index).objectReferenceValue = group;
            serializedObject.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(manager);
            Selection.activeGameObject = go;
            
            Debug.Log($"[PermissionsManager] Created group: {groupName}");
        }
    }
}

