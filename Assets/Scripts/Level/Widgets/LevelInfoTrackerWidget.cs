using Installers;
using Services;
using Storage.Levels.Params;
using Zenject;

namespace Level.Widgets
{
    public class LevelInfoTrackerWidget : InjectableMonoBehaviour
    {
        [Inject] private TimerService _timerService;

        public float _currentLevelPlayingTime;
        public int GetTotalLevelCollectedStars => 1;
        
        private string _levelId;
        private float _timeToBeatMinimumTime;
        
        public void StartLevelTracking(int levelNumber, LevelBeatingTimeInfo levelBeatingTimeInfo)
        {
            _levelId =  levelNumber.ToString();
            
            _timerService.StartTimer(_levelId, levelBeatingTimeInfo.MinimumTime, onUpdate: OnTimerUpdate);
        }

        public void StopLevelTracking()
        {
            _timerService.DeregisterTimer(_levelId);
        }

        private void OnTimerUpdate(float time)
        {
            _timeToBeatMinimumTime = time;
        }
    }
}