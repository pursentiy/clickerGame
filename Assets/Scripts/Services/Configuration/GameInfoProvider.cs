using System.Collections.Generic;
using System.Linq;
using Common.Currency;
using Common.Data.Info;
using Configurations;
using Configurations.Progress;
using Extensions;
using Services.Base;
using Storage;
using UnityEngine;
using Zenject;

namespace Services.Configuration
{
    public class GameInfoProvider : DisposableService
    {
        [Inject] private readonly LevelsParamsStorageData _levelsParamsStorageData;
        [Inject] private readonly GameConfigurationProvider _gameConfigurationProvider;
        
        private List<PackInfo> _packInfoList = new();
        
        public bool IsInitialized { get; private set; }
        public IReadOnlyCollection<PackInfo> PacksInfo => _packInfoList;
        public IEnumerable<int> GetPacksIds() => _packInfoList.Select(p => p.PackId);
        public IEnumerable<int> GetLevelsIds(int packId) => GetLevelParamsByPack(packId)?.Select(p => p.LevelId) ?? Enumerable.Empty<int>();
        public int GetPacksCount() => _packInfoList.Count;
        //TODO MOVE TO CONFIG
        public Stars StarsRewardForAds => new (10);
        
        public PackInfo GetPackById(int packId)
        {
            var pack = _packInfoList.FirstOrDefault(levelParams => levelParams.PackId == packId);
            if (pack != null)
                return pack;
            
            LoggerService.LogWarning(this, $"{nameof(GetPackById)}: {nameof(PacksInfo)} is null for packId {packId}");
            return null;
        }
        
        public IReadOnlyList<LevelInfo> GetLevelParamsByPack(int packId)
        {
            var pack = GetPackById(packId);
            if (pack == null)
                return null;
            
            if (pack.LevelsInfo != null)
                return pack.LevelsInfo;
            
            LoggerService.LogWarning(this, $"{nameof(GetLevelParamsByPack)}: {nameof(List<LevelInfo>)} is null for packId {packId}");
            return null;
        }

        public LevelInfo GetLevelByNumber(int packId, int levelId)
        {
            var levelParams = GetLevelParamsByPack(packId);
            if (levelParams == null)
                return null;

            var levelParamData = levelParams.FirstOrDefault(levelParam => levelParam.LevelId == levelId);
            if (levelParamData != null)
                return levelParamData;
            
            LoggerService.LogWarning(this, $"{nameof(GetLevelByNumber)}: {nameof(LevelInfo)} for packId {packId} and levelId {levelId} in {this}");
            return null;
        }
        
        protected override void OnInitialize()
        {
            InitPacksInfoList();
        }

        protected override void OnDisposing()
        {
            
        }

        private void InitPacksInfoList()
        {
            var config = _gameConfigurationProvider.GetConfig<ProgressConfiguration>();
            if (config == null)
            {
                LoggerService.LogError(this, $"{nameof(InitPacksInfoList)}: {nameof(ProgressConfiguration)} is null");
                return;
            }
            
            var rawStorageData = _levelsParamsStorageData.DefaultPacksParamsList;
            _packInfoList = rawStorageData.MergeWithConfig(config.PacksInfoDictionary);

            IsInitialized = true;
        }
    }
}