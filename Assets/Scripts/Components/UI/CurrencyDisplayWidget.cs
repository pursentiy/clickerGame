using UnityEngine;
using TMPro;
using DG.Tweening;
using Utilities.Disposable;

namespace Components.UI
{
    public class CurrencyDisplayWidget : MonoBehaviour
    {
        [Header("UI References")] [SerializeField]
        private TextMeshProUGUI currencyText;

        [SerializeField] private RectTransform animationTarget;
        [SerializeField] private ParticleSystem confettiParticles;
        [SerializeField] private RectTransform bumpTransform;

        [Header("Animation Settings")] [SerializeField]
        private float animationDuration = 0.5f;

        [SerializeField] private float punchStrength = 0.2f; // 0.2 means +20% scale at peak
        [SerializeField] private int vibrato = 10; // How much it vibrates/shakes
        [SerializeField] private float elasticity = 1f; // Bounciness
        [SerializeField] private string numberFormat = "N0";

        private int _currentDisplayValue;
        private int _targetValue;
        private Tween _countTween;

        public RectTransform AnimationTarget => animationTarget;

        public void SetCurrency(int newValue, bool withAnimation = false)
        {
            _targetValue = newValue;

            if (withAnimation)
            {
                TriggerUpdateEffects();
            }
            else
            {
                _currentDisplayValue = newValue;
                UpdateText(_targetValue);
            }
        }

        public void Bump()
        {
            bumpTransform.DOComplete();
            bumpTransform.DOPunchScale(Vector3.one * punchStrength, animationDuration, vibrato, elasticity).KillWith(this);
        }
        
        // DEBUG
        [ContextMenu("Test Add 100")]
        public void TestAdd100() => SetCurrency(100, true);

        private void TriggerUpdateEffects()
        {
            // 1. Particle Burst
            if (confettiParticles != null)
            {
                confettiParticles.Stop();
                confettiParticles.Play();
            }

            Bump(); 

            _countTween?.Kill();
            _countTween = DOTween.To(
                () => _currentDisplayValue,
                x =>
                {
                    _currentDisplayValue = x;
                    UpdateText(_currentDisplayValue);
                },
                _targetValue,
                animationDuration
            ).SetEase(Ease.OutQuad).KillWith(this);
        }

        private void UpdateText(int value)
        {
            if (currencyText != null)
            {
                currencyText.text = value.ToString(numberFormat);
            }
        }
    }
}