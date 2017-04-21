using UnityEngine;

namespace Menu
{
    [RequireComponent(typeof(RectTransform))]
    public class MenuPanel : MonoBehaviour
    {
        public bool ContainsNestedPanels = false;
    }
}