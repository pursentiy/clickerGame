using System.Collections.Generic;

namespace Configurations.Progress
{
    public class PackInfoConfiguration
    {
        public string PackName { get; private set; }
        public int StarsToUnlock { get; private set; }
        public PackType PackType { get; private set; }
        public IReadOnlyCollection<LevelInfoConfiguration> Levels { get; private set; }

        public PackInfoConfiguration(int starsToUnlock, string packName, PackType packType, IReadOnlyCollection<LevelInfoConfiguration> levels)
        {
            Levels = levels;
            StarsToUnlock = starsToUnlock;
            PackName = packName;
            PackType = packType;
        }
    }
}