using System;
using System.Collections.Generic;
using System.Linq;
using Common.Currency;
using Common.Data.Info;
using Extensions;
using Services.Configuration;
using Services.Player;
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
            if (currencyToUnlock == null || currencyToUnlock.GetCount() <= 0)
                return true;

            return _playerCurrencyManager.CanSpend(currencyToUnlock);
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
        
        public ICurrency GetCurrencyToUnlock(int packNumber)
        {
            if (!_gameInfoProvider.IsInitialized)
            {
                LoggerService.LogWarning($"[{nameof(GetCurrencyToUnlock)}]: {nameof(GameInfoProvider)} is not initialized");
                return null;
            }

            var pack = _gameInfoProvider.GetPackById(packNumber);
            return pack?.CurrencyToUnlock;
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