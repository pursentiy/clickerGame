using System;
using System.Collections.Generic;
using UnityEngine;

namespace Storage.Levels
{
    [Serializable]
    public class PackParamsData
    {
        public int PackId;
        public string PackName;
        public GameObject PackImagePrefab;
        public int StarsToUnlock;
        public List<LevelParamsData> LevelsParams;
    }
}