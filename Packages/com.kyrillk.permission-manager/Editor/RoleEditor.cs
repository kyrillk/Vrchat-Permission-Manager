using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRCLinking;
using VRCLinking.Editor;
using VRCLinkingAPI.Api;
using VRCLinkingAPI.Client;

namespace PermissionSystem.Editor
{
    [CustomEditor(typeof(Role))]
    public class RoleEditor : UnityEditor.Editor
    {
        private SerializedProperty _roleColorProp;
        private SerializedProperty _assignDefaultProp;
        private SerializedProperty _rolesDataKeyProp;
        private SerializedProperty _jsonRoleNameProp;
        private SerializedProperty _loadAllUsersProp;
        private SerializedProperty _membersProp;
        private ReorderableList _membersList;
        private string _addMemberName;
        private string _downloadStatus;
        
        // Data source options
        private static readonly string[] DataSourceOptions = { "GuildUsers (Discord)", "GroupUsers (VRChat)", "Custom" };
        private static readonly string[] DataSourceKeys = { "GuildUsers", "GroupUsers", "" };
        private int _selectedDataSourceIndex;
        private string _customDataKey = "";
        
        // Dropdown data from API - world-specific roles
        private List<string> _discordRoleNames = new List<string>();
        private bool _discordRolesLoaded;
        
        private List<string> _vrchatGroupRoleNames = new List<string>();
        private bool _vrchatRolesLoaded;
        
        private string _customRoleName = "";
        
        // Cached references
        private VrcLinkingDownloader _vrcLinkingDownloader;
        private VrcLinkingApiHelper _apiHelper;

        private void OnEnable()
        {
            _roleColorProp = serializedObject.FindProperty("roleColor");
            _assignDefaultProp = serializedObject.FindProperty("assignDefault");
            _rolesDataKeyProp = serializedObject.FindProperty("rolesDataKey");
            _jsonRoleNameProp = serializedObject.FindProperty("jsonRoleName");
            _loadAllUsersProp = serializedObject.FindProperty("loadAllUsers");
            _membersProp = serializedObject.FindProperty("members");
            _membersList = new ReorderableList(serializedObject, _membersProp, true, true, true, true);
            _membersList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Members");
            _membersList.drawElementCallback = (rect, index, _, _) =>
            {
                var element = _membersProp.GetArrayElementAtIndex(index);
                rect.y += 2;
                element.stringValue = EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element.stringValue);
            };
            _membersList.elementHeight = EditorGUIUtility.singleLineHeight + 6;
            
            // Find VrcLinkingDownloader in scene
            _vrcLinkingDownloader = FindObjectOfType<VrcLinkingDownloader>(true);
            _apiHelper = new VrcLinkingApiHelper();
            
            // Detect current data source from existing value
            string currentKey = _rolesDataKeyProp.stringValue;
            _selectedDataSourceIndex = Array.IndexOf(DataSourceKeys, currentKey);
            if (_selectedDataSourceIndex < 0)
            {
                _selectedDataSourceIndex = 2; // Custom
                _customDataKey = currentKey;
            }
            
            _customRoleName = _jsonRoleNameProp.stringValue;
            
            // Auto-load roles
            _ = LoadRolesAsync();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawRoleConfigSection();
            
            // Only show data source and members if not assigning to all players
            if (!_assignDefaultProp.boolValue)
            {
                DrawDataSourceSection();
                DrawMembersSection();
            }
            else
            {
                EditorGUILayout.HelpBox("All players are automatically members of this role.", MessageType.Info);
            }
            
            DrawHelpSection();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRoleConfigSection()
        {
            EditorGUILayout.PropertyField(_roleColorProp);
            EditorGUILayout.PropertyField(_assignDefaultProp, new GUIContent("Assign to All Players"));
            
            // Only show Load All Users if not assigning to all
            if (!_assignDefaultProp.boolValue)
            {
                EditorGUILayout.PropertyField(_loadAllUsersProp, new GUIContent("Load All Users"));
            }
            
            EditorGUILayout.Space();
        }

        private void DrawDataSourceSection()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Role Data Source", EditorStyles.boldLabel);
            
            // Data Source dropdown
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Source:", GUILayout.Width(60));
            int newDataSourceIndex = EditorGUILayout.Popup(_selectedDataSourceIndex, DataSourceOptions);
            if (newDataSourceIndex != _selectedDataSourceIndex)
            {
                _selectedDataSourceIndex = newDataSourceIndex;
                if (_selectedDataSourceIndex < 2)
                {
                    _rolesDataKeyProp.stringValue = DataSourceKeys[_selectedDataSourceIndex];
                    serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Show different UI based on selected data source
            switch (_selectedDataSourceIndex)
            {
                case 0: // GuildUsers (Discord)
                    DrawDiscordRolesSection();
                    break;
                case 1: // GroupUsers (VRChat)
                    DrawVRChatGroupRolesSection();
                    break;
                case 2: // Custom
                    DrawCustomRoleSection();
                    break;
            }
            
            EditorGUILayout.Space();
            
            // Show current configuration
            EditorGUILayout.LabelField("Current Configuration", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Data Key: " + _rolesDataKeyProp.stringValue);
            EditorGUILayout.LabelField("Role Name: " + _jsonRoleNameProp.stringValue);
            
            EditorGUILayout.Space();
        }

        private void DrawDiscordRolesSection()
        {
            EditorGUILayout.LabelField("Discord Roles (GuildUsers)", EditorStyles.boldLabel);
            
            // Check for VrcLinkingDownloader
            if (_vrcLinkingDownloader == null)
            {
                _vrcLinkingDownloader = FindObjectOfType<VrcLinkingDownloader>(true);
            }
            
            if (_vrcLinkingDownloader == null)
            {
                EditorGUILayout.HelpBox("No VrcLinkingDownloader found in scene.", MessageType.Warning);
                if (GUILayout.Button("Create VrcLinkingDownloader"))
                {
                    var newGameObject = new GameObject("VrcLinkingDownloader");
                    newGameObject.AddComponent<VrcLinkingDownloader>();
                    _vrcLinkingDownloader = newGameObject.GetComponent<VrcLinkingDownloader>();
                    Selection.activeGameObject = newGameObject;
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Enter role name manually:");
                EditorGUI.BeginChangeCheck();
                _customRoleName = EditorGUILayout.TextField(_customRoleName);
                if (EditorGUI.EndChangeCheck())
                {
                    _jsonRoleNameProp.stringValue = _customRoleName;
                    _rolesDataKeyProp.stringValue = "GuildUsers";
                }
                return;
            }
            
            // Show VrcLinkingDownloader info
            EditorGUILayout.LabelField("Server: " + (_vrcLinkingDownloader.serverName ?? "(not set)"));
            
            // Check if properly configured
            if (_vrcLinkingDownloader.worldId == Guid.Empty || string.IsNullOrEmpty(_vrcLinkingDownloader.serverId))
            {
                EditorGUILayout.HelpBox("VrcLinkingDownloader is not configured.", MessageType.Warning);
                if (GUILayout.Button("Configure VrcLinkingDownloader"))
                {
                    Selection.activeGameObject = _vrcLinkingDownloader.gameObject;
                }
                return;
            }
            
            // Fetch roles button
            if (GUILayout.Button("Fetch Discord Roles"))
            {
                _downloadStatus = "Loading Discord roles...";
                _ = LoadDiscordRolesAsync();
            }
            
            // Build dropdown options with Custom at the end
            if (_discordRolesLoaded && _discordRoleNames.Count > 0)
            {
                var options = new List<string>(_discordRoleNames);
                options.Add("(Custom)");
                
                // Find current selection - if value not in list, it's custom
                int currentIndex = _discordRoleNames.IndexOf(_jsonRoleNameProp.stringValue);
                bool isCustom = currentIndex < 0;
                if (isCustom) currentIndex = options.Count - 1;
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Role:", GUILayout.Width(60));
                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUILayout.Popup(currentIndex, options.ToArray());
                if (EditorGUI.EndChangeCheck())
                {
                    if (newIndex < _discordRoleNames.Count)
                    {
                        // Selected a role from the list
                        _jsonRoleNameProp.stringValue = _discordRoleNames[newIndex];
                        _customRoleName = _discordRoleNames[newIndex];
                        _rolesDataKeyProp.stringValue = "GuildUsers";
                        _downloadStatus = $"Selected: {_discordRoleNames[newIndex]}";
                    }
                    else
                    {
                        // Selected Custom - set to empty to trigger custom input
                        _jsonRoleNameProp.stringValue = "";
                        _customRoleName = "";
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                // Show custom input if Custom is selected (value not in list or empty)
                bool showCustomInput = newIndex >= _discordRoleNames.Count || isCustom;
                if (showCustomInput)
                {
                    EditorGUI.BeginChangeCheck();
                    _customRoleName = EditorGUILayout.TextField("Custom Role:", _customRoleName);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _jsonRoleNameProp.stringValue = _customRoleName;
                        _rolesDataKeyProp.stringValue = "GuildUsers";
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(_downloadStatus))
            {
                EditorGUILayout.HelpBox(_downloadStatus, MessageType.Info);
            }
        }

        private void DrawVRChatGroupRolesSection()
        {
            EditorGUILayout.LabelField("VRChat Group Roles (GroupUsers)", EditorStyles.boldLabel);
            
            // Check for VrcLinkingDownloader
            if (_vrcLinkingDownloader == null)
            {
                _vrcLinkingDownloader = FindObjectOfType<VrcLinkingDownloader>(true);
            }
            
            if (_vrcLinkingDownloader == null || _vrcLinkingDownloader.worldId == Guid.Empty)
            {
                EditorGUILayout.HelpBox("VrcLinkingDownloader not configured.", MessageType.Info);
            }
            
            // Fetch VRChat roles if available
            if (_vrcLinkingDownloader != null && !string.IsNullOrEmpty(_vrcLinkingDownloader.serverId))
            {
                if (GUILayout.Button("Fetch VRChat Group Roles"))
                {
                    _downloadStatus = "Loading VRChat group roles...";
                    _ = LoadVRChatGroupRolesAsync();
                }
                
                if (_vrchatRolesLoaded && _vrchatGroupRoleNames.Count > 0)
                {
                    var options = new List<string>(_vrchatGroupRoleNames);
                    options.Add("(Custom)");
                    
                    // Find current selection - if value not in list, it's custom
                    int currentIndex = _vrchatGroupRoleNames.IndexOf(_jsonRoleNameProp.stringValue);
                    bool isCustom = currentIndex < 0;
                    if (isCustom) currentIndex = options.Count - 1;
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Role:", GUILayout.Width(60));
                    EditorGUI.BeginChangeCheck();
                    int newIndex = EditorGUILayout.Popup(currentIndex, options.ToArray());
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (newIndex < _vrchatGroupRoleNames.Count)
                        {
                            // Selected a role from the list
                            _jsonRoleNameProp.stringValue = _vrchatGroupRoleNames[newIndex];
                            _customRoleName = _vrchatGroupRoleNames[newIndex];
                            _rolesDataKeyProp.stringValue = "GroupUsers";
                            _downloadStatus = $"Selected: {_vrchatGroupRoleNames[newIndex]}";
                        }
                        else
                        {
                            // Selected Custom - set to empty to trigger custom input
                            _jsonRoleNameProp.stringValue = "";
                            _customRoleName = "";
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    // Show custom input if Custom is selected (value not in list or empty)
                    bool showCustomInput = newIndex >= _vrchatGroupRoleNames.Count || isCustom;
                    if (showCustomInput)
                    {
                        EditorGUI.BeginChangeCheck();
                        _customRoleName = EditorGUILayout.TextField("Custom Role:", _customRoleName);
                        if (EditorGUI.EndChangeCheck())
                        {
                            _jsonRoleNameProp.stringValue = _customRoleName;
                            _rolesDataKeyProp.stringValue = "GroupUsers";
                        }
                    }
                }
                else
                {
                    // No roles loaded yet, show manual input
                    EditorGUI.BeginChangeCheck();
                    _customRoleName = EditorGUILayout.TextField("Role Name:", _customRoleName);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _jsonRoleNameProp.stringValue = _customRoleName;
                        _rolesDataKeyProp.stringValue = "GroupUsers";
                    }
                }
            }
            else
            {
                // No VrcLinkingDownloader, show manual input
                EditorGUI.BeginChangeCheck();
                _customRoleName = EditorGUILayout.TextField("Role Name:", _customRoleName);
                if (EditorGUI.EndChangeCheck())
                {
                    _jsonRoleNameProp.stringValue = _customRoleName;
                    _rolesDataKeyProp.stringValue = "GroupUsers";
                }
            }
            
            if (!string.IsNullOrEmpty(_downloadStatus))
            {
                EditorGUILayout.HelpBox(_downloadStatus, MessageType.Info);
            }
        }

        private void DrawCustomRoleSection()
        {
            EditorGUILayout.LabelField("Custom Data Source", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Data key (e.g., 'MyCustomRoles'):");
            EditorGUI.BeginChangeCheck();
            _customDataKey = EditorGUILayout.TextField(_customDataKey);
            if (EditorGUI.EndChangeCheck())
            {
                _rolesDataKeyProp.stringValue = _customDataKey;
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Role name:");
            EditorGUI.BeginChangeCheck();
            _customRoleName = EditorGUILayout.TextField(_customRoleName);
            if (EditorGUI.EndChangeCheck())
            {
                _jsonRoleNameProp.stringValue = _customRoleName;
            }
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                if (_vrcLinkingDownloader == null || _vrcLinkingDownloader.worldId == Guid.Empty || string.IsNullOrEmpty(_vrcLinkingDownloader.serverId))
                {
                    _downloadStatus = "VrcLinkingDownloader not configured.";
                    Repaint();
                    return;
                }

                if (!(await _apiHelper.IsUserLoggedIn()))
                {
                    _downloadStatus = "Not logged in. Please log in via VrcLinkingDownloader.";
                    Repaint();
                    return;
                }

                var config = new Configuration();
                config.ApiKey.Add("Bearer", EditorPrefs.GetString("VRCLinking_ApiToken", ""));
                var worldsApi = new WorldsApi(config);
                
                var response = await worldsApi.GetGuildWorldsAsync(_vrcLinkingDownloader.serverId);
                
                if (response == null)
                {
                    _downloadStatus = "Could not load world data.";
                    Repaint();
                    return;
                }

                var worldSettings = response.Worlds?.FirstOrDefault(w => w.Id == _vrcLinkingDownloader.worldId);

                // Get Discord roles - use world-specific encode roles
                _discordRoleNames.Clear();
                
                if (worldSettings == null)
                {
                    _downloadStatus = "World not found.";
                    _discordRolesLoaded = false;
                }
                else if (!worldSettings.IncludeDiscordData)
                {
                    _downloadStatus = "Discord not enabled. Enable 'Include Discord Data' on VRCLinking website.";
                    _discordRolesLoaded = false;
                }
                else
                {
                    if (worldSettings.DiscordEncodeRoles != null)
                    {
                        foreach (var role in worldSettings.DiscordEncodeRoles)
                        {
                            if (!string.IsNullOrEmpty(role.RoleName) && !_discordRoleNames.Contains(role.RoleName))
                            {
                                _discordRoleNames.Add(role.RoleName);
                            }
                        }
                    }
                    
                    if (worldSettings.DiscordAlwaysEncodeRoles != null)
                    {
                        foreach (var role in worldSettings.DiscordAlwaysEncodeRoles)
                        {
                            if (!string.IsNullOrEmpty(role.RoleName) && !_discordRoleNames.Contains(role.RoleName))
                            {
                                _discordRoleNames.Add(role.RoleName);
                            }
                        }
                    }
                    
                    _discordRolesLoaded = _discordRoleNames.Count > 0;
                    
                    if (!_discordRolesLoaded)
                    {
                        _downloadStatus = "No Discord roles configured. Add roles on VRCLinking website.";
                    }
                }

                // Get VRChat group roles - use world-specific encode roles
                _vrchatGroupRoleNames.Clear();
                
                if (worldSettings == null)
                {
                    _vrchatRolesLoaded = false;
                }
                else if (!worldSettings.IncludeGroupData)
                {
                    _downloadStatus = "VRChat groups not enabled. Enable 'Include Group Data' on VRCLinking website.";
                    _vrchatRolesLoaded = false;
                }
                else
                {
                    if (worldSettings.GroupEncodeRoles != null)
                    {
                        foreach (var role in worldSettings.GroupEncodeRoles)
                        {
                            if (!string.IsNullOrEmpty(role.RoleName) && !_vrchatGroupRoleNames.Contains(role.RoleName))
                            {
                                _vrchatGroupRoleNames.Add(role.RoleName);
                            }
                        }
                    }
                    
                    if (worldSettings.GroupAlwaysEncodeRoles != null)
                    {
                        foreach (var role in worldSettings.GroupAlwaysEncodeRoles)
                        {
                            if (!string.IsNullOrEmpty(role.RoleName) && !_vrchatGroupRoleNames.Contains(role.RoleName))
                            {
                                _vrchatGroupRoleNames.Add(role.RoleName);
                            }
                        }
                    }
                    
                    _vrchatRolesLoaded = _vrchatGroupRoleNames.Count > 0;
                    
                    if (!_vrchatRolesLoaded)
                    {
                        _downloadStatus = "No VRChat group roles configured. Add roles on VRCLinking website.";
                    }
                }

                // Final status
                if (_discordRolesLoaded || _vrchatRolesLoaded)
                {
                    _downloadStatus = $"Loaded {_discordRoleNames.Count} Discord roles, {_vrchatGroupRoleNames.Count} VRChat roles.";
                }
            }
            catch (Exception e)
            {
                _downloadStatus = "Error: " + e.Message;
                Debug.LogError("[RoleEditor] " + e);
            }
            
            Repaint();
        }

        private async Task LoadDiscordRolesAsync()
        {
            await LoadRolesAsync();
        }

        private async Task LoadVRChatGroupRolesAsync()
        {
            await LoadRolesAsync();
        }

        private void DrawMembersSection()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Manual Members", EditorStyles.boldLabel);
            _membersList.DoLayoutList();
            
            EditorGUILayout.BeginHorizontal();
            _addMemberName = EditorGUILayout.TextField(_addMemberName);
            if (GUILayout.Button("Add", GUILayout.Width(60)))
            {
                if (!string.IsNullOrEmpty(_addMemberName))
                {
                    int newIndex = _membersProp.arraySize;
                    _membersProp.InsertArrayElementAtIndex(newIndex);
                    _membersProp.GetArrayElementAtIndex(newIndex).stringValue = _addMemberName;
                    _addMemberName = "";
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Clear All Members"))
            {
                if (EditorUtility.DisplayDialog("Clear Members", "Remove all members?", "Yes", "No"))
                {
                    _membersProp.ClearArray();
                }
            }
            
            EditorGUILayout.Space();
        }

        private void DrawHelpSection()
        {
            EditorGUILayout.LabelField("Quick Tips:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• GuildUsers = Discord server roles");
            EditorGUILayout.LabelField("• GroupUsers = VRChat group roles");
            EditorGUILayout.LabelField("• Custom = Your own data structure");
        }
    }
}
