using UnityEngine;

namespace Managers
{
    public class SettingsManager : MonoBehaviour
    {
        private void Start()
        {
            PlatformDependentCheck();
        }

        private void PlatformDependentCheck()
        {
#if UNITY_ANDROID
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
        }
    }
}