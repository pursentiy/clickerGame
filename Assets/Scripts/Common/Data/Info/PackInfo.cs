using System.Collections.Generic;
using Configurations.Progress;
using UnityEngine;

namespace Common.Data.Info
{
    public class PackInfo
    {
        public int PackId;
        public string PackName;
        public GameObject PackImagePrefab;
        public int StarsToUnlock;
        public PackType PackType;
        public List<LevelInfo> LevelsInfo;

        public PackInfo(int packId, string packName, GameObject packImagePrefab, int starsToUnlock, PackType packType, List<LevelInfo> levelsInfo)
        {
            PackId = packId;
            PackName = packName;
            PackImagePrefab = packImagePrefab;
            StarsToUnlock = starsToUnlock;
            PackType = packType;
            LevelsInfo = levelsInfo;
        }
    }
}