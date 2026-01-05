using System.Collections.Generic;
using System.Linq;
using Extensions;
using Storage.Levels;
using Storage.Snapshots;
using Zenject;

namespace Services
{
    public class PlayerProgressService
    {
        [Inject] private PlayerService _playerService;
        [Inject] private PlayerCurrencyService _playerCurrencyService;
        
        private List<PackParamsData> _packParamsList;

        public int CurrentPackNumber { get; set; } = -1;
        public int CurrentLevelNumber { get; set; } = -1;
        
        public void InitializeHandler(List<PackParamsData> levelsParams, List<PackParamsData> newLevelsParams = null)
        {
            if (levelsParams == null)
            {
                LoggerService.LogWarning($"Levels Params is null in {this}");
                return;
            }
            
            _packParamsList = levelsParams;
        }

        public bool IsPackAvailable(int packNumber)
        {
            var starsToUnlock = GetPackStarsToUnlock(packNumber);
            if (starsToUnlock < 0)
                return false;

            return _playerCurrencyService.Stars >= starsToUnlock;
        }

        public int GetPackStarsToUnlock(int packNumber)
        {
            if (_packParamsList.IsNullOrEmpty())
            {
                LoggerService.LogWarning($"Levels Params is null in {this}");
                return -1;
            }

            var pack = _packParamsList.FirstOrDefault(i => i.PackNumber == packNumber);
            if (pack == null)
            {
                LoggerService.LogWarning($"Pack Params is null in {this} for PackNumber {packNumber}");
                return -1;
            }
            
            return pack.StarsToUnlock;
        }

        public bool IsLevelAvailable(int packNumber, int levelNumber)
        {
            if (levelNumber <= 0 || !IsPackAvailable(packNumber))
                return false;

            if (levelNumber == 1)
                return true;

            var previousLevelIsCompleted = _playerService.IsLevelCompleted(packNumber, levelNumber - 1);
            return previousLevelIsCompleted;
        }
        
        public int? GetEarnedStarsForLevel(int packNumber, int levelNumber)
        {
            var pack = _playerService.TryGetPack(packNumber);

            var level = pack?.CompletedLevelsSnapshots.FirstOrDefault(x => x.LevelNumber == levelNumber);
            return level?.StarsEarned;
        }

        public bool TrySetOrUpdateLevelCompletion(int packNumber, int levelNumber, int earnedStars, float completeTime)
        {
            var pack = _playerService.GetOrCreatePack(packNumber);
            if (pack == null)
                return false;
            
            var level = pack.CompletedLevelsSnapshots.FirstOrDefault(x => x.LevelNumber == levelNumber);
            if (level != null)
            {
                if (earnedStars > level.StarsEarned)
                {
                    level.StarsEarned = earnedStars;
                }
                
                if (completeTime < level.LevelCompletedTime || level.LevelCompletedTime <= 0)
                {
                    level.LevelCompletedTime = completeTime;
                }
                
                return true;
            }
            
            pack.CompletedLevelsSnapshots.Add(new LevelSnapshot(levelNumber, completeTime, earnedStars));
            return true;
        }

        public PackParamsData GetPackPackByNumber(int packNumber)
        {
            var pack = _packParamsList.FirstOrDefault(levelParams => levelParams.PackNumber == packNumber);
            
            if (pack != null)
            {
                return pack;
            }
            
            LoggerService.LogWarning($"Could not get pack by {packNumber} in {this}");
            return null;
            
        }

        public LevelParamsData GetLevelByNumber(int packNumber, int levelNumber)
        {
            var levelProgress = _packParamsList.FirstOrDefault(levelParams => levelParams.PackNumber == packNumber)?
                .LevelsParams.FirstOrDefault(levelParams => levelParams.LevelNumber == levelNumber);

            if (levelProgress != null)
            {
                return levelProgress;
            }
            
            LoggerService.LogWarning($"Could not get level by number {levelNumber} in {this}");
            return null;
        }

        public List<LevelParamsData> GetLevelsByPack(int packNumber)
        {
            var levelsParams = _packParamsList.FirstOrDefault(levelParams => levelParams.PackNumber == packNumber)?.LevelsParams;

            if (levelsParams != null)
            {
                return levelsParams;
            }
            
            LoggerService.LogWarning($"Could not update progress in level {packNumber} in {this}");
            return null;
        }
        
        public List<PackParamsData> GetPackParams()
        {
            return _packParamsList;
        }
    }
}