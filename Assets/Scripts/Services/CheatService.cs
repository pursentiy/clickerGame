using UnityEngine;
using Zenject;

namespace Services
{
    //TODO REMOVE
    public class CheatService
    {
        [Inject] private PlayerRepositoryService _playerRepositoryService;
        [Inject] private ProfileBuilderService _profileBuilderService;
        
        public void CheatResetProgress()
        {
            var snapshot = _profileBuilderService.BuildNewProfileSnapshot();
            _playerRepositoryService.SavePlayerSnapshot(snapshot);
            Application.Quit();
        }
    }
}