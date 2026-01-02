
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;
using Debug = UnityEngine.Debug;


namespace PermissionSystem.Loader
{
    public class JsonPermissionLoader : PermissionLoader
    {
        private VRCUrl url;

        public override void ChangeUrl(VRCUrl url)
        {
            if(url == null) return;
            this.url = url;
            Debug.Log("Url Set to " + url);
        }

        public override void RequestDataLoad()
        {
            VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.LogError("Failed To Load String");
        }

    }
}