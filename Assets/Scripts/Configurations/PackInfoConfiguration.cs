using System.Collections.Generic;

namespace Configurations
{
    public class PackInfoConfiguration
    {
        public string PackName { get; private set; }
        public int StarsToUnlock { get; private set; }
        public IReadOnlyCollection<LevelInfoConfiguration> Levels { get; private set; }

        public PackInfoConfiguration(int starsToUnlock, string packName, IReadOnlyCollection<LevelInfoConfiguration> levels)
        {
            Levels = levels;
            StarsToUnlock = starsToUnlock;
            PackName = packName;
        }
    }
}