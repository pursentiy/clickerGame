using RSG;
using Storage.Snapshots;
using Zenject;

namespace Services
{
    public class PlayerProfileManager
    {
        [Inject] private readonly PlayerService _playerService;
        [Inject] private readonly PlayerRepositoryService _playerRepositoryService;
        
        public void SaveProfile()
        {
            if (_playerService.ProfileSnapshot == null)
            {
                LoggerService.LogError(this, $"{nameof(SaveProfile)}: Profile snapshot is null.");
                return;
            }
            
            //TODO ADD PROMISE AWAIT
            _playerRepositoryService.SavePlayerSnapshot(_playerService.ProfileSnapshot);
        }
        
        public IPromise<ProfileSnapshot> LoadProfile()
        {
            return _playerRepositoryService.LoadPlayerSnapshot();
        }
    }
}