using System.Collections.Generic;
using Storage.Levels.Params;

namespace Handlers
{
    public interface IProgressHandler
    {
        void InitializeProfileSettings();
        void InitializeHandler(List<PackParams> levelsParams, List<PackParams> newLevelsParams = null);
        void UpdateProgress(int packNumber, int levelNumber, int figureId);
        bool CheckForLevelCompletion(int packNumber, int levelNumber);
        List<PackParams> GetCurrentProgress();
        PackParams GetPackPackByNumber(int packNumber);
        LevelParams GetLevelByNumber(int packNumber, int levelNumber);
        List<LevelParams> GetLevelsByPack(int packNumber);
        void ResetLevelProgress(int packNumber, int levelNumber);
        bool ProfileSettingsSound { get; set; }
        bool ProfileSettingsMusic { get; set; }
    }
}