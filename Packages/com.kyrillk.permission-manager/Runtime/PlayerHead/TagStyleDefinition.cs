using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PermissionSystem
{
    /// <summary>
    /// Position of the icon relative to the player name text
    /// </summary>
    public enum IconPosition
    {
        Left,
        Right,
        Above,
        Below
    }

    /// <summary>
    /// Defines the visual style of a player tag.
    /// Can be used both as a data container for tag appearance
    /// and as a menu item for tag style selection.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TagStyleDefinition : UdonSharpBehaviour
    {
        [Header("Style Identity")]
        [Tooltip("Name of this tag style")]
        public string styleName = "Default";
        
        [Tooltip("Description of this style")]
        [TextArea(2, 4)]
        public string styleDescription;

        [Header("Icon Settings")]
        [Tooltip("The icon/image to display on the tag")]
        public Sprite tagIcon;
        
        [Tooltip("Icon color tint")]
        public Color iconColor = Color.white;
        
        [Tooltip("Icon size multiplier")]
        [Range(0.1f, 3f)]
        public float iconScale = 1f;

        [Header("Background Settings")]
        [Tooltip("Background sprite for the tag")]
        public Sprite backgroundSprite;
        
        [Tooltip("Background color")]
        public Color backgroundColor = new Color(0, 0, 0, 0.7f);
        
        [Tooltip("Background size")]
        public Vector2 backgroundSize = new Vector2(200, 50);

        [Header("Text Settings")]
        [Tooltip("Font size for player name")]
        public float nameFontSize = 24f;
        
        [Tooltip("Player name text color")]
        public Color nameColor = Color.white;
        
        [Tooltip("Text outline color")]
        public Color outlineColor = Color.black;
        
        [Tooltip("Text outline thickness")]
        [Range(0f, 1f)]
        public float outlineThickness = 0.2f;

        [Header("Layout Settings")]
        [Tooltip("Padding around the tag content")]
        public Vector4 padding = new Vector4(10, 10, 10, 10); // left, right, top, bottom
        
        [Tooltip("Spacing between icon and text")]
        public float iconTextSpacing = 10f;
        
        [Tooltip("Icon position relative to text")]
        public IconPosition iconPosition = IconPosition.Left;

        [Header("Animation Settings")]
        [Tooltip("Enable bobbing animation")]
        public bool enableBobbing;
        
        [Tooltip("Bobbing speed")]
        public float bobbingSpeed = 1f;
        
        [Tooltip("Bobbing amount")]
        public float bobbingAmount = 0.1f;

        [Header("Visibility Settings")]
        [Tooltip("Maximum distance at which the tag is visible")]
        public float maxVisibleDistance = 20f;
        
        [Tooltip("Distance at which the tag starts fading")]
        public float fadeStartDistance = 15f;
        
        [Tooltip("Enable distance-based scaling")]
        public bool scaleWithDistance = true;


        /// <summary>
        /// Applies this style to a tag GameObject
        /// </summary>
        public void ApplyStyleToTag(GameObject tagObject)
        {
            if (tagObject == null) return;

            // Apply background
            Image background = tagObject.GetComponent<Image>();
            if (background != null)
            {
                if (backgroundSprite != null)
                    background.sprite = backgroundSprite;
                background.color = backgroundColor;
                
                RectTransform rt = background.GetComponent<RectTransform>();
                if (rt != null)
                    rt.sizeDelta = backgroundSize;
            }

            // Apply icon
            Transform iconTransform = tagObject.transform.Find("Icon");
            if (iconTransform != null)
            {
                Image icon = iconTransform.GetComponent<Image>();
                if (icon != null)
                {
                    if (tagIcon != null)
                        icon.sprite = tagIcon;
                    icon.color = iconColor;
                    icon.transform.localScale = Vector3.one * iconScale;
                }
            }
        }

        /// <summary>
        /// Sets the player name on a tag object
        /// </summary>
        public void SetPlayerName(GameObject tagObject, string playerName)
        {
            if (tagObject == null) return;

            Transform textTransform = tagObject.transform.Find("PlayerName");
            if (textTransform != null)
            {
                TextMeshProUGUI text = textTransform.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = playerName;
                }
            }
        }

        /// <summary>
        /// Sets the icon on a tag object
        /// </summary>
        public void SetIcon(GameObject tagObject, Sprite icon)
        {
            if (tagObject == null) return;

            Transform iconTransform = tagObject.transform.Find("Icon");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = icon;
                    iconImage.enabled = icon != null;
                }
            }
        }
    }
}

