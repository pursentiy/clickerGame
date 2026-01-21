using System.Collections.Generic;
using System.Linq;
using Common.Data.Info;
using Configurations;
using Services;
using Storage.Levels;

namespace Extensions
{
    public static class ConfigurationMergingExtensions
    {
        public static List<PackInfo> MergeWithConfig(
            this List<PackParamsData> storagePacks,
            IReadOnlyDictionary<int, PackInfoConfiguration> configs)
        {
            var result = new List<PackInfo>();

            if (storagePacks == null || configs == null)
                return result;

            foreach (var storagePack in storagePacks)
            {
                // Ищем соответствующий конфиг пака по ID
                if (!configs.TryGetValue(storagePack.PackId, out var configPack))
                {
                    LoggerService.LogError($"[Merge] Config not found for PackId: {storagePack.PackId}");
                    continue;
                }

                // Мерджим уровни внутри пака
                var mergedLevels = MergeLevels(storagePack.LevelsParams, configPack.Levels);

                // Создаем итоговый объект пака
                var packInfo = new PackInfo(
                    storagePack.PackId,
                    configPack.PackName,
                    storagePack.PackImagePrefab,
                    configPack.StarsToUnlock,
                    mergedLevels
                );

                result.Add(packInfo);
            }

            return result;
        }

        private static List<LevelInfo> MergeLevels(
            List<LevelParamsData> storageLevels,
            IReadOnlyCollection<LevelInfoConfiguration> configLevels)
        {
            var result = new List<LevelInfo>();

            if (storageLevels == null || configLevels == null)
                return result;

            foreach (var storageLevel in storageLevels)
            {
                var configLevel = configLevels.FirstOrDefault(l => l.LevelId == storageLevel.LevelId);

                if (configLevel == null)
                {
                    LoggerService.LogError($"[Merge] Config not found for LevelId: {storageLevel.LevelId}");
                    continue;
                }
                
                var figuresInfo = storageLevel.LevelsFiguresParams
                    .Select(i => new FigureInfo(i.FigureId, i.FigureTarget, i.FigureMenu)).ToList();
                
                var levelInfo = new LevelInfo(
                    storageLevel.LevelId,
                    configLevel.LevelName,
                    configLevel.Difficulty,
                    storageLevel.LevelImage,
                    configLevel.FigureScale,
                    figuresInfo,
                    configLevel.BeatingTimes
                );

                result.Add(levelInfo);
            }

            return result;
        }
    }
}