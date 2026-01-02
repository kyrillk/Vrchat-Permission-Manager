using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using PermissionSystem;
using System.Net.NetworkInformation;

namespace PermissionSystem.Loader
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class PermissionLoader : UdonSharpBehaviour
    {
        [Header("Permission System Settings")]
        [Tooltip("Reference to the Permission Manager")]
        public PermissionsManager permissionManager;

        [Header("Role Mapping (add on join)")]
        [Tooltip("JSON role names (must match order of roleObjects)")]
        public string[] jsonRoleNames;

        [Tooltip("Role objects (must match order of jsonRoleNames)")]
        public Role[] roleObjects;

        [Header("Role Mapping (always add) ")]
        [Tooltip("JSON role names (must match order of roleObjects)")]
        public string[] jsonRoleNamesAlways;

        [Tooltip("Role objects (must match order of jsonRoleNames)")]
        public Role[] roleObjectsAlways;

        [Header("Data Source Settings")]
        [Tooltip("Key in JSON data that contains role information (e.g., 'GuildUsers')")]
        public string rolesDataKey = "GuildUsers";

        [Header("Debug")]
        [Tooltip("Enable detailed logging")]
        public bool enableDebugLogging = false;

        public abstract void ChangeUrl(VRCUrl url);
        public abstract void RequestDataLoad();

        /// <summary>
        /// Loads roles and members from parsed data. Can be overridden for custom logic.
        /// </summary>
        protected virtual void LoadRolesFromData(DataDictionary data)
        {
            // Check if roles data key exists
            if (!data.ContainsKey(rolesDataKey))
            {
                LogError($"Data does not contain key: {rolesDataKey}");
                return;
            }

            DataToken rolesToken = data[rolesDataKey];
            if (rolesToken.TokenType != TokenType.DataDictionary)
            {
                LogError($"'{rolesDataKey}' is not a dictionary!");
                return;
            }

            DataDictionary rolesData = rolesToken.DataDictionary;
            LogDebug($"Found {rolesData.Count} roles in data");

            // Process each role mapping using parallel arrays
            if (jsonRoleNames == null || roleObjects == null || jsonRoleNames.Length == 0 || roleObjects.Length == 0)
            {
                LogWarning("No role mappings configured!");
                return;
            }

            if (jsonRoleNames.Length != roleObjects.Length)
            {
                LogError($"jsonRoleNames and roleObjects arrays must be the same length! ({jsonRoleNames.Length} vs {roleObjects.Length})");
                return;
            }

            int rolesLoaded = 0;
            
            // Process regular role mappings (add only local player)
            for (int i = 0; i < jsonRoleNames.Length; i++)
            {
                string jsonRoleName = jsonRoleNames[i];
                Role role = roleObjects[i];

                if (role == null)
                {
                    LogWarning($"Null role object at index {i}, skipping");
                    continue;
                }
                if (string.IsNullOrEmpty(jsonRoleName))
                {
                    LogWarning($"Empty JSON role name for role '{role.permissionName}', skipping");
                    continue;
                }
                // Check if this role exists in the data
                if (!rolesData.ContainsKey(jsonRoleName))
                {
                    LogDebug($"Role '{jsonRoleName}' not found in data");
                    continue;
                }
                DataToken usersToken = rolesData[jsonRoleName];
                if (usersToken.TokenType != TokenType.DataList)
                {
                    LogWarning($"Role '{jsonRoleName}' does not contain a user list");
                    continue;
                }
                DataList usersList = usersToken.DataList;
                string[] userNames = new string[usersList.Count];
                for (int j = 0; j < usersList.Count; j++)
                {
                    if (usersList[j].TokenType == TokenType.String)
                    {
                        userNames[j] = usersList[j].String;
                    }
                    else
                    {
                        LogWarning($"Non-string value in user list for role '{jsonRoleName}'");
                        userNames[j] = "";
                    }
                }
                // Load users into the role (only local player)
                LoadUsersIntoRole(role, userNames, false);
                rolesLoaded++;
                LogDebug($"Loaded {userNames.Length} users into role '{role.permissionName}' (JSON: '{jsonRoleName}')");
            }

            // Process "always add" role mappings (add all users)
            if (jsonRoleNamesAlways != null && roleObjectsAlways != null && jsonRoleNamesAlways.Length > 0 && roleObjectsAlways.Length > 0)
            {
                if (jsonRoleNamesAlways.Length != roleObjectsAlways.Length)
                {
                    LogError($"jsonRoleNamesAlways and roleObjectsAlways arrays must be the same length! ({jsonRoleNamesAlways.Length} vs {roleObjectsAlways.Length})");
                }
                else
                {
                    for (int i = 0; i < jsonRoleNamesAlways.Length; i++)
                    {
                        string jsonRoleName = jsonRoleNamesAlways[i];
                        Role role = roleObjectsAlways[i];

                        if (role == null)
                        {
                            LogWarning($"Null role object at index {i} in always-add mappings, skipping");
                            continue;
                        }
                        if (string.IsNullOrEmpty(jsonRoleName))
                        {
                            LogWarning($"Empty JSON role name for always-add role '{role.permissionName}', skipping");
                            continue;
                        }
                        // Check if this role exists in the data
                        if (!rolesData.ContainsKey(jsonRoleName))
                        {
                            LogDebug($"Always-add role '{jsonRoleName}' not found in data");
                            continue;
                        }
                        DataToken usersToken = rolesData[jsonRoleName];
                        if (usersToken.TokenType != TokenType.DataList)
                        {
                            LogWarning($"Always-add role '{jsonRoleName}' does not contain a user list");
                            continue;
                        }
                        DataList usersList = usersToken.DataList;
                        string[] userNames = new string[usersList.Count];
                        for (int j = 0; j < usersList.Count; j++)
                        {
                            if (usersList[j].TokenType == TokenType.String)
                            {
                                userNames[j] = usersList[j].String;
                            }
                            else
                            {
                                LogWarning($"Non-string value in user list for always-add role '{jsonRoleName}'");
                                userNames[j] = "";
                            }
                        }
                        // Load ALL users into the role (not just local player)
                        LoadUsersIntoRole(role, userNames, true);
                        rolesLoaded++;
                        LogDebug($"Loaded {userNames.Length} users into always-add role '{role.permissionName}' (JSON: '{jsonRoleName}')");
                    }
                }
            }
            
            LogSuccess($"Permissions loaded successfully! Processed {rolesLoaded} roles.");
        }

        /// <summary>
        /// Loads users into a role. Can be overridden for custom member assignment logic.
        /// </summary>
        /// <param name="role">The role to add users to</param>
        /// <param name="userNames">Array of usernames to add</param>
        /// <param name="addAll">If true, adds all users. If false, only adds the local player.</param>
        protected void LoadUsersIntoRole(Role role, string[] userNames, bool addAll = false)
        {
            if (role == null)
            {
                LogWarning("Attempted to load users into a null role");
                return;
            }

            foreach (string userName in userNames)
            {
                if (!string.IsNullOrEmpty(userName))
                {
                    if (addAll)
                    {
                        // Add all users from the list
                        LogDebug($"Adding user '{userName}' to role '{role.permissionName}' (always-add)");
                        role.addMember(userName);
                    }
                    else if (Networking.LocalPlayer.displayName == userName)
                    {
                        // Only add if it's the local player
                        LogDebug($"Adding local player '{userName}' to role '{role.permissionName}'");
                        role.addMember(userName);
                    }
                }
            }
        }

        // Logging helpers
        /// <summary>
        /// Debug log helper. Can be used by subclasses.
        /// </summary>
        protected void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"<color=cyan>[PermissionLoader]</color> {message}");
            }
        }

        /// <summary>
        /// Success log helper. Can be used by subclasses.
        /// </summary>
        protected void LogSuccess(string message)
        {
            Debug.Log($"<color=green>[PermissionLoader]</color> {message}");
        }

        /// <summary>
        /// Warning log helper. Can be used by subclasses.
        /// </summary>
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"<color=yellow>[PermissionLoader]</color> {message}");
        }

        /// <summary>
        /// Error log helper. Can be used by subclasses.
        /// </summary>
        protected void LogError(string message)
        {
            Debug.LogError($"<color=red>[PermissionLoader]</color> {message}");
        }
    }

    // RoleMapping class removed; replaced by two parallel arrays
}