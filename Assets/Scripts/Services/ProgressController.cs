using System.Collections.Generic;
using Common.Currency;
using Services.Player;
using Storage;
using Storage.Snapshots;
using Zenject;

namespace Services
{
    public class ProgressController
    {
        [Inject] private readonly GameParamsManager _gameParamsManager;
        [Inject] private readonly PlayerProfileManager _playerProfileManager;
        [Inject] private readonly ProgressProvider _progressProvider;

        public int CurrentPackId { get; private set; }
        public int CurrentLevelId { get; private set; }

        public void SetCurrentLevelId(int currentLevelId)
        {
            CurrentLevelId = currentLevelId;
        }

        public void SetCurrentPackId(int currentPackId)
        {
            CurrentPackId = currentPackId;
        }
        
        public bool SetLevelCompleted(int packId, int levelId, float levelCompletedTime, Stars starsEarned, SavePriority savePriority)
        {
            if (levelCompletedTime < 0)
            {
                LoggerService.LogWarning(this, $"[{nameof(SetLevelCompleted)}]: LevelCompletedTime cannot be negative: {levelCompletedTime}. For packId {packId} and levelId {levelId}");
                return false;
            }
            
            if (starsEarned < 0)
            {
                LoggerService.LogWarning(this, $"[{nameof(SetLevelCompleted)}]: Earned Stars cannot be negative: {starsEarned}. For packId {packId} and levelId {levelId}");
                return false;
            }
            
            if (_progressProvider.TryGetPackSnapshot(packId) == null && !TryAddPackToProfile(packId))
            {
                LoggerService.LogWarning(this, $"[{nameof(SetLevelCompleted)}]: Cannot get packId {packId} neither cannot add it");
                return false;
            }

            var maybeLevelSnapshot = _progressProvider.TryGetLevelSnapshot(packId, levelId);
            if (maybeLevelSnapshot != null)
            {
                if (!TryUpdateLevelSnapshot(packId, levelId, levelCompletedTime, starsEarned))
                {
                    LoggerService.LogWarning(this, $"[{nameof(SetLevelCompleted)}]: Could not update level snapshot for packId {packId} and levelId {levelId}");
                }
            }
            else
            {
                var result = TryAddLevelToProfile(packId, levelId, levelCompletedTime, starsEarned, UnlockStatus.UnlockedByProgress, savePriority);
                if (result) 
                    return true;
                
                LoggerService.LogWarning(this, $"[{nameof(SetLevelCompleted)}]: Could not create level snapshot for packId {packId} and levelId {levelId}");
                return false;
            }
            
            return true;
        }
        
        private bool TryAddPackToProfile(int packId)
        {
            if (!_playerProfileManager.IsInitialized)
            {
                LoggerService.LogWarning(this,$"[{nameof(TryAddPackToProfile)}]: {nameof(PlayerProfileManager)} is not initialized");
                return false;
            }

            var maybePack = _playerProfileManager.TryGetPackSnapshot(packId);
            if (maybePack != null)
            {
                LoggerService.LogError(this, $"Pack {packId} already exists at {nameof(TryAddPackToProfile)}");
                return false;
            }
            
            var newPack = new PackSnapshot(packId, new List<LevelSnapshot>());
            var result = _playerProfileManager.CreatePack(newPack);

            if (!result)
            {
                LoggerService.LogError(this, $"[{nameof(TryAddPackToProfile)}]: Failed to add pack {packId}");
                return false;
            }

            return true;
        }
        
        private bool TryAddLevelToProfile(int packId, int levelId, float completedTime, Stars starsEarned, UnlockStatus unlockStatus, SavePriority savePriority)
        {
            if (!_playerProfileManager.IsInitialized)
            {
                LoggerService.LogWarning(this,$"[{nameof(TryAddLevelToProfile)}]: {nameof(PlayerProfileManager)} is not initialized");
                return false;
            }
            
            var maybePack = _playerProfileManager.TryGetPackSnapshot(packId);
            if (maybePack == null)
            {
                LoggerService.LogError(this, $"No pack for packId {packId} and levelId {levelId} at {nameof(TryAddLevelToProfile)}");
                return false;
            }
            
            var maybeLevel = _playerProfileManager.TryGetLevelSnapshot(packId, levelId);
            if (maybeLevel != null)
            {
                LoggerService.LogError(this, $"Level Already Exists for packId {packId} and levelId {levelId} at {nameof(TryAddLevelToProfile)}");
                return false;
            }
            
            var newLevel = new LevelSnapshot(levelId, completedTime, starsEarned, unlockStatus, 1);
            var result = _playerProfileManager.CreateLevel(packId, newLevel, savePriority);
            if (!result)
            {
                LoggerService.LogError(this, $"[{nameof(TryAddLevelToProfile)}]: Failed to add level for {packId} and levelId {levelId}");
                return false;
            }

            return true;
        }
        
        private bool TryUpdateLevelSnapshot(int packId, int levelId, float newCompletedTime, Stars updatedEarnedStars)
        {
            var levelSnapshot = _progressProvider.TryGetLevelSnapshot(packId, levelId);
            if (levelSnapshot == null)
            {
                return false;
            }
            
            if (newCompletedTime < levelSnapshot.BestCompletedTime)
            {
                levelSnapshot.BestCompletedTime = newCompletedTime;
            }

            if (updatedEarnedStars > levelSnapshot.StarsEarned)
            {
                levelSnapshot.StarsEarned = updatedEarnedStars;
            }

            return true;
        }
    }
}