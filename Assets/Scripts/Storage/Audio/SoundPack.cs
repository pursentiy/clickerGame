using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Storage.Audio
{
    [Serializable]
    public class SoundPack
    {
        public List<Sound> sounds;

        public AudioClip GetClipByName(string name)
        {
            return sounds.Count <= 0 ? null : sounds.FirstOrDefault(sound => sound.name == name)?.clip;
        }

        public AudioClip GetRandomClip()
        {
            return sounds.Count <= 0 ? null : sounds[Random.Range(0, sounds.Count)].clip;
        }

        public Sound GetRandomSoundExceptSpecific(string name)
        {
            var clips = sounds.Where(sound => sound.name != name).ToList();
            
            return clips.Count <= 0 ? null : clips[Random.Range(0, clips.Count)];
        }
    }
}