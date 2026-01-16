using Common.Currency;
using Services.Player;
using Storage;
using Storage.Snapshots;
using Zenject;

namespace Services
{
    public class ProgressController
    {
        [Inject] private readonly PlayerProfileManager _playerProfileManager;

        public int CurrentPackNumber { get; private set; }
        public int CurrentLevelNumber { get; private set; }

        public void SetCurrentLevelNumber(int currentLevelNumber)
        {
            CurrentLevelNumber = currentLevelNumber;
        }

        public void SetCurrentPackNumber(int currentPackNumber)
        {
            CurrentPackNumber = currentPackNumber;
        }
        
        public bool SetLevelCompleted(int packNumber, int levelNumber, float levelCompletedTime, Stars starsEarned)
        {
            if (levelCompletedTime < 0)
            {
                LoggerService.LogWarning(this, $"[{nameof(SetLevelCompleted)}]: LevelCompletedTime cannot be negative: {levelCompletedTime}. For pack {packNumber} and levelNumber {levelNumber}");
                return;
            }
            
            if (starsEarned < 0)
            {
                LoggerService.LogWarning(this, $"[{nameof(SetLevelCompleted)}]: Earned Stars cannot be negative: {starsEarned}. For pack {packNumber} and levelNumber {levelNumber}");
                return;
            }
            
            var pack = GetOrCreatePack(packNumber);
            if (pack == null)
            {
                LoggerService.LogWarning(this, $"[{nameof(SetLevelCompleted)}]: Cannot get pack {packNumber} from {nameof(GetOrCreatePack)}");
                return;
            }
            
            var level = pack.CompletedLevelsSnapshots.FirstOrDefault(x => x.LevelNumber == levelNumber);
            if (level == null)
            {
                //TODO ADD CORRECT FIELD
                pack.CompletedLevelsSnapshots.Add(new LevelSnapshot(levelNumber, levelCompletedTime, starsEarned, UnlockStatus.UnlockedByProgress, 1));
            }
            else
            {
                if (levelCompletedTime < level.BestCompletedTime)
                {
                    level.BestCompletedTime = levelCompletedTime;
                }

                if (starsEarned > level.StarsEarned)
                {
                    level.StarsEarned = starsEarned;
                }
            }
        }
    }
}