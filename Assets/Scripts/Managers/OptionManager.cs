using UnityEngine.UI;
using Miscellaneous;

namespace Managers
{
    public class OptionManager : Singleton<OptionManager>
    {
        private Slider soundSlider;
        public Toggle SoundMute;

        private void Awake()
        {

        }

        public void SetSoundLevel(float soundVolume)
        {

        }

        public bool SoundToggle(bool isEnabled)
        {
            return true;
        }
    }
}