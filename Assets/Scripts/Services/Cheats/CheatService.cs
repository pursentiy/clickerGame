using Zenject;

namespace Services.Cheats
{
    public class CheatService
    {
        [Inject] private PlayerRepositoryService _playerRepositoryService;
        [Inject] private ProfileBuilderService _profileBuilderService;
        
        public void ResetProgress()
        {
            var snapshot = _profileBuilderService.BuildNewProfileSnapshot();
            _playerRepositoryService.SavePlayerSnapshot(snapshot);
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}