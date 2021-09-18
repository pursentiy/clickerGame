using System.Collections.Generic;
using Figures;
using Storage.Levels.Params;

namespace Handlers
{
    public interface IProgressHandler
    {
        void InitializeHandler(List<LevelParams> levelsParams);
        void UpdateProgress(int levelNumber, FigureType figureType);
        bool CheckForLevelCompletion(int levelNumber);
        List<LevelParams> GetAllLevelsParams();
        LevelParams GetLevelByNumber(int levelNumber);
        void ResetLevelProgress(int levelNumber);
    }
}