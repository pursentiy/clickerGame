using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Currency;
using Extensions;
using Services;

namespace Configurations.Progress
{
    public class ProgressConfiguration : ICSVConfig
    {
        public IReadOnlyDictionary<int, PackInfoConfiguration> PacksInfoDictionary { get; private set; }

        public void Parse(string csvText)
        {
            var packsData = new Dictionary<int, (string Name, ICurrency CurrencyToUnlock, PackType PackType, List<LevelInfoConfiguration> Levels)>();
            var uniqueLevelCheck = new HashSet<string>();

            var lines = csvText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(';');
                if (values.Length < 11) continue; // Now 11 columns (added PackType)

                try
                {
                    int packId = int.Parse(values[0]);
                    string packName = values[1];
                    var currencyToUnlock = CurrencyParser.Parse(values[2]);
                    int levelId = int.Parse(values[3]);
                    string levelName = values[4];
                    
                    float[] beatingTimes = new float[]
                    {
                        float.Parse(values[5], CultureInfo.InvariantCulture),
                        float.Parse(values[6], CultureInfo.InvariantCulture),
                        float.Parse(values[7], CultureInfo.InvariantCulture)
                    };

                    float figureScale = float.Parse(values[8], CultureInfo.InvariantCulture);
                    LevelDifficulty difficulty = (LevelDifficulty)int.Parse(values[9]);
                    PackType packType = (PackType)int.Parse(values[10]);

                    string levelKey = $"{packId}_{levelId}";
                    if (uniqueLevelCheck.Contains(levelKey)) continue;
                    uniqueLevelCheck.Add(levelKey);

                    if (!packsData.ContainsKey(packId))
                    {
                        packsData[packId] = (packName, currencyToUnlock, packType, new List<LevelInfoConfiguration>());
                    }

                    packsData[packId].Levels.Add(new LevelInfoConfiguration(levelId, levelName, beatingTimes, figureScale, difficulty));
                }
                catch (Exception e)
                {
                    LoggerService.LogError($"[{nameof(ProgressConfiguration)}] Error on line {i}: {e.Message}");
                }
            }

            var finalPacksDict = new Dictionary<int, PackInfoConfiguration>();
            foreach (var kvp in packsData)
            {
                var packInfo = new PackInfoConfiguration(kvp.Value.CurrencyToUnlock, kvp.Value.Name, kvp.Value.PackType, kvp.Value.Levels.AsReadOnly());
                finalPacksDict.Add(kvp.Key, packInfo);
            }
            
            PacksInfoDictionary = finalPacksDict;
        }
        
        public PackInfoConfiguration GetPackInfo(int packId)
        {
            if (PacksInfoDictionary.IsCollectionNullOrEmpty() ||
                !PacksInfoDictionary.TryGetValue(packId, out var packInfo))
                return null;

            return packInfo;
        }

        public LevelInfoConfiguration GetLevelInfo(int packId, int levelId)
        {
            var packInfo = GetPackInfo(packId);
            if (packInfo == null)
                return null;
            
            if (packInfo.Levels.IsCollectionNullOrEmpty())
                return null;

            var levelInfo = packInfo.Levels.FirstOrDefault(i => i != null && i.LevelId == levelId);
            return levelInfo;
        }
    }
}