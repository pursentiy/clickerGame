using System.Collections.Generic;
using UnityEngine;

namespace Storage.Audio
{
    [CreateAssetMenu(fileName = "AudioStorage", menuName = "ScriptableObjects/AudioStorage")]
    public class AudioStorageData : ScriptableObject
    {
        [SerializeField] private SoundPack _effectsPacks;
        [SerializeField] private SoundPack _musicPacks;
        [SerializeField] private SoundPack _buttonsPacks;

        public SoundPack EffectsPack => _effectsPacks;
        public SoundPack MusicPack => _musicPacks;
        public SoundPack ButtonSoundsPack => _buttonsPacks;
    }
}