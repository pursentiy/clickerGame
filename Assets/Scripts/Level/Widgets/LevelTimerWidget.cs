using Extensions;
using Installers;
using TMPro;
using UnityEngine;

namespace Level.Widgets
{
    public class LevelTimerWidget : InjectableMonoBehaviour
    {
        [SerializeField] private TMP_Text _timerText;

        public void UpdateTime(float seconds)
        {
            _timerText.text = DateTimeExtensions.ToClockTime(seconds);
        }
    }
}