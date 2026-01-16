using Extensions;
using Plugins.FSignal;
using Services.Base;
using Storage.Snapshots;
using Utilities.Disposable;
using Zenject;

namespace Services.Player
{
    public class GameManager : DisposableService
    {
        [Inject] private PlayerProfileManager _playerProfileManager;
        
        private GameParamsSnapshot _gameParamsSnapshot;
        
        public bool IsInitialized { get; private set; }
        public readonly FSignal GameParamsSnapshotInitializedSignal = new();
        
        public bool IsMusicOn => _gameParamsSnapshot?.IsMusicOn ?? true;
        public bool IsSoundOn => _gameParamsSnapshot?.IsSoundOn ?? true;
        public string LanguageCode => _gameParamsSnapshot?.Language ?? string.Empty;
        
        public void SetMusicAvailable(bool isOn)
        {
            if (!IsInitialized)
            {
                LoggerService.LogWarning(this, $"{nameof(SetMusicAvailable)} was called but not initialized.");
                return;
            }
            
            if (_gameParamsSnapshot.IsMusicOn == isOn)
                return;
            
            _gameParamsSnapshot.IsMusicOn = isOn;
            _playerProfileManager.SaveProfile();
        }
        
        public void SetSoundAvailable(bool isOn)
        {
            if (!IsInitialized)
            {
                LoggerService.LogWarning(this, $"{nameof(SetSoundAvailable)} was called but not initialized.");
                return;
            }
            
            if (_gameParamsSnapshot.IsSoundOn == isOn)
                return;
            
            _gameParamsSnapshot.IsSoundOn = isOn;
            _playerProfileManager.SaveProfile();
        }
        
        public void UpdateLanguage(string languageCode)
        {
            if (!IsInitialized)
            {
                LoggerService.LogWarning(this, $"{nameof(UpdateLanguage)} was called but not initialized.");
                return;
            }

            if (languageCode.IsNullOrEmpty())
            {
                LoggerService.LogError(this, $"{nameof(UpdateLanguage)}: languageCode is null or empty");
                return;
            }
        
            _gameParamsSnapshot.Language = languageCode;
            _playerProfileManager.SaveProfile();
        }
        
        protected override void OnInitialize()
        {
            _playerProfileManager.ProfileSnapshotInitializedSignal.MapListener(OnProfileSnapshotInitializedSignal).DisposeWith(this);
        }

        protected override void OnDisposing()
        {

        }

        private void OnProfileSnapshotInitializedSignal()
        {
            _gameParamsSnapshot = _playerProfileManager.TryGetGameParamsSnapshot();
            
            if (_gameParamsSnapshot == null)
            {
                LoggerService.LogWarning(this, $"[{nameof(OnProfileSnapshotInitializedSignal)}]: {nameof(GameParamsSnapshot)} is null");
                return;
            }

            IsInitialized = true;
            GameParamsSnapshotInitializedSignal.Dispatch();
        }
    }
}