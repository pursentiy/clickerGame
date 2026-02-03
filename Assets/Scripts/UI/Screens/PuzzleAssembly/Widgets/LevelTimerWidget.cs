using Extensions;
using Installers;
using Level.Widgets;
using TMPro;
using UnityEngine;
using Utilities.Disposable;
using Zenject;

namespace UI.Screens.PuzzleAssembly.Widgets
{
    public class LevelTimerWidget : InjectableMonoBehaviour
    {
        [Inject] private readonly LevelInfoTrackerService _levelInfoTrackerService;
        
        [SerializeField] private TMP_Text _timerText;

        public void Initialize()
        {
            _levelInfoTrackerService.CurrentLevelPlayingTimeChangedSignal.MapListener(OnTimeUpdate).DisposeWith(this);
        }

        private void OnTimeUpdate(double seconds)
        {
            _timerText.text = DateTimeExtensions.ToStopwatchTime(seconds);
        }
    }
}