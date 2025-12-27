using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRCLinking;
using VRC.SDK3.Data;

namespace PermissionSystem.Loader
{ 
    public class PermissionLoaderVrcLinking : PermissionLoader
    {
        
        public VrcLinkingDownloader downloader;

        private int retryCount = 0;
        private const int maxRetries = 3;
        private const float retryDelay = 5.0f;

        public override void RequestDataLoad()
        {
            if (downloader == null)
            {
                downloader = GetComponent<VrcLinkingDownloader>();
                if (downloader == null)
                {
                    LogError("No VrcLinkingDownloader found on the same GameObject!");
                    return;
                }
            }

            LoadData();
        }

        public void LoadData()
        {
            if (downloader == null)
            {
                LogError("Downloader is not set.");
                return;
            }

            DataDictionary data = downloader.parsedData;
            if (data == null)
            {
                if (retryCount < maxRetries)
                {
                    retryCount++;
                    Debug.Log($"Data not available yet. Retrying {retryCount}/{maxRetries} in {retryDelay} seconds...");
                    SendCustomEventDelayedSeconds(nameof(LoadData), retryDelay, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
                }
                else
                {
                    Debug.LogError("Failed to load data after multiple attempts.");
                }
                return;
            }
            LoadRolesFromData(data);
        }
    }
}