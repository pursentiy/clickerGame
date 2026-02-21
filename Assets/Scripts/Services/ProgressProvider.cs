using System;
using System.Collections.Generic;
using System.Linq;
using Common.Currency;
using Common.Data.Info;
using Configurations.Progress;
using Extensions;
using Services.Configuration;
using Services.Player;
using Storage;
using Storage.Snapshots;
using Zenject;

namespace Services
{
    public class ProgressProvider
    {
        [Inject] private PlayerProfileController _playerProfileController;
        [Inject] private PlayerCurrencyManager _playerCurrencyManager;
        [Inject] private GameInfoProvider _gameInfoProvider;

        public int GetAllPacksCount() => _gameInfoProvider.IsInitialized ? _gameInfoProvider.GetPacksCount() : 0;
        public IReadOnlyCollection<PackInfo> GetAllPacks() => _gameInfoProvider.IsInitialized ? _gameInfoProvider.PacksInfo : Array.Empty<PackInfo>();

        public PackInfo GetPackInfo(int packId)
        {
            if (!_gameInfoProvider.IsInitialized)
                return null;
            
            return _gameInfoProvider.PacksInfo.FirstOrDefault(i => i.PackId == packId);
        }

        public LevelInfo GetLevelInfo(int packId, int levelId)
        {
            var pack = GetPackInfo(packId);
            if (pack == null || pack.LevelsInfo.IsCollectionNullOrEmpty())
                return null;
            
            var level = pack.LevelsInfo.FirstOrDefault(i => i.LevelId == levelId);
            return level;
        }
        
        public bool IsPackAvailable(int packNumber)
        {
            var currencyToUnlock = GetCurrencyToUnlock(packNumber);
            if (currencyToUnlock == null || currencyToUnlock.Count == 0)
                return true;

            foreach (var currency in currencyToUnlock)
            {
                if (currency != null && currency.GetCount() > 0 && !_playerCurrencyManager.CanSpend(currency))
                    return false;
            }
            return true;
        }
        
        public int GetAllAvailablePacksCount()
        {
            if (!_gameInfoProvider.IsInitialized)
            {
                LoggerService.LogWarning($"[{nameof(GetAllAvailablePacksCount)}]: {nameof(GameInfoProvider)} is not initialized");
                return 0;
            }

            return _gameInfoProvider.GetPacksIds().Count(IsPackAvailable);
        }
        
        public List<ICurrency> GetCurrencyToUnlock(int packNumber)
        {
            if (!_gameInfoProvider.IsInitialized)
            {
                LoggerService.LogWarning($"[{nameof(GetCurrencyToUnlock)}]: {nameof(GameInfoProvider)} is not initialized");
                return null;
            }

            var pack = _gameInfoProvider.GetPackById(packNumber);
            if (pack?.CurrencyToUnlock == null)
                return new List<ICurrency>();

            return new List<ICurrency> { pack.CurrencyToUnlock };
        }

        public bool HasLevelBeenCompletedBefore(int packNumber, int levelNumber)
        {
            return TryGetSavedLevelSnapshot(packNumber, levelNumber) != null;
        }
        
        public bool IsLevelAvailableToPlay(int packId, int levelId)
        {
            if (!_gameInfoProvider.IsInitialized)
            {
                LoggerService.LogWarning($"[{nameof(IsLevelAvailableToPlay)}]: {nameof(GameInfoProvider)} is not initialized");
                return false;
            }
            
            if (!IsPackAvailable(packId))
                return false;
            
            if (levelId == 1)
                return true;

            var previousLevelId = levelId - 1;
            return HasLevelBeenCompletedBefore(packId, previousLevelId);
        }
        
        public int GetLevelsCountInPack(int packId, bool availableLevelsToPlay = false)
        {
            if (!_gameInfoProvider.IsInitialized)
            {
                LoggerService.LogWarning($"[{nameof(GetLevelsCountInPack)}]: {nameof(GameInfoProvider)} is not initialized");
                return 0;
            }

            var levelsIds = _gameInfoProvider.GetLevelsIds(packId);
            if (levelsIds == null)
                return 0;

            return availableLevelsToPlay ? levelsIds.Count(i => IsLevelAvailableToPlay(packId, i)) : levelsIds.Count();
        }
        
        public Stars? GetEarnedStarsForLevel(int packId, int levelId)
        {
            return TryGetSavedLevelSnapshot(packId, levelId)?.StarsEarned;
        }
        
        public PackSnapshot TryGetSavedPackSnapshot(int packId)
        {
            if (!_playerProfileController.IsInitialized)
            {
                LoggerService.LogWarning(this,$"[{nameof(TryGetSavedPackSnapshot)}]: {nameof(PlayerProfileController)} is not initialized");
                return null;
            }
            
            return _playerProfileController.TryGetPackSnapshot(packId);
        }

        /// <summary>
        /// Default: Unlocked if pack is in snapshot OR the previous pack (same type) is unlocked.
        /// Freemium: Unlocked if pack is in snapshot (and considered unlocked).
        /// </summary>
        public PackStatus GetPackStatus(int packId)
        {
            var packInfo = GetPackInfo(packId);
            if (packInfo == null)
                return PackStatus.Locked;

            var packType = packInfo.PackType;
            var snapshot = TryGetSavedPackSnapshot(packId);
            bool isUnlockedInSnapshot = snapshot != null && snapshot.IsUnlocked != UnlockStatus.NotUnlocked;

            if (packType == PackType.Default)
            {
                var defaultPacks = _gameInfoProvider.PacksInfo.Where(p => p.PackType == PackType.Default).ToList();
                var index = defaultPacks.FindIndex(p => p.PackId == packId);
                if (index < 0)
                    return PackStatus.Locked;
                bool hasInSnapshot = snapshot != null;
                if (index == 0)
                    return (hasInSnapshot && isUnlockedInSnapshot) ? PackStatus.Unlocked : PackStatus.Locked;
                var previousPackId = defaultPacks[index - 1].PackId;
                var previousSnapshot = TryGetSavedPackSnapshot(previousPackId);
                bool previousUnlocked = previousSnapshot != null && previousSnapshot.IsUnlocked != UnlockStatus.NotUnlocked;
                return (hasInSnapshot && isUnlockedInSnapshot) || previousUnlocked ? PackStatus.Unlocked : PackStatus.Locked;
            }

            if (packType == PackType.Freemium)
                return isUnlockedInSnapshot ? PackStatus.Unlocked : PackStatus.Locked;

            return PackStatus.Locked;
        }

        /// <summary>
        /// Default: FreePack. Freemium with enough currency: AvailableToUnlock. Otherwise: NotEnoughCurrency.
        /// </summary>
        public PackUnlockCurrencyStatus EnoughCurrencyToUnlockPack(int packId)
        {
            var packInfo = GetPackInfo(packId);
            if (packInfo == null)
                return PackUnlockCurrencyStatus.NotEnoughCurrency;

            if (packInfo.PackType == PackType.Default)
                return PackUnlockCurrencyStatus.FreePack;

            if (packInfo.PackType == PackType.Freemium)
                return IsPackAvailable(packId) ? PackUnlockCurrencyStatus.AvailableToUnlock : PackUnlockCurrencyStatus.NotEnoughCurrency;

            return PackUnlockCurrencyStatus.NotEnoughCurrency;
        }
        
        public LevelSnapshot TryGetSavedLevelSnapshot(int packId, int levelId)
        {
            if (!_playerProfileController.IsInitialized)
            {
                LoggerService.LogWarning(this,$"[{nameof(TryGetSavedLevelSnapshot)}]: {nameof(PlayerProfileController)} is not initialized");
                return null;
            }
            
            return _playerProfileController.TryGetLevelSnapshot(packId, levelId);
        }
    }
}