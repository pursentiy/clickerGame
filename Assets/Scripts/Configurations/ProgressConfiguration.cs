using System.Collections.Generic;
using System.Linq;
using Extensions;

namespace Configurations
{
    public class ProgressConfiguration
    {
        public readonly IReadOnlyDictionary<int, PackInfoConfiguration> PacksInfoDictionary;

        public ProgressConfiguration(IReadOnlyDictionary<int, PackInfoConfiguration> packsInfo)
        {
            PacksInfoDictionary = packsInfo;
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