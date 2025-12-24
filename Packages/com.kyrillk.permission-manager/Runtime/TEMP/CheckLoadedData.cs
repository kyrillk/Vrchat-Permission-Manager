
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using VRCLinking;
using VRCLinking.Modules;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class CheckLoadedData : VrcLinkingModuleBase
{
    [Header("Debug Options")]
    [Tooltip("Print raw JSON data")]
    public bool logRawJson = true;
    
    [Tooltip("Print parsed data structure")]
    public bool logParsedStructure = true;
    
    [Tooltip("Print detailed data breakdown")]
    public bool logDetailedBreakdown = true;
    
    [Tooltip("Maximum characters to display for raw JSON (0 = unlimited)")]
    public int maxDisplayLength = 5000;

    public override void OnDataLoaded()
    {
        if (downloader == null)
        {
            LogError("Downloader reference is null!");
            return;
        }

        LogSuccess("=== VRCLinking API DATA LOADED ===");

        // Log basic info
        LogInfo($"Server Name: {downloader.serverName}");
        LogInfo($"Server ID: {downloader.serverId}");
        LogInfo($"World Name: {downloader.worldName}");
        LogInfo($"World ID: {downloader.worldId}");

        // Log parsed data
        if (downloader.parsedData != null)
        {
            LogInfo($"Parsed data contains {downloader.parsedData.Count} top-level keys");
            
            if (logParsedStructure || logDetailedBreakdown)
            {
                LogParsedData();
            }
        }
        else
        {
            LogWarning("Parsed data is null!");
        }

        LogSuccess("=== DATA LOGGING COMPLETE ===");
    }

    private void LogParsedData()
    {
        LogInfo("--- API RESPONSE STRUCTURE ---");

        var parsedData = downloader.parsedData;
        var keys = parsedData.GetKeys();

        foreach (var key in keys.ToArray())
        {
            var value = parsedData[key];
            string keyStr = key.String;

            switch (value.TokenType)
            {
                case TokenType.DataDictionary:
                    LogInfo($"[{keyStr}] = Dictionary ({value.DataDictionary.Count} items)");
                    if (logDetailedBreakdown)
                    {
                        LogDictionary(value.DataDictionary, 1, keyStr);
                    }
                    break;

                case TokenType.DataList:
                    LogInfo($"[{keyStr}] = List ({value.DataList.Count} items)");
                    if (logDetailedBreakdown)
                    {
                        LogList(value.DataList, 1, keyStr);
                    }
                    break;

                case TokenType.String:
                    LogInfo($"[{keyStr}] = \"{value.String}\"");
                    break;

                case TokenType.Double:
                    LogInfo($"[{keyStr}] = {value.Double}");
                    break;

                case TokenType.Boolean:
                    LogInfo($"[{keyStr}] = {value.Boolean}");
                    break;

                case TokenType.Null:
                    LogInfo($"[{keyStr}] = null");
                    break;

                default:
                    LogInfo($"[{keyStr}] = {value.TokenType}");
                    break;
            }
        }
    }

    private void LogDictionary(DataDictionary dict, int depth, string parentKey)
    {
        if (depth > 3) return; // Prevent too deep recursion

        string indent = new string(' ', depth * 2);
        var keys = dict.GetKeys();

        foreach (var key in keys.ToArray())
        {
            var value = dict[key];
            string keyStr = key.String;

            switch (value.TokenType)
            {
                case TokenType.DataDictionary:
                    LogInfo($"{indent}[{parentKey}.{keyStr}] = Dictionary ({value.DataDictionary.Count} items)");
                    LogDictionary(value.DataDictionary, depth + 1, $"{parentKey}.{keyStr}");
                    break;

                case TokenType.DataList:
                    LogInfo($"{indent}[{parentKey}.{keyStr}] = List ({value.DataList.Count} items)");
                    LogList(value.DataList, depth + 1, $"{parentKey}.{keyStr}");
                    break;

                case TokenType.String:
                    LogInfo($"{indent}[{parentKey}.{keyStr}] = \"{value.String}\"");
                    break;

                case TokenType.Double:
                    LogInfo($"{indent}[{parentKey}.{keyStr}] = {value.Double}");
                    break;

                case TokenType.Boolean:
                    LogInfo($"{indent}[{parentKey}.{keyStr}] = {value.Boolean}");
                    break;

                case TokenType.Null:
                    LogInfo($"{indent}[{parentKey}.{keyStr}] = null");
                    break;

                default:
                    LogInfo($"{indent}[{parentKey}.{keyStr}] = {value.TokenType}");
                    break;
            }
        }
    }

    private void LogList(DataList list, int depth, string parentKey)
    {
        if (depth > 3) return; // Prevent too deep recursion

        string indent = new string(' ', depth * 2);
        int maxItemsToShow = logDetailedBreakdown ? 20 : 5;

        for (int i = 0; i < Math.Min(list.Count, maxItemsToShow); i++)
        {
            var value = list[i];

            switch (value.TokenType)
            {
                case TokenType.DataDictionary:
                    LogInfo($"{indent}[{parentKey}[{i}]] = Dictionary ({value.DataDictionary.Count} items)");
                    if (depth < 2)
                    {
                        LogDictionary(value.DataDictionary, depth + 1, $"{parentKey}[{i}]");
                    }
                    break;

                case TokenType.DataList:
                    LogInfo($"{indent}[{parentKey}[{i}]] = List ({value.DataList.Count} items)");
                    if (depth < 2)
                    {
                        LogList(value.DataList, depth + 1, $"{parentKey}[{i}]");
                    }
                    break;

                case TokenType.String:
                    LogInfo($"{indent}[{parentKey}[{i}]] = \"{value.String}\"");
                    break;

                case TokenType.Double:
                    LogInfo($"{indent}[{parentKey}[{i}]] = {value.Double}");
                    break;

                case TokenType.Boolean:
                    LogInfo($"{indent}[{parentKey}[{i}]] = {value.Boolean}");
                    break;

                case TokenType.Null:
                    LogInfo($"{indent}[{parentKey}[{i}]] = null");
                    break;

                default:
                    LogInfo($"{indent}[{parentKey}[{i}]] = {value.TokenType}");
                    break;
            }
        }

        if (list.Count > maxItemsToShow)
        {
            LogInfo($"{indent}... and {list.Count - maxItemsToShow} more items");
        }
    }

    // Logging helpers with color coding
    private void LogInfo(string message)
    {
        Debug.Log($"<color=cyan>[CheckLoadedData]</color> {message}");
    }

    private void LogSuccess(string message)
    {
        Debug.Log($"<color=green>[CheckLoadedData]</color> {message}");
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"<color=yellow>[CheckLoadedData]</color> {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"<color=red>[CheckLoadedData]</color> {message}");
    }
}
