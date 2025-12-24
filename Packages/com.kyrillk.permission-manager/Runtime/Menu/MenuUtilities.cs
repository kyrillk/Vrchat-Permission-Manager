using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace PermissionsManager.UI
{
    /// <summary>
    /// Utility methods for menu operations and UI manipulation
    /// VRChat Compatible - All utility methods work with Unity Canvas UI
    /// Coroutines should be started from a MonoBehaviour component
    /// </summary>
    public static class MenuUtilities
    {
        /// <summary>
        /// Smoothly fade a CanvasGroup
        /// </summary>
        public static IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
        {
            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }
            
            canvasGroup.alpha = targetAlpha;
        }
        
        /// <summary>
        /// Smoothly scale a transform
        /// </summary>
        public static IEnumerator ScaleTransform(Transform transform, Vector3 targetScale, float duration, AnimationCurve curve = null)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;
            
            if (curve == null)
            {
                curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = curve.Evaluate(elapsed / duration);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            transform.localScale = targetScale;
        }
        
        /// <summary>
        /// Smoothly move a RectTransform
        /// </summary>
        public static IEnumerator MoveRectTransform(RectTransform rectTransform, Vector2 targetPosition, float duration, AnimationCurve curve = null)
        {
            Vector2 startPosition = rectTransform.anchoredPosition;
            float elapsed = 0f;
            
            if (curve == null)
            {
                curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = curve.Evaluate(elapsed / duration);
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = targetPosition;
        }
        
        /// <summary>
        /// Create a simple button with text
        /// </summary>
        public static Button CreateButton(Transform parent, string text, Vector2 position, Vector2 size)
        {
            GameObject buttonObj = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(parent, false);
            
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
            
            Image image = buttonObj.GetComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            // Create text child
            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text textComponent = textObj.GetComponent<Text>();
            textComponent.text = text;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.color = Color.white;
            
            return buttonObj.GetComponent<Button>();
        }
        
        /// <summary>
        /// Set all buttons in an array to interactable or not
        /// </summary>
        public static void SetButtonsInteractable(Button[] buttons, bool interactable)
        {
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    button.interactable = interactable;
                }
            }
        }
        
        /// <summary>
        /// Set all buttons in a list to interactable or not
        /// </summary>
        public static void SetButtonsInteractable(List<Button> buttons, bool interactable)
        {
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    button.interactable = interactable;
                }
            }
        }
        
        /// <summary>
        /// Find all components of type T in children, including inactive
        /// </summary>
        public static List<T> FindComponentsInChildren<T>(Transform parent, bool includeInactive = true) where T : Component
        {
            List<T> components = new List<T>();
            T[] foundComponents = parent.GetComponentsInChildren<T>(includeInactive);
            components.AddRange(foundComponents);
            return components;
        }
        
        /// <summary>
        /// Set the active state of multiple GameObjects
        /// </summary>
        public static void SetActiveMultiple(bool active, params GameObject[] objects)
        {
            foreach (var obj in objects)
            {
                if (obj != null)
                {
                    obj.SetActive(active);
                }
            }
        }
        
        /// <summary>
        /// Calculate optimal grid layout for a number of items
        /// </summary>
        public static Vector2Int CalculateGridDimensions(int itemCount, float aspectRatio = 1f)
        {
            int columns = Mathf.CeilToInt(Mathf.Sqrt(itemCount * aspectRatio));
            int rows = Mathf.CeilToInt((float)itemCount / columns);
            return new Vector2Int(columns, rows);
        }
        
        /// <summary>
        /// Pulse effect for UI elements
        /// </summary>
        public static IEnumerator PulseEffect(Transform transform, float scaleMultiplier = 1.2f, float duration = 0.5f)
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * scaleMultiplier;
            
            float elapsed = 0f;
            while (elapsed < duration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration / 2);
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            elapsed = 0f;
            while (elapsed < duration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration / 2);
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
        
        /// <summary>
        /// Shake effect for UI elements
        /// </summary>
        public static IEnumerator ShakeEffect(Transform transform, float intensity = 10f, float duration = 0.5f)
        {
            Vector3 originalPosition = transform.localPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float strength = intensity * (1 - elapsed / duration);
                
                float x = Random.Range(-1f, 1f) * strength;
                float y = Random.Range(-1f, 1f) * strength;
                
                transform.localPosition = originalPosition + new Vector3(x, y, 0);
                yield return null;
            }
            
            transform.localPosition = originalPosition;
        }
    }
}
