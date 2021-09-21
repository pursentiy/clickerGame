using System;
using System.Collections.Generic;
using UnityEngine;

namespace Storage.Levels.Params
{
    [Serializable]
    public class PackParams
    {
        public int PackNumber;
        public bool PackCompleted;
        public bool PackPlayable;
        public List<LevelParams> LevelsParams;
    }
}