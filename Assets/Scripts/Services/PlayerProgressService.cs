using System.Collections.Generic;
using System.Linq;
using Common.Currency;
using Extensions;
using Storage;
using Storage.Levels;
using Storage.Snapshots;
using Zenject;

namespace Services
{
    public class PlayerProgressService
    {
        [Inject] private PlayerProfileManager _playerProfileManager;
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
        
                public bool IsLevelCompleted(int packNumber, int levelNumber)
        {
            var pack = GetOrCreatePack(packNumber);

            var level = pack?.CompletedLevelsSnapshots.FirstOrDefault(x => x.LevelNumber == levelNumber);
            return level != null;
        }

        public PackSnapshot TryGetPack(int packNumber)
        {
            if (_playerProfileManager.ProfileSnapshot == null || _playerProfileManager.ProfileSnapshot.PackSnapshots.IsNullOrEmpty()) 
                return null;
            
            return _playerProfileManager.ProfileSnapshot.PackSnapshots.FirstOrDefault(p => p.PackNumber == packNumber);
        }

        public PackSnapshot GetOrCreatePack(int packNumber)
        {
            if (_playerProfileManager.ProfileSnapshot == null) 
                return null;

            _playerProfileManager.ProfileSnapshot.PackSnapshots ??= new List<PackSnapshot>();

            var pack = _playerProfileManager.ProfileSnapshot.PackSnapshots.FirstOrDefault(p => p.PackNumber == packNumber);
            if (pack != null) 
                return pack;
            
            pack = new PackSnapshot(packNumber, new List<LevelSnapshot>());
            _playerProfileManager.ProfileSnapshot.PackSnapshots.Add(pack);

            return pack;
        }

        public void SetLevelCompleted(int packNumber, int levelNumber, float levelCompletedTime, Stars starsEarned)
        {
            if (levelCompletedTime < 0)
            {
                LoggerService.LogError($"LevelCompletedTime cannot be negative: {levelCompletedTime}. For pack {packNumber} and levelNumber {levelNumber}");
                return;
            }
            
            if (starsEarned < 0)
            {
                LoggerService.LogError($"Earned Stars cannot be negative: {starsEarned}. For pack {packNumber} and levelNumber {levelNumber}");
                return;
            }
            
            var pack = GetOrCreatePack(packNumber);
            if (pack == null)
            {
                LoggerService.LogError($"Cannot get pack {packNumber} from {nameof(GetOrCreatePack)}");
                return;
            }
            
            var level = pack.CompletedLevelsSnapshots.FirstOrDefault(x => x.LevelNumber == levelNumber);
            if (level == null)
            {
                //TODO ADD CORRECT FIELD
                pack.CompletedLevelsSnapshots.Add(new LevelSnapshot(levelNumber, levelCompletedTime, starsEarned, UnlockStatus.UnlockedByProgress, 1));
            }
            else
            {
                if (levelCompletedTime < level.BestCompletedTime)
                {
                    level.BestCompletedTime = levelCompletedTime;
                }

                if (level.StarsEarned > starsEarned)
                {
                    level.StarsEarned = starsEarned;
                }
            }
        }

        public int GetAllPacksCount()
        {
            return _packParamsList.IsNullOrEmpty() ? 0 : _packParamsList.Count;
        }
        
        public int GetAllAvailablePacksCount()
        {
            if (_packParamsList.IsNullOrEmpty())
                return 0;

            return _packParamsList.Count(i => IsPackAvailable(i.PackNumber));
        }

        public bool IsPackAvailable(int packNumber)
        {
            var starsToUnlock = GetPackStarsToUnlock(packNumber);
            if (starsToUnlock.GetCount() < 0)
                return false;

            return _playerCurrencyService.Stars.GetCount() >= starsToUnlock.GetCount();
        }

        public Stars GetPackStarsToUnlock(int packNumber)
        {
            if (_packParamsList.IsNullOrEmpty())
            {
                LoggerService.LogWarning($"Levels Params is null in {this}");
                return new Stars(-1);
            }

            var pack = _packParamsList.FirstOrDefault(i => i.PackNumber == packNumber);
            if (pack == null)
            {
                LoggerService.LogWarning($"Pack Params is null in {this} for PackNumber {packNumber}");
                return new Stars(-1);
            }
            
            return new Stars(pack.StarsToUnlock);
        }
        
        public int GetAllLevelsCount(int packNumber)
        {
            if (_packParamsList.IsNullOrEmpty())
                return 0;
            
            var pack = _packParamsList.FirstOrDefault(i => i.PackNumber == packNumber);
            if (pack == null)
                return 0;
            
            return pack.LevelsParams.Count;
        }
        
        public int GetAllAvailableLevelsCount(int packNumber)
        {
            if (_packParamsList.IsNullOrEmpty())
                return 0;
            
            var pack = _packParamsList.FirstOrDefault(i => i.PackNumber == packNumber);
            if (pack == null)
                return 0;

            return pack.LevelsParams.Count(i => IsLevelAvailable(packNumber, i.LevelNumber));
        }

        public bool IsLevelAvailable(int packNumber, int levelNumber)
        {
            if (levelNumber <= 0 || !IsPackAvailable(packNumber))
                return false;

            if (levelNumber == 1)
                return true;

            var previousLevelIsCompleted = IsLevelCompleted(packNumber, levelNumber - 1);
            return previousLevelIsCompleted;
        }
        
        public Stars? GetEarnedStarsForLevel(int packNumber, int levelNumber)
        {
            var pack = TryGetPack(packNumber);

            var level = pack?.CompletedLevelsSnapshots.FirstOrDefault(x => x.LevelNumber == levelNumber);

            if (level == null)
                return null;
            
            
            return new Stars((int)level.StarsEarned.GetCount());
        }

        public bool TrySetOrUpdateLevelCompletion(int packNumber, int levelNumber, Stars earnedStars, float completeTime)
        {
            var pack = GetOrCreatePack(packNumber);
            if (pack == null)
                return false;
            
            var level = pack.CompletedLevelsSnapshots.FirstOrDefault(x => x.LevelNumber == levelNumber);
            if (level != null)
            {
                if (earnedStars > level.StarsEarned)
                {
                    level.StarsEarned = earnedStars;
                }
                
                if (completeTime < level.BestCompletedTime || level.BestCompletedTime <= 0)
                {
                    level.BestCompletedTime = completeTime;
                }

                level.PlayCount++;
                
                return true;
            }
            
            //TODO ADD CORRECT FIELD
            pack.CompletedLevelsSnapshots.Add(new LevelSnapshot(levelNumber, completeTime, earnedStars, UnlockStatus.UnlockedByProgress, 1));
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