using System.Collections.Generic;
using System.Linq;
using Services.Base;
using Storage;
using Storage.Levels;
using Zenject;

namespace Services
{
    public class GameConfigurationProvider : DisposableService
    {
        [Inject] private readonly LevelsParamsStorageData _levelsParamsStorageData;
        
        private List<PackParamsData> _packParamsList = new();
        
        public bool IsInitialized { get; private set; }
        public IReadOnlyCollection<PackParamsData> PackParamsData => _packParamsList;
        public IEnumerable<int> GetPacksIds() => _packParamsList.Select(p => p.PackId);
        public IEnumerable<int> GetLevelsIds(int packId) => GetLevelParamsByPack(packId)?.Select(p => p.LevelId) ?? Enumerable.Empty<int>();
        public int GetPacksCount() => _packParamsList.Count;
        
        public PackParamsData GetPackById(int packId)
        {
            var pack = _packParamsList.FirstOrDefault(levelParams => levelParams.PackId == packId);
            if (pack != null)
                return pack;
            
            LoggerService.LogWarning(this, $"{nameof(GetPackById)}: {nameof(PackParamsData)} is null for packId {packId}");
            return null;
        }
        
        public IReadOnlyList<LevelParamsData> GetLevelParamsByPack(int packId)
        {
            var pack = GetPackById(packId);
            if (pack == null)
                return null;
            
            if (pack.LevelsParams != null)
                return pack.LevelsParams;
            
            LoggerService.LogWarning(this, $"{nameof(GetLevelParamsByPack)}: {nameof(List<LevelParamsData>)} is null for packId {packId}");
            return null;
        }

        public LevelParamsData GetLevelByNumber(int packId, int levelId)
        {
            var levelParams = GetLevelParamsByPack(packId);
            if (levelParams == null)
                return null;

            var levelParamData = levelParams.FirstOrDefault(levelParam => levelParam.LevelId == levelId);
            if (levelParamData != null)
                return levelParamData;
            
            LoggerService.LogWarning(this, $"{nameof(GetLevelByNumber)}: {nameof(LevelParamsData)} for packId {packId} and levelId {levelId} in {this}");
            return null;
        }
        
        protected override void OnInitialize()
        {
            InitPackParamsList();
        }

        protected override void OnDisposing()
        {
            
        }

        private void InitPackParamsList()
        {
            _packParamsList = _levelsParamsStorageData.DefaultPacksParamsList;

            if (_packParamsList == null)
            {
                _packParamsList = new();
                LoggerService.LogWarning("No pack parameters have been set.");
                return;
            }

            IsInitialized = true;
        }
    }
}