using Plugins.FSignal;
using Services;
using UnityEngine;
using Zenject;

namespace Level.Widgets
{
    public class LevelInfoTrackerService
    {
        [Inject] private TimeService _timeService;
        
        public FSignal<double> CurrentLevelPlayingTimeChangedSignal = new ();
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

        private void OnTimerUpdate(double time)
        {
            var roundedTime = (float)System.Math.Round(time, 2);

            if (!(Mathf.Abs(CurrentLevelPlayingTime - roundedTime) >= 0.01f)) 
                return;
            
            CurrentLevelPlayingTime = roundedTime;
            CurrentLevelPlayingTimeChangedSignal.Dispatch(CurrentLevelPlayingTime);
        }
    }
}