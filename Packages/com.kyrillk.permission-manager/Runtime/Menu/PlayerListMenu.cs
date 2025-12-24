using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using TMPro;
using VRC.Udon.Common;

namespace PermissionSystem.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerListMenu : UdonSharpBehaviour
    {
        [Header("UI")]
        [SerializeField] private PermissionsManager manager;
        public Transform playerListParent;
        public GameObject playerNamePrefab;

        private GameObject entry;

        void  _Start()
        {
            UpdatePlayerList();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            UpdatePlayerList();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            UpdatePlayerList();
        }

        public void UpdatePlayerList()
        {
            if (playerListParent == null || playerNamePrefab == null)
            {
                Debug.LogWarning("PlayerListMenu: playerListParent or playerNamePrefab is not assigned.");
                return;
            }
            // Clear old UI entries
            for (int i = playerListParent.childCount - 1; i >= 0; i--)
            {
                Destroy(playerListParent.GetChild(i).gameObject);
            }

            // Get all players in instance
            var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);

            foreach (var player in players)
            {
                if (player == null) continue;

                entry = Instantiate(playerNamePrefab, playerListParent);
                SetupPlayerEntryDelayed();

                 PlayerEntry playerEntry = entry.GetComponentInChildren<PlayerEntry>();

                if (playerEntry != null)
                {   
                    if (manager == null)
                    {
                        Debug.LogWarning("PermissionsManager is not assigned in PlayerListMenu.");
                    }                    
                    playerEntry.SetManager(manager);

                    playerEntry.SetPlayer(player);
                }
            }
        }
        public void SetupPlayerEntryDelayed()
        {
           
        }
    }
}