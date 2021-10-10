using System.Collections.Generic;
using UnityEngine;

namespace Storage.Audio
{
    [CreateAssetMenu(fileName = "AudioStorage", menuName = "ScriptableObjects/AudioStorage")]
    public class AudioStorageData : ScriptableObject
    {
        [SerializeField] private List<SoundPack> _soundPacks;
    }
}