using Installers;
using Services;
using Storage.Levels.Params;
using Zenject;

namespace Level.Widgets
{
    public class LevelInfoTrackerWidget : InjectableMonoBehaviour
    {
        [Inject] private TimeService _timeService;

        public float _currentLevelPlayingTime;
        public int GetTotalLevelCollectedStars => 1;
        
        private string _levelId;
        private float _timeToBeatMinimumTime;
        
        public void StartLevelTracking(int levelNumber, LevelBeatingTimeInfo levelBeatingTimeInfo)
        {
            _levelId =  levelNumber.ToString();
            
            _timeService.StartTimer(_levelId, levelBeatingTimeInfo.MinimumTime, onUpdate: OnTimerUpdate);
        }

        public void StopLevelTracking()
        {
            _timeService.DeregisterTimer(_levelId);
        }

        private void OnTimerUpdate(float time)
        {
            _timeToBeatMinimumTime = time;
        }
    }
}