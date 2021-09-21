using System;
using System.Collections.Generic;

namespace Storage.Levels
{
    [Serializable]
    public class PackParamsData
    {
        public int PackNumber;
        public List<LevelParamsData> LevelsParams;
    }
}