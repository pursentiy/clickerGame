using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Components.UI
{
    public class CurrencyDisplayWidget : MonoBehaviour
    {
        [Header("UI References")] [SerializeField]
        private TextMeshProUGUI currencyText;

        [SerializeField] private ParticleSystem confettiParticles;
        [SerializeField] private RectTransform bumpTransform;

        [Header("Animation Settings")] [SerializeField]
        private float animationDuration = 0.5f;

        [SerializeField] private float punchStrength = 0.2f; // 0.2 means +20% scale at peak
        [SerializeField] private int vibrato = 10; // How much it vibrates/shakes
        [SerializeField] private float elasticity = 1f; // Bounciness

        [Header("Data")] [SerializeField] private int startingValue = 0;
        [SerializeField] private string numberFormat = "N0";

        private int _currentDisplayValue;
        private int _targetValue;
        private Tween _countTween;

        public void AddCurrency(int amount)
        {
            _targetValue += amount;
            TriggerUpdateEffects();
        }

        public void SetCurrency(int newValue)
        {
            _targetValue = newValue;
            TriggerUpdateEffects();
        }
        
        // DEBUG
        [ContextMenu("Test Add 100")]
        public void TestAdd100() => AddCurrency(100);

        private void TriggerUpdateEffects()
        {
            // 1. Particle Burst
            if (confettiParticles != null)
            {
                confettiParticles.Stop();
                confettiParticles.Play();
            }

            // 2. DOTween Bump (PunchScale)
            // Complete() ensures if it's already bumping, it resets first so it doesn't grow infinitely
            bumpTransform.DOComplete();
            bumpTransform.DOPunchScale(Vector3.one * punchStrength, animationDuration, vibrato, elasticity);

            // 3. DOTween Number Counter
            // We kill the previous count tween so we don't have two tweens fighting over the variable
            _countTween?.Kill();

            _countTween = DOTween.To(
                () => _currentDisplayValue, // Getter
                x =>
                {
                    // Setter
                    _currentDisplayValue = x;
                    UpdateText(_currentDisplayValue);
                },
                _targetValue, // Target
                animationDuration // Duration
            ).SetEase(Ease.OutQuad); // Optional: Add easing for nicer counting
        }

        private void UpdateText(int value)
        {
            if (currencyText != null)
            {
                currencyText.text = value.ToString(numberFormat);
            }
        }
        
        private void OnDestroy()
        {
            bumpTransform.DOKill();
            _countTween?.Kill();
        }
    }
}