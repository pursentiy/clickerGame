using System.Collections.Generic;
using Common.Currency;
using Configurations.Progress;
using UnityEngine;

namespace Common.Data.Info
{
    public class PackInfo
    {
        public int PackId;
        public string PackName;
        public GameObject PackImagePrefab;
        public ICurrency CurrencyToUnlock;
        public PackType PackType;
        public List<LevelInfo> LevelsInfo;

        public PackInfo(int packId, string packName, GameObject packImagePrefab, ICurrency currencyToUnlock, PackType packType, List<LevelInfo> levelsInfo)
        {
            PackId = packId;
            PackName = packName;
            PackImagePrefab = packImagePrefab;
            CurrencyToUnlock = currencyToUnlock;
            PackType = packType;
            LevelsInfo = levelsInfo;
        }
    }
}