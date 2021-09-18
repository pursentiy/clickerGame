using System.Collections.Generic;
using System.Linq;
using Figures;
using Installers;
using Storage.Levels.Params;
using UnityEngine;
using Zenject;

namespace Handlers
{
    public class ProgressHandler : InjectableMonoBehaviour, IProgressHandler
    {
        [Inject] private LevelsParamsStorage _levelsParamsStorage;
        
        private List<LevelParams> _gameLevelsProgress;

        public void InitializeHandler(List<LevelParams> levelsParams)
        {
            if (levelsParams == null)
            {
                Debug.LogWarning($"Levels Params is null in {this}");
                return;
            }
            
            _gameLevelsProgress = levelsParams;
            
            SetLevelsVisualHandler();
        }

        private void SetLevelsVisualHandler()
        {
            _gameLevelsProgress.ForEach(levelProgress =>
            {
                levelProgress.LevelVisualHandler = _levelsParamsStorage.DefaultLevelsParamsList.FirstOrDefault(levelParams => levelParams.LevelNumber == levelProgress.LevelNumber)?.LevelVisualHandler;
            });
        }
        
        public void UpdateProgress(int levelNumber, FigureType figureType)
        {
            var levelProgress = GetLevelByNumber(levelNumber);

            if (levelProgress == null)
            {
                return;
            }

            var levelFigure = levelProgress.LevelFiguresParamsList.FirstOrDefault(level => level.FigureType == figureType);
            
            if (levelFigure == null)
            {
                Debug.LogWarning($"Could not update progress with figure type {figureType} in {this}");
                return;
            }
            
            levelFigure.Completed = true;
            
            var levelCompleted = levelProgress.LevelFiguresParamsList.TrueForAll(levelFigureParams =>
                levelFigureParams.Completed);

            levelProgress.LevelCompleted = levelCompleted;

            if (levelCompleted)
            {
                TryUpdateNextLevelPlayableStatus(levelNumber, true);
            }
        }

        private void TryUpdateNextLevelPlayableStatus(int levelNumber, bool value)
        {
            if (levelNumber + 1 > _gameLevelsProgress.Count)
            {
                return;
            }
            
            var levelProgress = GetLevelByNumber(levelNumber + 1);
            levelProgress.LevelPlayable = value;
        }

        public bool CheckForLevelCompletion(int levelNumber)
        {
            var levelProgress = GetLevelByNumber(levelNumber);
            
            if (levelProgress == null)
            {
                return false;
            }

            var progress = levelProgress.LevelFiguresParamsList.TrueForAll(levelFigureParams =>
                levelFigureParams.Completed);

            return progress;
        }
        
        public LevelParams GetLevelByNumber(int levelNumber)
        {
            var levelProgress = _gameLevelsProgress.FirstOrDefault(levelParams => levelParams.LevelNumber == levelNumber);

            if (levelProgress != null)
            {
                return levelProgress;
            }
            
            Debug.LogWarning($"Could not update progress in level {levelNumber} in {this}");
            return null;
        }

        public void ResetLevelProgress(int levelNumber)
        {
            var currentLevel = GetLevelByNumber(levelNumber);
            currentLevel.LevelFiguresParamsList.ForEach(levelParams =>
            {
                levelParams.Completed = false;
            });
        }
        
        public List<LevelParams> GetAllLevelsParams()
        {
            return _gameLevelsProgress;
        }
    }
}