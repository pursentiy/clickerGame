using Extensions;
using Installers;
using TMPro;
using UnityEngine;

namespace UI.Screens.PuzzleAssembly.Widgets
{
    public class LevelTimerWidget : InjectableMonoBehaviour
    {
        [SerializeField] private TMP_Text _timerText;

        public void UpdateTime(double seconds)
        {
            _timerText.text = DateTimeExtensions.ToStopwatchTime(seconds);
        }
    }
}