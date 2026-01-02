using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PermissionSystem.Editor
{
    // Editor window to manage PermissionSystem.Role components without directly referencing the runtime type.
    public class PermissionRolesManagerWindow : EditorWindow
    {
        Vector2 scrollPos;
        Component[] roles;

        [MenuItem("Window/Permission Roles Manager")]
        public static void ShowWindow()
        {
            GetWindow<PermissionRolesManagerWindow>("Roles Manager");
        }

        void OnFocus()
        {
            RefreshRoles();
        }

        void RefreshRoles()
        {
            // Find all GameObjects in the scene and collect components whose full type name is PermissionSystem.Role
            var gos = UnityEngine.Object.FindObjectsOfType<GameObject>();
            List<Component> found = new List<Component>();
            if (gos == null) { roles = new Component[0]; return; }
            foreach (var go in gos)
            {
                if (go == null) continue;
                var comps = go.GetComponents<Component>();
                if (comps == null) continue;
                foreach (var c in comps)
                {
                    if (c == null) continue;
                    var t = c.GetType();
                    if (t.FullName == "PermissionSystem.Role")
                    {
                        found.Add(c);
                    }
                }
            }

            roles = found.ToArray();
        }

        void OnGUI()
        {
            if (GUILayout.Button("Refresh Roles")) RefreshRoles();

            if (roles == null) RefreshRoles();

            EditorGUILayout.Space();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            var localRoles = roles ?? new Component[0];
            for (int i = 0; i < localRoles.Length; i++)
            {
                var comp = localRoles[i];
                if (comp == null) continue;

                // Use SerializedObject to read properties without needing the concrete type
                var so = new SerializedObject(comp);
                var permissionNameProp = so.FindProperty("permissionName");
                var jsonRoleNameProp = so.FindProperty("jsonRoleName");
                var rolesDataKeyProp = so.FindProperty("rolesDataKey");
                var membersProp = so.FindProperty("members");

                string permissionName = permissionNameProp != null ? permissionNameProp.stringValue : comp.GetType().Name;
                string jsonRoleName = jsonRoleNameProp != null ? jsonRoleNameProp.stringValue : "";
                string rolesDataKey = rolesDataKeyProp != null ? rolesDataKeyProp.stringValue : "";

                // Build members display
                string membersDisplay = "(none)";
                if (membersProp != null && membersProp.isArray && membersProp.arraySize > 0)
                {
                    List<string> vals = new List<string>();
                    for (int m = 0; m < membersProp.arraySize; m++)
                    {
                        var el = membersProp.GetArrayElementAtIndex(m);
                        if (el != null) vals.Add(el.stringValue);
                    }
                    membersDisplay = string.Join(",", vals.ToArray());
                }

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(permissionName, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("JSON Key: " + jsonRoleName);
                EditorGUILayout.LabelField("Roles Data Key: " + rolesDataKey);
                EditorGUILayout.LabelField("Members: " + membersDisplay);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select"))
                {
                    Selection.activeGameObject = comp.gameObject;
                }
                if (GUILayout.Button("Clear Members"))
                {
                    if (EditorUtility.DisplayDialog("Clear Members", "Remove all members from role?", "Yes", "No"))
                    {
                        if (membersProp != null)
                        {
                            membersProp.ClearArray();
                            so.ApplyModifiedProperties();
                            EditorUtility.SetDirty(comp);
                            var go = comp.gameObject;
                            if (go != null) EditorSceneManager.MarkSceneDirty(go.scene);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Find Roles in Scene")) RefreshRoles();
        }
    }
}
