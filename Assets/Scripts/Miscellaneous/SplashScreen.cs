using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Miscellaneous
{
    public class SplashScreen : MonoBehaviour
    {
        public int MenuScene;
        public float TimeBeforeFadeOut;

        private void Start()
        {
            StartCoroutine(StartFade());
        }

        // Wait for a X amount of seconds before loading the next screen
        private IEnumerator StartFade()
        {
            yield return new WaitForSeconds(TimeBeforeFadeOut);
            SceneManager.LoadScene(MenuScene);
        }
    }
}