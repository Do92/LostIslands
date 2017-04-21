using UnityEngine;
using Miscellaneous;

namespace Managers
{
    public class SoundManager : Singleton<SoundManager>
    {
        public bool IsSoundEnabled;
        public float SoundVolume;
        public AudioListener AudioListener;
        public AudioSource SoundClipSource;
        public AudioSource MusicSource;

        public void PlaySound(AudioClip audio)
        {
            SoundClipSource.PlayOneShot(audio);
        }

        public void StopSounds()
        {
            SoundClipSource.Stop();
        }

        public void PlayMusic(AudioClip music)
        {
            //musicSource.Play();
        }

        public void StopMusic()
        {

        }
    }
}