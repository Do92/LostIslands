using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ClickableAlphaModifier : MonoBehaviour // Ment for buttons
{
    // Kinda dumb that this isn't easily customizable in the unity editor by default, as it should be.
    [Range(0, 1), Tooltip("Clicks on pixels with an alpha spectrum below this value will be filtered out")]
    public float AlphaThreshold = 0.5f;

    protected void Start()
    {
        GetComponent<Image>().eventAlphaThreshold = AlphaThreshold;
    }
}