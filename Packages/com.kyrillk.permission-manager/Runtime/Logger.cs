
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace PermissionSystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Logger : UdonSharpBehaviour
    {
        void Start()
        {
            
        }

        public void Log(string message)
        {
            Debug.Log("[Logger] " + message);
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning("[Logger] " + message);
        }

        public void LogError(string message)
        {
            Debug.LogError("[Logger] " + message);
        }

        public void test(string message)
        {
            Debug.LogError("[Logger] Exception: " + message);
        }
    }
}