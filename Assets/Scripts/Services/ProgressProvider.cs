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
        [Inject] private PlayerProfileManager _playerProfileManager;
        [Inject] private PlayerCurrencyService _playerCurrencyService;
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
            var starsToUnlock = GetStarsCountForPackUnlocking(packNumber);
            return starsToUnlock != null && _playerCurrencyService.Stars >= starsToUnlock;
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
        
        public Stars? GetStarsCountForPackUnlocking(int packNumber)
        {
            if (!_gameInfoProvider.IsInitialized)
            {
                LoggerService.LogWarning($"[{nameof(GetStarsCountForPackUnlocking)}]: {nameof(GameInfoProvider)} is not initialized");
                return null;
            }

            var pack = _gameInfoProvider.GetPackById(packNumber);
            return pack?.StarsToUnlock;
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
            if (!_playerProfileManager.IsInitialized)
            {
                LoggerService.LogWarning(this,$"[{nameof(TryGetSavedPackSnapshot)}]: {nameof(PlayerProfileManager)} is not initialized");
                return null;
            }
            
            return _playerProfileManager.TryGetPackSnapshot(packId);
        }
        
        public LevelSnapshot TryGetSavedLevelSnapshot(int packId, int levelId)
        {
            if (!_playerProfileManager.IsInitialized)
            {
                LoggerService.LogWarning(this,$"[{nameof(TryGetSavedLevelSnapshot)}]: {nameof(PlayerProfileManager)} is not initialized");
                return null;
            }
            
            return _playerProfileManager.TryGetLevelSnapshot(packId, levelId);
        }
    }
}