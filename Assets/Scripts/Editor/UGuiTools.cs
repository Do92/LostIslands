using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class UGuiTools : MonoBehaviour
    {

        // Anchor to the sides of the image
        [MenuItem("Custom tools/UGui: Anchors to Corners %[")]
        private static void AnchorsToCorners()
        {
            RectTransform currentTransform = Selection.activeTransform as RectTransform;
            RectTransform parentTransform = Selection.activeTransform.parent as RectTransform;

            if (currentTransform == null || parentTransform == null) return;

            Vector2 newAnchorsMin = new Vector2(currentTransform.anchorMin.x + currentTransform.offsetMin.x / parentTransform.rect.width,
                currentTransform.anchorMin.y + currentTransform.offsetMin.y / parentTransform.rect.height);
            Vector2 newAnchorsMax = new Vector2(currentTransform.anchorMax.x + currentTransform.offsetMax.x / parentTransform.rect.width,
                currentTransform.anchorMax.y + currentTransform.offsetMax.y / parentTransform.rect.height);

            currentTransform.anchorMin = newAnchorsMin;
            currentTransform.anchorMax = newAnchorsMax;
            currentTransform.offsetMin = currentTransform.offsetMax = new Vector2(0, 0);
        }

        [MenuItem("Custom tools/UGui: Corners to Anchors %]")]
        private static void CornersToAnchors()
        {
            RectTransform currentTransform = Selection.activeTransform as RectTransform;

            if (currentTransform == null) return;

            currentTransform.offsetMin = currentTransform.offsetMax = new Vector2(0, 0);
        }
    }
}