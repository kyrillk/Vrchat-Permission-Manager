using UdonSharp;
using UnityEngine;
using VRC.Economy;
using VRC.SDK3.Data;
using VRC.SDKBase;
using PermissionSystem.Core;

namespace PermissionSystem
{
    /// <summary>
    /// Displays floating indicators above players' heads based on permission membership.
    /// Extends PermissionContainerBase to use permission checking for showing indicators.
    /// Only players who are members of the required permissions will have an indicator displayed.
    /// </summary>

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FloatingOverheadBuyIndicator : PermissionAwareBehaviour
    {
        [Tooltip("The tag to assign to players when they join.")]
        public string playerTag = "Player";

        [Tooltip("The indicator GameObject prefab to float above players' heads. Avoid adding colliders to prevent interaction issues.")]
        public GameObject overheadIndicatorPrefab;

        [Tooltip("How far above the players head you want the Indicator to float.")]
        public float heightAboveHead = 1f;

        [Tooltip("Whether or not you should see an indicator above yourself if you own the product.")]
        public bool showIndicatorAboveLocalPlayer = true;
        
        [Tooltip("Whether or not you should see an indicator above OtherPlayers.")]
        public bool showIndicatorAboveAllPlayers = true;

        [Tooltip("Max amount of indicators to update per frame." +
                 "This helps performance but can make the indicator look choppy if you set it too low if there are a lot of players in the instance who own the product.")]
        [Range(1, 100)]
        public int maxUpdatesPerFrame = 10;
        private int _nextIndexToUpdate;
        private int _updatesThisFrame;
        protected override string LogPrefix => "FloatingOverheadBuyIndicator";

        // We'll use this to keep track of which players have an indicator that should be shown above their head. We store the player id as the key and the indicator as the value.
        private DataDictionary _playerIndicatorDataDictionary;

        private void Start()
        {
            _playerIndicatorDataDictionary = new DataDictionary();
        }

        protected override void OnManagedStart()
        {
            if (requiredPermissions == null || requiredPermissions.Length == 0)
            {
                Debug.LogWarning("FloatingOverheadBuyIndicator: No required membership set. The indicator will be shown for all players.");
                return;
            }
            else
            {
                foreach (PermissionContainerBase membership in requiredPermissions)
                {
                    membership.AddUpdateListener(this);
                }
            }

            UpdatePlayerTag();
        }

        public override void OnPermissionsUpdated()
        {
            UpdatePlayerTag();
        }

        // When a player joins, add an indicator above them if they have required permissions
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return;
            
            if (!showIndicatorAboveLocalPlayer && player.isLocal || !showIndicatorAboveAllPlayers) return;

            // Check if player has any of the required permissions
            if (!HasPermission(player))
            {
                return;
            }

            // Add the player to the dictionary with an indicator when they join
            if (!_playerIndicatorDataDictionary.ContainsKey(player.playerId))
            {
                _playerIndicatorDataDictionary.Add(player.playerId, Instantiate(overheadIndicatorPrefab));
            }
        }

        public void UpdatePlayerTag()
        {
            // Check all players to see if they should have indicators (in case permissions change during the session)
            var playersCount = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi[] players = VRCPlayerApi.GetPlayers(playersCount);

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            
            foreach (VRCPlayerApi player in players)
            {
                if (!Utilities.IsValid(player)) continue;

                // Skip local player if disabled
                if (localPlayer == player && !showIndicatorAboveLocalPlayer)
                {
                    RemovePlayerFromDictionaryIfExists(player);
                    continue;
                }

                // Skip other players if disabled
                if (!showIndicatorAboveAllPlayers)
                {
                    RemovePlayerFromDictionaryIfExists(player);
                    continue;
                }

                // Check if player has required permission
                if (HasPermission(player))
                {
                    GameObject indicatorObject = null;
                    // Add indicator if they don't have one
                    if (!_playerIndicatorDataDictionary.ContainsKey(player.playerId))
                    {
                        GameObject indicator = Instantiate(overheadIndicatorPrefab);
                        indicator.SetActive(showIndicatorAboveAllPlayers);
                        _playerIndicatorDataDictionary.Add(player.playerId, indicator);
                    }else
                    {
                        // Get existing indicator
                        DataToken value = _playerIndicatorDataDictionary[player.playerId];
                        if (value.TokenType == TokenType.Reference)
                        {
                            indicatorObject = (GameObject)value.Reference;
                        }
                    }
                    // set setting indicatorObject?
                    //indicatorObject.getComponent<Renderer>().enabled = true;
                    
                }
                else
                {
                    // Remove indicator if they no longer have permission
                    RemovePlayerFromDictionaryIfExists(player);
                }
            }
        }

        // Running this in PostLateUpdate to make sure the player's position, IK pose and Animator is updated before we try to set the indicator's position.
        public override void PostLateUpdate()
        {
            if (!showIndicatorAboveAllPlayers) return;
            DataList keys = _playerIndicatorDataDictionary.GetKeys();
            // Each key in the dictionary is a player id, so we want to loop through each key and get the value (Indicator object).

            if (keys.Count == 0) return;

            // If there are more keys than the max updates per frame, we want to limit the amount of updates we do this frame. Otherwise, we'll update all of them.
            _updatesThisFrame = Mathf.Min(maxUpdatesPerFrame, keys.Count);

            // For performance reasons, we want to spread out the updates over multiple frames. We'll use _updatesThisFrame to keep track of how many updates we want to do this frame.
            for (int i = 0; i < _updatesThisFrame; i++)
            {
                // If we've reached the end of the list, we want to start over from the beginning.
                if (_nextIndexToUpdate >= keys.Count)
                {
                    _nextIndexToUpdate = 0;
                }

                DataToken key = keys[_nextIndexToUpdate];

                if (_playerIndicatorDataDictionary.TryGetValue(key, out DataToken value))
                {
                    VRCPlayerApi player = VRCPlayerApi.GetPlayerById(key.Int);
                    // We want to make sure the player is valid and the token is a reference before we try to use it to avoid possible errors.
                    if (Utilities.IsValid(player) && value.TokenType == TokenType.Reference)
                    {
                        GameObject indicator = (GameObject)value.Reference;
                        // We also want to make sure the indicator is valid before we try to set it's position.
                        if (indicator != null)
                        {
                            indicator.transform.position = player.GetBonePosition(HumanBodyBones.Head) +
                                                           Vector3.up * heightAboveHead;
                        }
                    }
                }

                _nextIndexToUpdate++;
            }
        }

        // If the player leaves the instance, we want to remove them from the dictionary if they exist.
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            RemovePlayerFromDictionaryIfExists(player);
        }

        // We want to make sure we destroy the indicator before removing the player from the dictionary to not leave any lingering objects.
        private void RemovePlayerFromDictionaryIfExists(VRCPlayerApi player)
        {
            if (_playerIndicatorDataDictionary.TryGetValue(player.playerId, out DataToken value))
            {
                if (value.TokenType == TokenType.Reference)
                {
                    GameObject indicatorObject = (GameObject)value.Reference;

                    Destroy(indicatorObject);
                }

                _playerIndicatorDataDictionary.Remove(player.playerId);
            }
        }


        public void setshowIndicatorAboveAllPlayers(bool value)
        {
            showIndicatorAboveAllPlayers = value;
        }

        public void setshowIndicatorAboveLocalPlayer(bool value)
        {
            showIndicatorAboveLocalPlayer = value;
        }
    }
}