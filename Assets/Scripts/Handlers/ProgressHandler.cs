using System.Collections.Generic;
using System.Linq;
using Installers;
using Storage.Levels.Params;
using UnityEngine;
using Zenject;

namespace Handlers
{
    public class ProgressHandler : InjectableMonoBehaviour, IProgressHandler
    {
        [Inject] private LevelsParamsStorage _levelsParamsStorage;
        
        private List<PackParams> _gameProgress;

        public int CurrentPackNumber { get; set; } = -1;
        public int CurrentLevelNumber { get; set; } = -1;

        public void InitializeHandler(List<PackParams> levelsParams)
        {
            if (levelsParams == null)
            {
                Debug.LogWarning($"Levels Params is null in {this}");
                return;
            }
            
            _gameProgress = levelsParams;
            
        }

        public void UpdateProgress(int packNumber, int levelNumber, int figureId)
        {
            var levelProgress = GetLevelByNumber(packNumber, levelNumber);

            if (levelProgress == null)
            {
                return;
            }

            var levelFigure = levelProgress.LevelFiguresParamsList.FirstOrDefault(level => level.FigureId == figureId);
            
            if (levelFigure == null)
            {
                Debug.LogWarning($"Could not update progress with figure id {figureId} in {this}");
                return;
            }
            
            levelFigure.Completed = true;
            
            var levelCompleted = levelProgress.LevelFiguresParamsList.TrueForAll(levelFigureParams =>
                levelFigureParams.Completed);

            levelProgress.LevelCompleted = levelCompleted;

            if (levelCompleted)
            {
                TryUpdateNextLevelPlayableStatus(packNumber, levelNumber, true);
            }
        }

        private void TryUpdateNextLevelPlayableStatus(int packNumber, int levelNumber, bool value)
        {
            if (levelNumber + 1 > _gameProgress.Count)
            {
                return;
            }
            
            var levelProgress = GetLevelByNumber(packNumber, levelNumber + 1);
            levelProgress.LevelPlayable = value;
        }

        public bool CheckForLevelCompletion(int packNumber, int levelNumber)
        {
            var levelProgress = GetLevelByNumber(packNumber, levelNumber);
            
            if (levelProgress == null)
            {
                return false;
            }

            var progress = levelProgress.LevelFiguresParamsList.TrueForAll(levelFigureParams =>
                levelFigureParams.Completed);

            return progress;
        }

        public PackParams GetPackPackByNumber(int packNumber)
        {
            var pack = _gameProgress.FirstOrDefault(levelParams => levelParams.PackNumber == packNumber);
            
            if (pack != null)
            {
                return pack;
            }
            
            Debug.LogWarning($"Could not get pack by {packNumber} in {this}");
            return null;
            
        }

        public LevelParams GetLevelByNumber(int packNumber, int levelNumber)
        {
            var levelProgress = _gameProgress.FirstOrDefault(levelParams => levelParams.PackNumber == packNumber)?
                .LevelsParams.FirstOrDefault(levelParams => levelParams.LevelNumber == levelNumber);

            if (levelProgress != null)
            {
                return levelProgress;
            }
            
            Debug.LogWarning($"Could not get level by number {levelNumber} in {this}");
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
            var levelsParams = _gameProgress.FirstOrDefault(levelParams => levelParams.PackNumber == packNumber)?.LevelsParams;

            if (levelsParams != null)
            {
                return levelsParams;
            }
            
            Debug.LogWarning($"Could not update progress in level {packNumber} in {this}");
            return null;
        }
        
        public List<PackParams> GetCurrentProgress()
        {
            return _gameProgress;
        }
    }
}