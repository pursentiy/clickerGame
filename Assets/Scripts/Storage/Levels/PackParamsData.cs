using System;
using System.Collections.Generic;
using UnityEngine;

namespace Storage.Levels
{
    [Serializable]
    public class PackParamsData
    {
        public int PackId;
        public GameObject PackImagePrefab;
        public List<LevelParamsData> LevelsParams;
    }
}