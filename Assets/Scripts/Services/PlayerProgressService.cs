using System.Collections.Generic;
using System.Linq;
using Extensions;
using GlobalParams;
using Storage.Levels.Params;
using Storage.Snapshots;
using UnityEngine;
using Zenject;

namespace Services
{
    public class PlayerProgressService
    {
        [Inject] private LevelsParamsStorage _levelsParamsStorage;
        [Inject] private PlayerSnapshotService _playerSnapshotService;
        
        private List<PackParams> _packParamsList;
        private ProfileSettingsParams _profileSettings;

        public int CurrentPackNumber { get; set; } = -1;
        public int CurrentLevelNumber { get; set; } = -1;

        public bool ProfileSettingsSound
        {
            get => _profileSettings.IsSoundOn;
            set => _profileSettings.IsSoundOn = value;
        }

        public bool ProfileSettingsMusic
        {
            get => _profileSettings.IsMusicOn;
            set => _profileSettings.IsMusicOn = value;
        }

        public void InitializeProfileSettings()
        {
            _profileSettings = new ProfileSettingsParams(true, true);
        }
        
        public void InitializeHandler(List<PackParams> levelsParams, List<PackParams> newLevelsParams = null)
        {
            if (levelsParams == null)
            {
                LoggerService.LogWarning($"Levels Params is null in {this}");
                return;
            }
            
            _packParamsList = levelsParams;
        }

        public void ResetProgress(int packNumber, int levelNumber)
        {
            var levelProgress = GetLevelByNumber(packNumber, levelNumber);

            if (levelProgress == null)
            {
                return;
            }
            
            foreach (var levelFigureParam in levelProgress.LevelFiguresParamsList)
            {
                levelFigureParam.Completed = false;
            }
        }

        public bool IsPackAvailable(int packNumber)
        {
            if (_packParamsList.IsNullOrEmpty())
                return false;

            var pack = _packParamsList.FirstOrDefault(i => i.PackNumber == packNumber);
            return pack != null;
        }

        public bool IsLevelAvailable(int packNumber, int levelNumber)
        {
            if (levelNumber <= 0 || !IsPackAvailable(packNumber))
                return false;

            if (levelNumber == 1)
                return true;

            var previousLevelIsCompleted = IsLevelCompleted(packNumber, levelNumber);
            return previousLevelIsCompleted;
        }

        public bool IsLevelCompleted(int packNumber, int levelNumber)
        {
            return _playerSnapshotService.HasLevelInPack(packNumber, levelNumber);
        }

        public int? GetEarnedStarsForLevel(int packNumber, int levelNumber)
        {
            var pack = _playerSnapshotService.TryGetPack(packNumber);

            var level = pack?.CompletedLevelsSnapshots.FirstOrDefault(x => x.LevelNumber == levelNumber);
            return level?.StarsEarned;
        }

        public bool TrySetOrUpdateLevelCompletion(int packNumber, int levelNumber, int earnedStars, float completeTime)
        {
            var pack = _playerSnapshotService.GetOrCreatePack(packNumber);
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

        public PackParams GetPackPackByNumber(int packNumber)
        {
            var pack = _packParamsList.FirstOrDefault(levelParams => levelParams.PackNumber == packNumber);
            
            if (pack != null)
            {
                return pack;
            }
            
            LoggerService.LogWarning($"Could not get pack by {packNumber} in {this}");
            return null;
            
        }

        public LevelParams GetLevelByNumber(int packNumber, int levelNumber)
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

        public void ResetLevelProgress(int packNumber, int levelNumber)
        {
            var currentLevel = GetLevelByNumber(packNumber, levelNumber);
            currentLevel.LevelFiguresParamsList.ForEach(levelParams =>
            {
                levelParams.Completed = false;
            });
        }

        public List<LevelParams> GetLevelsByPack(int packNumber)
        {
            var levelsParams = _packParamsList.FirstOrDefault(levelParams => levelParams.PackNumber == packNumber)?.LevelsParams;

            if (levelsParams != null)
            {
                return levelsParams;
            }
            
            LoggerService.LogWarning($"Could not update progress in level {packNumber} in {this}");
            return null;
        }
        
        public List<PackParams> GetPackParams()
        {
            return _packParamsList;
        }
    }
}