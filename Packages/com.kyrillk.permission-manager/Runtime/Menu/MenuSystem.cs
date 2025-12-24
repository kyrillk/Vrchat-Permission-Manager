using UnityEngine;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

namespace PermissionsManager.UI
{
    /// <summary>
    /// Simple tab menu system for VRChat
    /// Attach to a Canvas GameObject - handles tab switching only
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MenuSystem : UdonSharpBehaviour
    {
        [Header("Tab Buttons")]
        [Tooltip("MenuTabButton components will auto-update when assigned here")]
        [SerializeField] private MenuTabButton[] menuTabButtons;
        
        private int currentTabIndex = -1;
        
        public int CurrentTabIndex => currentTabIndex;
        
        private void Start()
        {
            // Find MenuTabButton components if not assigned
            if (menuTabButtons == null || menuTabButtons.Length == 0)
            {
                menuTabButtons = GetComponentsInChildren<MenuTabButton>(true);
            }
            
            // Select first tab by default
            if (menuTabButtons != null && menuTabButtons.Length > 0)
            {
                SelectTab(0);
            }
        }
        

        #region Tab Management
        
        public void SelectTab(int index)
        {
            if (index < 0 || menuTabButtons == null || index >= menuTabButtons.Length)
                return;
            
            currentTabIndex = index;
            UpdateTabButtons();
        }
        
        public void SelectTabByName(string tabName)
        {
            if (menuTabButtons == null || string.IsNullOrEmpty(tabName))
                return;
            
            // Find the button with matching tab name and get its index
            for (int i = 0; i < menuTabButtons.Length; i++)
            {
                if (menuTabButtons[i] != null)
                {
                    // Try to switch - the button will check if it matches
                    string buttonTabName = menuTabButtons[i].GetTabName();
                    if (buttonTabName == tabName)
                    {
                        SelectTab(menuTabButtons[i].GetTabIndex());
                        return;
                    }
                }
            }
        }
        
        public void NextTab()
        {
            if (menuTabButtons == null || menuTabButtons.Length == 0)
                return;
            
            int nextIndex = (currentTabIndex + 1) % menuTabButtons.Length;
            SelectTab(nextIndex);
        }
        
        public void PreviousTab()
        {
            if (menuTabButtons == null || menuTabButtons.Length == 0)
                return;
            
            int prevIndex = currentTabIndex - 1;
            if (prevIndex < 0)
                prevIndex = menuTabButtons.Length - 1;
            
            SelectTab(prevIndex);
        }
        
        public string GetCurrentTabName()
        {
            if (menuTabButtons != null && currentTabIndex >= 0 && currentTabIndex < menuTabButtons.Length)
            {
                if (menuTabButtons[currentTabIndex] != null)
                {
                    return menuTabButtons[currentTabIndex].GetTabName();
                }
            }
            return null;
        }
        
        public GameObject GetCurrentTabContent()
        {
            if (menuTabButtons != null && currentTabIndex >= 0 && currentTabIndex < menuTabButtons.Length)
            {
                if (menuTabButtons[currentTabIndex] != null)
                {
                    return menuTabButtons[currentTabIndex].GetTabContent();
                }
            }
            return null;
        }
        
        /// <summary>
        /// Update all MenuTabButton visual states
        /// </summary>
        public void UpdateTabButtons()
        {
            if (menuTabButtons != null)
            {
                for (int i = 0; i < menuTabButtons.Length; i++)
                {
                    if (menuTabButtons[i] != null)
                    {
                        menuTabButtons[i].UpdateVisualState();
                    }
                }
            }
        }
        
        /// <summary>
        /// Register a MenuTabButton to be updated when tabs change
        /// </summary>
        public void RegisterTabButton(MenuTabButton tabButton)
        {
            if (tabButton == null)
                return;
            
            // Check if already registered
            if (menuTabButtons != null)
            {
                for (int i = 0; i < menuTabButtons.Length; i++)
                {
                    if (menuTabButtons[i] == tabButton)
                        return;
                }
            }
            
            // Add to array
            if (menuTabButtons == null)
            {
                menuTabButtons = new MenuTabButton[1];
                menuTabButtons[0] = tabButton;
            }
            else
            {
                MenuTabButton[] newArray = new MenuTabButton[menuTabButtons.Length + 1];
                for (int i = 0; i < menuTabButtons.Length; i++)
                {
                    newArray[i] = menuTabButtons[i];
                }
                newArray[menuTabButtons.Length] = tabButton;
                menuTabButtons = newArray;
            }
        }
        
        /// <summary>
        /// Get the index of a specific tab button in the array
        /// Returns -1 if not found
        /// </summary>
        public int GetTabButtonIndex(MenuTabButton tabButton)
        {
            if (menuTabButtons == null || tabButton == null)
                return -1;
            
            for (int i = 0; i < menuTabButtons.Length; i++)
            {
                if (menuTabButtons[i] == tabButton)
                    return i;
            }
            
            return -1;
        }
        
        #endregion
        
        #region Public Methods for Unity Editor OnClick Events
        
        public void _NextTab()
        {
            NextTab();
        }
        
        public void _PreviousTab()
        {
            PreviousTab();
        }
        
        #endregion
    }
}
