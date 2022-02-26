using System;
using System.Collections;
using Installers;
using Storage.Audio;
using UnityEngine;
using Zenject;

namespace Handlers
{
    public class SoundHandler : InjectableMonoBehaviour, ISoundHandler
    {
        [Inject] private AudioStorageData _audioStorageData;

        [SerializeField] private AudioSource _effectsSource;
        [SerializeField] private AudioSource _musicSource;

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

        public void PlaySound(string clipName)
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
    }
}