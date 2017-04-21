using UnityEngine;

namespace Miscellaneous
{
    // A utility class that will provide an instance of a mono behavior
    // If there is no instance of a object found, it will try to find a
    // GameManager object (by tag), if this is found the will add and return
    // otherwise it will throw an error in the console and return null
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (T)FindObjectOfType(typeof(T));

                    if (instance == null)
                    {
                        Debug.LogError("An instance of " + typeof(T) + " is needed in the scene, but there is none.");

                        GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");

                        if (gameManager)
                            instance = gameManager.AddComponent<T>();
                        else
                            Debug.LogError("A GameObject with the tag \"GameManager\" is needed in the scene, but there is none.");
                    }
                }

                return instance;
            }
        }
    }
}