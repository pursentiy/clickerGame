using System;
using System.Collections.Generic;

namespace Storage.Audio
{
    [Serializable]
    public class SoundPack
    {
        public string name;
        public List<Sound> sounds;
    }
}