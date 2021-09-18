using System.Collections.Generic;
using Storage.Levels.Params;

namespace Services
{
    public interface IProcessProgressDataService
    {
        void SaveProgress(List<LevelParams> levelsParams);
        List<LevelParams> LoadProgress();
    }
}