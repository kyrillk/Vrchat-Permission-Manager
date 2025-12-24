using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

namespace PermissionsManager.UI
{
    /// <summary>
    /// Helper component for managing scrollable menu content with dynamic item management
    /// VRChat Compatible - Uses Unity ScrollRect for VR-friendly scrolling
    /// Supports both VR controller and desktop mouse scrolling
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MenuScrollView : UdonSharpBehaviour
    {
        [Header("References")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private GameObject itemPrefab;
        
        [Header("Layout Settings")]
        [SerializeField] private float spacing = 10f;
        [SerializeField] private float itemHeight = 50f;
        [SerializeField] private bool usePooling = true;
        [SerializeField] private int poolSize = 20;
        
        [Header("Scrolling")]
        [SerializeField] private bool smoothScrolling = true;
        [SerializeField] private float scrollSpeed = 5f;
        
        private GameObject[] itemPool;
        private int poolIndex = 0;
        private GameObject[] activeItems;
        private int activeCount = 0;
        private float targetScrollPosition = 0f;
        
        private void Start()
        {
            if (scrollRect == null)
            {
                scrollRect = GetComponent<ScrollRect>();
            }
            
            if (content == null && scrollRect != null)
            {
                content = scrollRect.content;
            }
            
            // Initialize arrays
            itemPool = new GameObject[poolSize];
            activeItems = new GameObject[poolSize * 2];
            activeCount = 0;
            poolIndex = 0;
            
            // Initialize pool
            if (usePooling && itemPrefab != null)
            {
                InitializePool();
            }
        }
        
        private void Update()
        {
            if (smoothScrolling && scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(
                    scrollRect.verticalNormalizedPosition,
                    targetScrollPosition,
                    Time.deltaTime * scrollSpeed
                );
            }
        }
        
        private void InitializePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject item = Instantiate(itemPrefab, content);
                item.SetActive(false);
                itemPool[i] = item;
            }
            poolIndex = poolSize;
        }
        
        /// <summary>
        /// Add an item to the scroll view
        /// </summary>
        public GameObject AddItem()
        {
            GameObject item;
            
            if (usePooling && poolIndex > 0)
            {
                poolIndex--;
                item = itemPool[poolIndex];
                item.SetActive(true);
            }
            else
            {
                item = Instantiate(itemPrefab, content);
            }
            
            if (activeCount < activeItems.Length)
            {
                activeItems[activeCount] = item;
                activeCount++;
            }
            
            UpdateLayout();
            
            return item;
        }
        
        /// <summary>
        /// Remove an item from the scroll view
        /// </summary>
        public void RemoveItem(GameObject item)
        {
            int index = -1;
            for (int i = 0; i < activeCount; i++)
            {
                if (activeItems[i] == item)
                {
                    index = i;
                    break;
                }
            }
            
            if (index >= 0)
            {
                // Shift items down
                for (int i = index; i < activeCount - 1; i++)
                {
                    activeItems[i] = activeItems[i + 1];
                }
                activeCount--;
                
                if (usePooling && poolIndex < itemPool.Length)
                {
                    item.SetActive(false);
                    itemPool[poolIndex] = item;
                    poolIndex++;
                }
                else
                {
                    Destroy(item);
                }
                
                UpdateLayout();
            }
        }
        
        /// <summary>
        /// Clear all items
        /// </summary>
        public void ClearItems()
        {
            for (int i = 0; i < activeCount; i++)
            {
                if (activeItems[i] != null)
                {
                    if (usePooling && poolIndex < itemPool.Length)
                    {
                        activeItems[i].SetActive(false);
                        itemPool[poolIndex] = activeItems[i];
                        poolIndex++;
                    }
                    else
                    {
                        Destroy(activeItems[i]);
                    }
                }
            }
            
            activeCount = 0;
            UpdateLayout();
        }
        
        /// <summary>
        /// Update the layout of items
        /// </summary>
        private void UpdateLayout()
        {
            float yPosition = 0f;
            
            for (int i = 0; i < activeCount; i++)
            {
                if (activeItems[i] != null)
                {
                    RectTransform rectTransform = activeItems[i].GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(0, -yPosition);
                    yPosition += itemHeight + spacing;
                }
            }
            
            // Update content size
            if (content != null)
            {
                content.sizeDelta = new Vector2(content.sizeDelta.x, yPosition);
            }
        }
        
        /// <summary>
        /// Scroll to a specific item
        /// </summary>
        public void ScrollToItem(GameObject item, bool immediate = false)
        {
            int index = -1;
            for (int i = 0; i < activeCount; i++)
            {
                if (activeItems[i] == item)
                {
                    index = i;
                    break;
                }
            }
            
            if (index >= 0)
            {
                ScrollToIndex(index, immediate);
            }
        }
        
        /// <summary>
        /// Scroll to an item by index
        /// </summary>
        public void ScrollToIndex(int index, bool immediate = false)
        {
            if (index < 0 || index >= activeCount || scrollRect == null)
                return;
            
            float totalHeight = content.rect.height;
            float viewportHeight = scrollRect.viewport.rect.height;
            
            if (totalHeight <= viewportHeight)
                return;
            
            float itemPosition = index * (itemHeight + spacing);
            float normalizedPosition = 1f - (itemPosition / (totalHeight - viewportHeight));
            normalizedPosition = Mathf.Clamp01(normalizedPosition);
            
            if (immediate || !smoothScrolling)
            {
                scrollRect.verticalNormalizedPosition = normalizedPosition;
            }
            else
            {
                targetScrollPosition = normalizedPosition;
            }
        }
        
        /// <summary>
        /// Scroll to top
        /// </summary>
        public void ScrollToTop(bool immediate = false)
        {
            if (immediate || !smoothScrolling)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
            else
            {
                targetScrollPosition = 1f;
            }
        }
        
        /// <summary>
        /// Scroll to bottom
        /// </summary>
        public void ScrollToBottom(bool immediate = false)
        {
            if (immediate || !smoothScrolling)
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }
            else
            {
                targetScrollPosition = 0f;
            }
        }
        
        /// <summary>
        /// Get all active items as array
        /// </summary>
        public GameObject[] GetActiveItems()
        {
            GameObject[] result = new GameObject[activeCount];
            for (int i = 0; i < activeCount; i++)
            {
                result[i] = activeItems[i];
            }
            return result;
        }
        
        /// <summary>
        /// Get item count
        /// </summary>
        public int GetItemCount()
        {
            return activeCount;
        }
        
        /// <summary>
        /// Get item at specific index
        /// </summary>
        public GameObject GetItemAt(int index)
        {
            if (index >= 0 && index < activeCount)
            {
                return activeItems[index];
            }
            return null;
        }
    }
}
