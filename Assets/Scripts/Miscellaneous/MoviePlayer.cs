using System.Collections;
using System.IO;
using Managers.Menu;
//using Menu;
using UnityEngine;
using UnityEngine.UI;

namespace Miscellaneous
{
    /// <summary>
    /// This class is used for handling cutscene animations.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    [RequireComponent(typeof(AudioSource))]
    public class MoviePlayer : MonoBehaviour
    {
        //public GameObject MenuPanelToShowAfterwards;
        public MenuPanelManager MenuPanelManager;
        public string MovieFileName; // The file extension has to be included
        public string MovieAssetBundleName;

        public bool IsPlayingMovie;

        // Using this because the conditional directive didn't work as the DEBUG symbol was always defined
        public bool ShowDebugMessages = false;

        // Initialization
        private void Start()
        {
            if (!IsPlayingMovie)
            {
                if (ShowDebugMessages)
                    Debug.Log("Playing the cutscene automatically");

                StartCoroutine(PlayCutscene());
            }
            else if (ShowDebugMessages)
                Debug.Log("Cutscene is already playing");
        }

#if (UNITY_STANDALONE || UNITY_EDITOR)
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                SkipMovie();
        }
#endif

        // Plays cutscene on either a regular computer or mobile platform
        public IEnumerator PlayCutscene()
        {
            if (IsPlayingMovie)
            {
                if (ShowDebugMessages)
                    Debug.Log("is already playing");

                yield return null;
            }
            else
                IsPlayingMovie = true;

#if (UNITY_ANDROID || UNITY_IPHONE)
            // This is to hide the white image required for the MovieTexture of the standalone build
            RawImage rawImage = GetComponent<RawImage>();
            rawImage.color = Color.black;

            StartCoroutine(PlayAndroidMovie(MovieFileName, Color.black, FullScreenMovieControlMode.CancelOnInput, FullScreenMovieScalingMode.AspectFit));
#elif (UNITY_STANDALONE || UNITY_EDITOR)

            // Loading the asset bundle
            AssetBundle myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, MovieAssetBundleName));

            // Validation
            if (myLoadedAssetBundle == null)
            {
                if (ShowDebugMessages)
                    Debug.Log("Failed to load AssetBundle!");

                SkipMovie();

                yield return null;
            }

            // Setting references for easy access and a little resource efficiency
            RawImage rawImage = GetComponent<RawImage>();
            AudioSource audioSource = GetComponent<AudioSource>();

            // Assign the assets to the components
            rawImage.texture = myLoadedAssetBundle.LoadAsset<MovieTexture>("Assets/StreamingAssets/" + MovieFileName);
            audioSource.clip = (rawImage.texture as MovieTexture).audioClip;

            // Starting up the components
            (rawImage.texture as MovieTexture).Play();
            audioSource.Play();

            if (ShowDebugMessages)
                Debug.Log("show menu panel after intro");

            yield return StartCoroutine(ShowMenuPanelAfterwards());

            if (ShowDebugMessages)
                Debug.Log("unloading assetbundle is unreachable"); // TODO: make code below reachable somehow or simply remove it

            // Unloading the asset bundle because we don't need it anymore
            myLoadedAssetBundle.Unload(false);
#endif

            yield return null;
        }

#if (UNITY_STANDALONE || UNITY_EDITOR)
        private IEnumerator ShowMenuPanelAfterwards()
        {
            MovieTexture movieTexture = GetComponent<RawImage>().texture as MovieTexture;

            while (movieTexture.isPlaying)
                yield return new WaitForEndOfFrame();

            SkipMovie();
        }
#endif

#if UNITY_ANDROID
        // Plays movie for android only. This pauses the whole game.
        public IEnumerator PlayAndroidMovie(string movieFileName, Color backgroundColor, FullScreenMovieControlMode fullscreenControlMode, FullScreenMovieScalingMode fullscreenScalingMode)
        {
            Handheld.PlayFullScreenMovie(movieFileName, backgroundColor, fullscreenControlMode, fullscreenScalingMode);

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            SkipMovie();
        }
#endif

        public void SkipMovie()
        {
            //MenuPanelManager.ShowPanel(MenuPanelToShowAfterwards);
            //MenuPanelToShowAfterwards.SetActive(true);
            gameObject.SetActive(false);

            MenuPanelManager.EnableTheMagicallyDisabledPanel(); // Quick-fix for the disappearing menu main screen content panel

            IsPlayingMovie = false;

            if (ShowDebugMessages)
                Debug.Log("Is done playing");

            //StopCoroutine(ShowMenuPanelAfterwards());
        }
    }
}