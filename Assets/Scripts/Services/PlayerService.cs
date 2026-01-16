using System.Collections.Generic;
using System.Linq;
using Common.Currency;
using Extensions;
using Plugins.FSignal;
using Storage;
using Storage.Snapshots;

namespace Services
{
    public class PlayerService
    {
        public ProfileSnapshot ProfileSnapshot { get; private set; }
        public FSignal ProfileSnapshotInitialized = new ();
        public bool IsMusicOn => GameParamsSnapshot?.IsMusicOn ?? true;
        public bool IsSoundOn => GameParamsSnapshot?.IsSoundOn ?? true;
        
        private GameParamsSnapshot GameParamsSnapshot { get; set; }
        
        public void SetMusicAvailable(bool isOn)
        {
            if (GameParamsSnapshot == null)
                return;
            
            GameParamsSnapshot.IsMusicOn = isOn;
        }
        
        public void SetSoundsAvailable(bool isOn)
        {
            if (GameParamsSnapshot == null)
                return;
            
            GameParamsSnapshot.IsSoundOn = isOn;
        }

        public void Initialize(ProfileSnapshot profileSnapshot)
        {
            if (profileSnapshot == null)
            {
                LoggerService.LogError($"{nameof(PlayerService)}.{nameof(Initialize)}: ProfileSnapshot is null");
            }
            
            ProfileSnapshot = profileSnapshot;
            GameParamsSnapshot =  profileSnapshot?.GameParamsSnapshot;
            ProfileSnapshotInitialized.Dispatch();
        }

        public void SetLevelCompleted(int packNumber, int levelNumber, float levelCompletedTime, Stars starsEarned)
        {
            if (levelCompletedTime < 0)
            {
                LoggerService.LogError($"LevelCompletedTime cannot be negative: {levelCompletedTime}. For pack {packNumber} and levelNumber {levelNumber}");
                return;
            }
            
            if (starsEarned < 0)
            {
                LoggerService.LogError($"Earned Stars cannot be negative: {starsEarned}. For pack {packNumber} and levelNumber {levelNumber}");
                return;
            }
            
            var pack = GetOrCreatePack(packNumber);
            if (pack == null)
            {
                LoggerService.LogError($"Cannot get pack {packNumber} from {nameof(GetOrCreatePack)}");
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

                if (level.StarsEarned > starsEarned)
                {
                    level.StarsEarned = starsEarned;
                }
            }
        }

        public bool IsLevelCompleted(int packNumber, int levelNumber)
        {
            var pack = GetOrCreatePack(packNumber);

            var level = pack?.CompletedLevelsSnapshots.FirstOrDefault(x => x.LevelNumber == levelNumber);
            return level != null;
        }

        public PackSnapshot TryGetPack(int packNumber)
        {
            if (ProfileSnapshot == null || ProfileSnapshot.PackSnapshots.IsNullOrEmpty()) 
                return null;
            
            return ProfileSnapshot.PackSnapshots.FirstOrDefault(p => p.PackNumber == packNumber);
        }

        public PackSnapshot GetOrCreatePack(int packNumber)
        {
            if (ProfileSnapshot == null) 
                return null;

            ProfileSnapshot.PackSnapshots ??= new List<PackSnapshot>();

            var pack = ProfileSnapshot.PackSnapshots.FirstOrDefault(p => p.PackNumber == packNumber);
            if (pack != null) 
                return pack;
            
            pack = new PackSnapshot(packNumber, new List<LevelSnapshot>());
            ProfileSnapshot.PackSnapshots.Add(pack);

            return pack;
        }
    }
}