// This file has been modified to work with the new GUI system, with a working sort order to split graphical elements.
//
// Original source below:
// http://wiki.unity3d.com/index.php/FadeInOut

// Instructions:
// You do not need to add this to any objects, just keep it in the project.
// This class is a Singleton, which means there's only one and you can call it from anywhere even without a reference to it.
// Look in the CameraFadeOnStart script for an example of how to start a fade. It's mega easy.

using UnityEngine;
using System;
using UnityEngine.UI;

namespace Miscellaneous
{
    public class CameraFade : MonoBehaviour
    {
        private static CameraFade actualInstance;

        private static CameraFade instance
        {
            get
            {
                if (!actualInstance)
                    actualInstance = FindObjectOfType(typeof(CameraFade)) as CameraFade ??
                                     new GameObject("CameraFade").AddComponent<CameraFade>();

                return actualInstance;
            }
        }

        // This will contain the 1x1 pixel texture used for fading
        public RawImage FadeImage;

        // Default starting color: black and fully transparent
        public Color CurrentScreenOverlayColor = new Color(0, 0, 0, 255);

        // Default target color: black and fully transparent
        public Color TargetScreenOverlayColor = new Color(0, 0, 0, 0);

        // The delta-color is basically the "speed / second" at which the current color should change
        public Color DeltaColor = new Color(0, 0, 0, 0);

        // Make sure this texture is drawn on top of everything (replaced by the now usable sorting order since this didn't work)
        public int FadeGuiDepth = -1000;

        public float FadeDelay;
        public Action OnFadeFinish;

        private void Awake()
        {
            if (actualInstance == null)
            {
                actualInstance = this;
                instance.Initialize();
            }
        }

        // Initialize the texture, background-style and initial color:
        public void Initialize()
        {
            // Create the fade texture
            instance.FadeImage = gameObject.AddComponent<RawImage>();
            instance.FadeImage.texture = new Texture2D(1, 1);

            // Create a parent canvas game object with the correct display settings
            GameObject canvasGameObject = new GameObject("Canvas - Camera Fade");
            Canvas actualCanvas = canvasGameObject.AddComponent<Canvas>();
            actualCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            actualCanvas.pixelPerfect = true;
            actualCanvas.sortingOrder = -1;
            CanvasScaler canvasScaler = canvasGameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasGameObject.AddComponent<GraphicRaycaster>();
            transform.SetParent(canvasGameObject.transform, false);

            // Reset any incorrect transform settings
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
        }

        // Draw the texture and perform the fade:
        private void OnGUI()
        {
            // If delay is over...
            if (Time.time > instance.FadeDelay)
            {
                // If the current color of the screen is not equal to the desired color: keep fading!
                if (instance.CurrentScreenOverlayColor != instance.TargetScreenOverlayColor)
                {
                    // If the difference between the current alpha and the desired alpha is smaller than delta-alpha * deltaTime, then we're pretty much done fading:
                    if (Mathf.Abs(instance.CurrentScreenOverlayColor.a - instance.TargetScreenOverlayColor.a) < Mathf.Abs(instance.DeltaColor.a) * Time.deltaTime)
                    {
                        instance.CurrentScreenOverlayColor = instance.TargetScreenOverlayColor;
                        SetScreenOverlayColor(instance.CurrentScreenOverlayColor);
                        instance.DeltaColor = new Color(0, 0, 0, 0);

                        if (instance.OnFadeFinish != null)
                            instance.OnFadeFinish();

                        //Die(); // This object will get used continuously, so lets not destroy it
                    }
                    else
                    {
                        // Fade!
                        SetScreenOverlayColor(instance.CurrentScreenOverlayColor + instance.DeltaColor * Time.deltaTime);
                    }
                }
            }
            // Only draw the texture when the alpha value is greater than 0:
            if (CurrentScreenOverlayColor.a > 0)
            {
                GUI.depth = instance.FadeGuiDepth;
                GUI.Label(new Rect(-10, -10, Screen.width + 10, Screen.height + 10), instance.FadeImage.texture);
            }
        }

        /// <summary>
        /// Sets the color of the screen overlay instantly. Useful to start a fade.
        /// </summary>
        /// <param name='newScreenOverlayColor'>
        /// New screen overlay color.
        /// </param>
        private static void SetScreenOverlayColor(Color newScreenOverlayColor)
        {
            instance.CurrentScreenOverlayColor = newScreenOverlayColor;
            Texture2D fadeTexture = instance.FadeImage.texture as Texture2D;
            if (fadeTexture)
            {
                fadeTexture.SetPixel(0, 0, instance.CurrentScreenOverlayColor);
                fadeTexture.Apply();
            }
        }

        /// <summary>
        /// Starts the fade from color newScreenOverlayColor.
        /// </summary>
        /// <param name='newScreenOverlayColor'>
        /// Target screen overlay Color.
        /// </param>
        /// <param name='willFadeOut'>
        /// Will fade out (from opaque to transparent) if true and false simply does the opposite.
        /// </param>
        /// <param name='fadeDuration'>
        /// Fade duration.
        /// </param>
        public static void StartAlphaFade(Color newScreenOverlayColor, bool willFadeOut, float fadeDuration)
        {
            if (fadeDuration <= 0.0f)
                SetScreenOverlayColor(newScreenOverlayColor);
            else
            {
                if (willFadeOut)
                {
                    instance.TargetScreenOverlayColor = new Color(newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0);
                    SetScreenOverlayColor(newScreenOverlayColor);
                }
                else
                {
                    instance.TargetScreenOverlayColor = newScreenOverlayColor;
                    SetScreenOverlayColor(new Color(newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0));
                }

                instance.DeltaColor = (instance.TargetScreenOverlayColor - instance.CurrentScreenOverlayColor) / fadeDuration;
            }
        }

        /// <summary>
        /// Starts the fade from color newScreenOverlayColor, after a delay.
        /// </summary>
        /// <param name='newScreenOverlayColor'>
        /// New screen overlay color.
        /// </param>
        /// <param name='willFadeOut'>
        /// Will fade out (from opaque to transparent) if true and false simply does the opposite.
        /// </param>
        /// <param name='fadeDuration'>
        /// Fade duration.
        /// </param>
        /// <param name='fadeDelay'>
        /// Fade delay.
        /// </param>
        public static void StartAlphaFade(Color newScreenOverlayColor, bool willFadeOut, float fadeDuration, float fadeDelay)
        {
            if (fadeDuration <= 0.0f)
                SetScreenOverlayColor(newScreenOverlayColor);
            else
            {
                instance.FadeDelay = Time.time + fadeDelay;

                if (willFadeOut)
                {
                    instance.TargetScreenOverlayColor = new Color(newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0);
                    SetScreenOverlayColor(newScreenOverlayColor);
                }
                else
                {
                    instance.TargetScreenOverlayColor = newScreenOverlayColor;
                    SetScreenOverlayColor(new Color(newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0));
                }

                instance.DeltaColor = (instance.TargetScreenOverlayColor - instance.CurrentScreenOverlayColor) / fadeDuration;
            }
        }

        /// <summary>
        /// Starts the fade from color newScreenOverlayColor, after a delay, with Action OnFadeFinish.
        /// </summary>
        /// <param name='newScreenOverlayColor'>
        /// New screen overlay color.
        /// </param>
        /// <param name='willFadeOut'>
        /// Will fade out (from opaque to transparent) if true and false simply does the opposite.
        /// </param>
        /// <param name='fadeDuration'>
        /// Fade duration.
        /// </param>
        /// <param name='fadeDelay'>
        /// Fade delay.
        /// </param>
        /// <param name='onFadeFinish'>
        /// On fade finish, doWork().
        /// </param>
        public static void StartAlphaFade(Color newScreenOverlayColor, bool willFadeOut, float fadeDuration, float fadeDelay, Action onFadeFinish)
        {
            if (fadeDuration <= 0.0f)
                SetScreenOverlayColor(newScreenOverlayColor);
            else
            {
                instance.OnFadeFinish = onFadeFinish;
                instance.FadeDelay = Time.time + fadeDelay;

                if (willFadeOut)
                {
                    instance.TargetScreenOverlayColor = new Color(newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0);
                    SetScreenOverlayColor(newScreenOverlayColor);
                }
                else
                {
                    instance.TargetScreenOverlayColor = newScreenOverlayColor;
                    SetScreenOverlayColor(new Color(newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0));
                }
                instance.DeltaColor = (instance.TargetScreenOverlayColor - instance.CurrentScreenOverlayColor) / fadeDuration;
            }
        }

        private void Die()
        {
            actualInstance = null;
            Destroy(gameObject);
        }

        private void OnApplicationQuit()
        {
            actualInstance = null;
        }
    }
}