using System.Collections;
using Storage.Audio;
using UnityEngine;
using Zenject;

namespace Handlers
{
    public class SoundHandler : MonoBehaviour
    {
        [Inject] private AudioStorageData _audioStorageData;

        [SerializeField] private AudioSource _effectsSource;
        [SerializeField] private AudioSource _musicSource;
        [Range(0, 1)] 
        [SerializeField] private float _effectsVolume = 1f;
        [Range(0, 1)] 
        [SerializeField] private float _musicVolume = 0.25f;

        private Coroutine _ambientAwaitCoroutine;

        //TODO FIX EXCEPT CLIP CHOOSING
        //TODO ADD VOLUME SETTING AND PLAYING LOGIC
        public void StartAmbience(string exceptClipName = "")
        {
            var clip = _audioStorageData.MusicPack.GetRandomSoundExceptSpecific(exceptClipName);
            if (clip == null)
                return;

            if (_musicSource.isPlaying)
                return;
            
            _musicSource.PlayOneShot(clip.clip, 0.7f);
            
            if (gameObject != null && gameObject.activeInHierarchy)
                _ambientAwaitCoroutine = StartCoroutine(AwaitForNextAmbienceSong(clip));
        }

        public void StopAmbience()
        {
            if (_ambientAwaitCoroutine != null)
                StopCoroutine(_ambientAwaitCoroutine);
            
            _musicSource.Stop();
        }
        
        public void MuteAll()
        {
            SetMusicVolume(false);
            SetSoundVolume(false);
        }

        public void UnmuteAll()
        {
            SetMusicVolume(true);
            SetSoundVolume(true);
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

        private void OnDestroy()
        {
            if (_ambientAwaitCoroutine != null)
                StopCoroutine(_ambientAwaitCoroutine);
        }
        
        private IEnumerator AwaitForNextAmbienceSong(Sound soundParams)
        {
            yield return new WaitForSeconds(soundParams.clip.length);
            StartAmbience();
        }
    }
}