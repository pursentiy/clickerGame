using System.Collections.Generic;
using UnityEngine;

namespace Common.Data.Info
{
    public class PackInfo
    {
        public int PackId;
        public string PackName;
        public GameObject PackImagePrefab;
        public int StarsToUnlock;
        public List<LevelInfo> LevelsInfo;

        public PackInfo(int packId, string packName, GameObject packImagePrefab, int starsToUnlock, List<LevelInfo> levelsInfo)
        {
            PackId = packId;
            PackName = packName;
            PackImagePrefab = packImagePrefab;
            StarsToUnlock = starsToUnlock;
            LevelsInfo = levelsInfo;
        }
    }
}