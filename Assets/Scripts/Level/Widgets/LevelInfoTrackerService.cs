using Installers;
using Plugins.FSignal;
using Services;
using Zenject;

namespace Level.Widgets
{
    public class LevelInfoTrackerService
    {
        [Inject] private TimeService _timeService;
        
        public FSignal<float> CurrentLevelPlayingTimeChangedSignal = new ();
        public float CurrentLevelPlayingTime {get; private set;}
        
        private string _levelId;
        private Timer _stopwatchTimer;
        
        public void StartLevelTracking(string levelId)
        {
            ClearData();
            
            _levelId =  levelId;
            CurrentLevelPlayingTime = 0;
            _stopwatchTimer = _timeService.StartStopwatch(_levelId, onUpdate: OnTimerUpdate);
        }

        public void StopLevelTracking()
        {
            _timeService.StopTimer(_levelId);
        }

        public void ClearData()
        {
            _stopwatchTimer = null;
            _levelId = string.Empty;
            CurrentLevelPlayingTime = 0;
        }

        private void OnTimerUpdate(float time)
        {
            var oldTimeInSeconds = (int)CurrentLevelPlayingTime;
            CurrentLevelPlayingTime = time;
            var newTimeInSeconds = (int)CurrentLevelPlayingTime;
            
            if (newTimeInSeconds > oldTimeInSeconds)
                CurrentLevelPlayingTimeChangedSignal.Dispatch(newTimeInSeconds);
        }
    }
}