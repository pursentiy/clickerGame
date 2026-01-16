using Plugins.FSignal;
using RSG;
using Storage.Snapshots;
using Zenject;

namespace Services
{
    public class PlayerProfileManager
    {
        [Inject] private readonly PlayerRepositoryService _playerRepositoryService;
        
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
        
        public void SetSoundAvailable(bool isOn)
        {
            if (GameParamsSnapshot == null)
                return;
            
            GameParamsSnapshot.IsSoundOn = isOn;
        }

        public void Initialize(ProfileSnapshot profileSnapshot)
        {
            if (profileSnapshot == null)
            {
                LoggerService.LogError($"{GetType().Name}.{nameof(Initialize)}: ProfileSnapshot is null");
            }
            
            ProfileSnapshot = profileSnapshot;
            GameParamsSnapshot =  profileSnapshot?.GameParamsSnapshot;
            ProfileSnapshotInitialized.Dispatch();
        }
        
        public void SaveProfile()
        {
            if (ProfileSnapshot == null)
            {
                LoggerService.LogError(this, $"{nameof(SaveProfile)}: Profile snapshot is null.");
                return;
            }
            
            //TODO ADD PROMISE AWAIT
            _playerRepositoryService.SavePlayerSnapshot(ProfileSnapshot);
        }
        
        public IPromise<ProfileSnapshot> LoadProfile()
        {
            return _playerRepositoryService.LoadPlayerSnapshot();
        }
    }
}