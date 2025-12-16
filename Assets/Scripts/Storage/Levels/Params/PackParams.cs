using System;
using System.Collections.Generic;
using UnityEngine;

namespace Storage.Levels.Params
{
    [Serializable]
    public class PackParams
    {
        public int PackNumber;
        public int StarsToUnlock;
        public List<LevelParams> LevelsParams;
    }
}