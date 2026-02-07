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

        //TODO FIX RANDOM CLIPS CHOOSING
        //TODO ADD VOLUME SETTING AND PLAYING LOGIC
        public void StartAmbience(string exceptClipName = "")
        {
            var sound = _audioStorageData.MusicPack.GetRandomSoundExceptSpecific(exceptClipName);
            if (sound == null || sound.clip == null)
                return;
            
            _musicSource.clip = sound.clip;
            _musicSource.volume = sound.volume;
            _musicSource.loop = true;
            _musicSource.Play();
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
        
        public void PlaySound(string clipName, float volumeOverride = 1, float pitchOverride = 1)
        {
            var sound = _audioStorageData.EffectsPack.GetClipByName(clipName);
            if (sound == null) return;

            var volume = Mathf.Approximately(volumeOverride, 1) ? sound.volume : volumeOverride;
            _effectsSource.pitch = pitchOverride;
            _effectsSource.PlayOneShot(sound.clip, volume);
        }

        public void PlayButtonSound()
        {
            var sound = _audioStorageData.ButtonSoundsPack.GetRandomClip();
            if (sound == null)
                return;
            
            _effectsSource.pitch = 1;
            _effectsSource.PlayOneShot(sound.clip, sound.volume);
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
    }
}