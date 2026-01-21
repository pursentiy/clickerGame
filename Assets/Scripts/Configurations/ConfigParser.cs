using System;
using System.Collections.Generic;
using System.Globalization;
using Services;

namespace Configurations
{
    public static class ConfigParser
    {
        public static ProgressConfiguration ParseCSV(string csvText)
        {
            var packsData = new Dictionary<int, (string Name, int Stars, List<LevelInfoConfiguration> Levels)>();
            var uniqueLevelCheck = new HashSet<string>();

            var lines = csvText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(';');
                if (values.Length < 10) continue; // Теперь 10 колонок

                try
                {
                    int packId = int.Parse(values[0]);
                    string packName = values[1];
                    int starsToUnlock = int.Parse(values[2]);
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

                    string levelKey = $"{packId}_{levelId}";
                    if (uniqueLevelCheck.Contains(levelKey)) continue;
                    uniqueLevelCheck.Add(levelKey);

                    if (!packsData.ContainsKey(packId))
                    {
                        packsData[packId] = (packName, starsToUnlock, new List<LevelInfoConfiguration>());
                    }

                    packsData[packId].Levels.Add(new LevelInfoConfiguration(levelId, levelName, beatingTimes, figureScale, difficulty));
                }
                catch (Exception e)
                {
                    LoggerService.LogError($"[ConfigParser] Ошибка на строке {i}: {e.Message}");
                }
            }

            var finalPacksDict = new Dictionary<int, PackInfoConfiguration>();
            foreach (var kvp in packsData)
            {
                // Предполагаем, что PackInfo принимает IReadOnlyCollection<LevelInfoConfiguration>
                var packInfo = new PackInfoConfiguration(kvp.Value.Stars, kvp.Value.Name, kvp.Value.Levels.AsReadOnly());
                finalPacksDict.Add(kvp.Key, packInfo);
            }

            return new ProgressConfiguration(finalPacksDict);
        }
    }
}