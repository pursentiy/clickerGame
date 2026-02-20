using System.Collections.Generic;
using Common.Currency;

namespace Configurations.Progress
{
    public class PackInfoConfiguration
    {
        public string PackName { get; private set; }
        public ICurrency CurrencyToUnlock { get; private set; }
        public PackType PackType { get; private set; }
        public IReadOnlyCollection<LevelInfoConfiguration> Levels { get; private set; }

        public PackInfoConfiguration(ICurrency currencyToUnlock, string packName, PackType packType, IReadOnlyCollection<LevelInfoConfiguration> levels)
        {
            Levels = levels;
            CurrencyToUnlock = currencyToUnlock;
            PackName = packName;
            PackType = packType;
        }
    }
}