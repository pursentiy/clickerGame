using System.Collections;
using Installers;
using Storage.Audio;
using UnityEngine;
using Zenject;

namespace Handlers
{
    public class SoundHandler : InjectableMonoBehaviour
    {
        [Inject] private AudioStorageData _audioStorageData;

        [SerializeField] private AudioSource _effectsSource;
        [SerializeField] private AudioSource _musicSource;
        [Range(0, 1)] 
        [SerializeField] private float _effectsVolume = 1f;
        [Range(0, 1)] 
        [SerializeField] private float _musicVolume = 0.25f;

        public void StartAmbience(string exceptClipName = "")
        {
            var clip = _audioStorageData.MusicPack.GetRandomSoundExceptSpecific(exceptClipName);

            if (clip == null)
                return;
            
            _musicSource.PlayOneShot(clip.clip);
            StartCoroutine(AwaitForNextAmbienceSong(clip));
        }

        private IEnumerator AwaitForNextAmbienceSong(Sound soundParams)
        {
            yield return new WaitForSeconds(soundParams.clip.length);
            StartAmbience(soundParams.name);
        }

        public void PlaySound(string clipName, float volume = 1)
        {
            var clip = _audioStorageData.EffectsPack.GetClipByName(clipName);
            
            if (clip != null)
                _effectsSource.PlayOneShot(clip);
        }

        public void PlayButtonSound()
        {
            var clip = _audioStorageData.ButtonSoundsPack.GetRandomClip();
            
            if (clip != null)
                _effectsSource.PlayOneShot(clip);
        }

        public void SetMusicVolume(bool isOn)
        {
            if (!isOn)
            {
                _musicSource.volume = 0;
                return;
            }

            _musicSource.volume = _musicVolume;
        }
        
        public void SetSoundVolume(bool isOn)
        {
            if (!isOn)
            {
                _effectsSource.volume = 0;
                return;
            }

            _effectsSource.volume = _effectsVolume;
        }
    }
}