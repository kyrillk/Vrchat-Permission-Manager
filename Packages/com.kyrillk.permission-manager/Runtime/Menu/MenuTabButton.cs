using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

namespace PermissionsManager.UI
{
    /// <summary>
    /// Tab button component that switches between tabs in a MenuSystem
    /// VRChat Compatible - Attach to any button to make it control tabs
    /// Much simpler than configuring onClick events in the editor
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(Button))]
    public class MenuTabButton : UdonSharpBehaviour
    {
        [Header("References")]
        [SerializeField] private MenuSystem menuSystem;
        [SerializeField] private Button button;
        
        [Header("Tab Settings")]
        [Tooltip("The content GameObject to show/hide for this tab")]
        [SerializeField] private GameObject tabContent;
        
        [Tooltip("Optional: Use tab name instead of index")]
        [SerializeField] private string tabName = "";
        
        [Tooltip("Use tab name instead of index")]
        [SerializeField] private bool useTabName = false;
        
        // Tab index is automatically determined from position in MenuSystem array
        private int tabIndex = -1;
        
        [Header("Visual Feedback")]
        [Tooltip("GameObject to show when this tab is selected")]
        [SerializeField] private GameObject selectedIndicator;
        
        [Tooltip("Update button interactable state when selected")]
        [SerializeField] private bool disableWhenSelected = true;
        
        [Header("Audio")]
        [SerializeField] private bool playAudio = false;
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioSource audioSource;
        
        private void Start()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
            
            // Find MenuSystem if not assigned
            if (menuSystem == null)
            {
                menuSystem = GetComponentInParent<MenuSystem>();
                if (menuSystem == null)
                {
                    Debug.LogWarning("[MenuTabButton] MenuSystem not found on " + gameObject.name + ". Please assign MenuSystem in Inspector.");
                }
            }
            
            // Audio source should be added in editor
            if (playAudio && audioSource == null)
            {
                Debug.LogWarning("[MenuTabButton] AudioSource not assigned on " + gameObject.name + ". Please add AudioSource component in editor.");
            }
            
            // Auto-detect tab index from MenuSystem array
            if (menuSystem != null && !useTabName)
            {
                tabIndex = menuSystem.GetTabButtonIndex(this);
                if (tabIndex < 0)
                {
                    Debug.LogWarning("[MenuTabButton] Could not find this button in MenuSystem array on " + gameObject.name);
                }
            }
            
            // Hide selected indicator by default
            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(false);
            }
        }
        
        /// <summary>
        /// Called by Unity Button onClick event or VRC Interact
        /// </summary>
        public override void Interact()
        {
            if (menuSystem == null)
            {
                Debug.LogWarning("[MenuTabButton] Cannot switch tab - MenuSystem is null!");
                return;
            }
            
            // Switch to the specified tab
            if (useTabName && !string.IsNullOrEmpty(tabName))
            {
                menuSystem.SelectTabByName(tabName);
            }
            else
            {
                menuSystem.SelectTab(tabIndex);
            }
            
            // Play click sound
            if (playAudio && clickSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(clickSound);
            }
            
            // Update visual state
            UpdateVisualState();
        }
        
        /// <summary>
        /// Update the visual state of this button based on current tab
        /// Call this from MenuSystem when tab changes or manually
        /// </summary>
        public void UpdateVisualState()
        {
            if (menuSystem == null)
                return;
            
            bool isCurrentTab = false;
            
            if (useTabName && !string.IsNullOrEmpty(tabName))
            {
                isCurrentTab = menuSystem.GetCurrentTabName() == tabName;
            }
            else
            {
                isCurrentTab = menuSystem.CurrentTabIndex == tabIndex;
            }
            
            // Update selected indicator
            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(isCurrentTab);
            }
            
            // Update button interactable state
            if (disableWhenSelected && button != null)
            {
                button.interactable = !isCurrentTab;
            }
            
            // Show/hide tab content
            if (tabContent != null)
            {
                tabContent.SetActive(isCurrentTab);
            }
        }
        
        /// <summary>
        /// Set the tab index dynamically
        /// </summary>
        public void SetTabIndex(int index)
        {
            tabIndex = index;
            useTabName = false;
            UpdateVisualState();
        }
        
        /// <summary>
        /// Set the tab name dynamically
        /// </summary>
        public void SetTabName(string name)
        {
            tabName = name;
            useTabName = true;
            UpdateVisualState();
        }
        
        /// <summary>
        /// Set the menu system reference
        /// </summary>
        public void SetMenuSystem(MenuSystem menu)
        {
            menuSystem = menu;
        }
        
        /// <summary>
        /// Get the tab index
        /// </summary>
        public int GetTabIndex()
        {
            return tabIndex;
        }
        
        /// <summary>
        /// Get the tab name
        /// </summary>
        public string GetTabName()
        {
            return tabName;
        }
        
        /// <summary>
        /// Get the tab content GameObject
        /// </summary>
        public GameObject GetTabContent()
        {
            return tabContent;
        }
        
        /// <summary>
        /// Set the tab content GameObject
        /// </summary>
        public void SetTabContent(GameObject content)
        {
            tabContent = content;
        }
    }
}
