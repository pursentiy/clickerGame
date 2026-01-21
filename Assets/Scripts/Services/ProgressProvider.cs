using System;
using System.Collections.Generic;
using System.Linq;
using Common.Currency;
using Common.Data.Info;
using Services.Player;
using Storage;
using Storage.Levels;
using Storage.Snapshots;
using Zenject;

namespace Services
{
    public class ProgressProvider
    {
        [Inject] private PlayerProfileManager _playerProfileManager;
        [Inject] private PlayerCurrencyService _playerCurrencyService;
        [Inject] private GameConfigurationProvider _gameConfigurationProvider;

        public int GetAllPacksCount() => _gameConfigurationProvider.IsInitialized ? _gameConfigurationProvider.GetPacksCount() : 0;
        public IReadOnlyCollection<PackInfo> GetAllPacks() => _gameConfigurationProvider.IsInitialized ? _gameConfigurationProvider.PacksInfo : Array.Empty<PackInfo>();
        
        public bool IsPackAvailable(int packNumber)
        {
            var starsToUnlock = GetStarsCountForPackUnlocking(packNumber);
            return starsToUnlock != null && _playerCurrencyService.Stars >= starsToUnlock;
        }
        
        public int GetAllAvailablePacksCount()
        {
            if (!_gameConfigurationProvider.IsInitialized)
            {
                LoggerService.LogWarning($"[{nameof(GetAllAvailablePacksCount)}]: {nameof(GameConfigurationProvider)} is not initialized");
                return 0;
            }

            return _gameConfigurationProvider.GetPacksIds().Count(IsPackAvailable);
        }
        
        public Stars? GetStarsCountForPackUnlocking(int packNumber)
        {
            if (!_gameConfigurationProvider.IsInitialized)
            {
                LoggerService.LogWarning($"[{nameof(GetStarsCountForPackUnlocking)}]: {nameof(GameConfigurationProvider)} is not initialized");
                return null;
            }

            var pack = _gameConfigurationProvider.GetPackById(packNumber);
            return pack?.StarsToUnlock;
        }
        
        public bool IsLevelCompleted(int packNumber, int levelNumber)
        {
            return TryGetLevelSnapshot(packNumber, levelNumber) != null;
        }
        
        public bool IsLevelAvailableToPlay(int packId, int levelId)
        {
            if (!_gameConfigurationProvider.IsInitialized)
            {
                LoggerService.LogWarning($"[{nameof(IsLevelAvailableToPlay)}]: {nameof(GameConfigurationProvider)} is not initialized");
                return false;
            }
            
            if (!IsPackAvailable(packId))
                return false;
            
            if (levelId == 1)
                return true;

            var previousLevelId = levelId - 1;
            return IsLevelCompleted(packId, previousLevelId);
        }
        
        public int GetLevelsCountInPack(int packId, bool availableLevelsToPlay = false)
        {
            if (!_gameConfigurationProvider.IsInitialized)
            {
                LoggerService.LogWarning($"[{nameof(GetLevelsCountInPack)}]: {nameof(GameConfigurationProvider)} is not initialized");
                return 0;
            }

            var levelsIds = _gameConfigurationProvider.GetLevelsIds(packId);
            if (levelsIds == null)
                return 0;

            return availableLevelsToPlay ? levelsIds.Count(i => IsLevelAvailableToPlay(packId, i)) : levelsIds.Count();
        }
        
        public Stars? GetEarnedStarsForLevel(int packId, int levelId)
        {
            return TryGetLevelSnapshot(packId, levelId)?.StarsEarned;
        }
        
        public PackSnapshot TryGetPackSnapshot(int packId)
        {
            if (!_playerProfileManager.IsInitialized)
            {
                LoggerService.LogWarning(this,$"[{nameof(TryGetPackSnapshot)}]: {nameof(PlayerProfileManager)} is not initialized");
                return null;
            }
            
            return _playerProfileManager.TryGetPackSnapshot(packId);
        }
        
        public LevelSnapshot TryGetLevelSnapshot(int packId, int levelId)
        {
            if (!_playerProfileManager.IsInitialized)
            {
                LoggerService.LogWarning(this,$"[{nameof(TryGetLevelSnapshot)}]: {nameof(PlayerProfileManager)} is not initialized");
                return null;
            }
            
            return _playerProfileManager.TryGetLevelSnapshot(packId, levelId);
        }
    }
}