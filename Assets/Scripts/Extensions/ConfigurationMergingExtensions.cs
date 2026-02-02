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
                if (!configs.TryGetValue(storagePack.PackId, out var configPack))
                {
                    LoggerService.LogError($"[{nameof(MergeWithConfig)}] Config not found for PackId: {storagePack.PackId}");
                    continue;
                }

                // Передаем уровни конкретного пака для мерджа
                var mergedLevels = MergeLevels(storagePack.LevelsParams, configPack.Levels);

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
                // Ищем уровень по ID строго внутри текущего пака (configLevels уже отфильтрованы по паку в MergeWithConfig)
                var configLevel = configLevels.FirstOrDefault(l => l.LevelId == storageLevel.LevelId);

                if (configLevel == null)
                {
                    // Теперь ошибка будет указывать на конкретный ID уровня, который не найден в конфиге этого пака
                    LoggerService.LogError($"[{nameof(MergeLevels)}] Level configuration not found for LevelId: {storageLevel.LevelId} in current pack context.");
                    continue;
                }
                
                var figuresInfo = storageLevel.LevelsFiguresParams
                    .Select(i => new FigureInfo(i.FigureId, i._figureTargetWidget, i.figureMenuWidget)).ToList();
                
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