using System;
using Common.Currency;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Extensions;
using Installers;
using RSG;
using Services.Player;
using Utilities.Disposable;
using Zenject;

namespace Components.UI
{
    public class CurrencyDisplayWidget : InjectableMonoBehaviour
    {
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        
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

        private long _currentDisplayValue;
        private long _targetValue;
        private Tween _countTween;

        public RectTransform AnimationTarget => animationTarget;

        public void Start()
        {
            _playerCurrencyService.StarsChangedSignal.MapListener(OnCurrencyChanged).DisposeWith(this);
        }

        private void OnCurrencyChanged(ICurrency newValue, CurrencyChangeMode mode)
        {
            if (mode != CurrencyChangeMode.Instant)
                return;
            
            SetCurrency(newValue.GetCount(), false);
        }

        public void SetCurrency(long newValue, bool withAnimation = false)
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

        public IPromise Bump()
        {
            bumpTransform.DOComplete();
            
            return bumpTransform.DOPunchScale(Vector3.one * punchStrength, animationDuration, vibrato, elasticity)
                .KillWith(this)
                .AsPromise();
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

        private void UpdateText(long value)
        {
            if (currencyText != null)
            {
                currencyText.text = value.ToString(numberFormat);
            }
        }
    }
}