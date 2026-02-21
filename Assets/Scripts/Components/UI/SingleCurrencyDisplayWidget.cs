using Common.Currency;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Extensions;
using Handlers;
using Installers;
using RSG;
using Utilities.Disposable;
using Zenject;
using AudioExtensions = Extensions.AudioExtensions;

namespace Components.UI
{
    public class SingleCurrencyDisplayWidget : InjectableMonoBehaviour
    {
        [Inject] private readonly SoundHandler _soundHandler;
        
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _currencyText;
        [SerializeField] private RectTransform _animationTarget;
        [SerializeField] private ParticleSystem _confettiParticles;
        [SerializeField] private RectTransform _bumpTransform;

        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 0.5f;
        [SerializeField] private float _punchStrength = 0.2f;
        [SerializeField] private int _vibrato = 10;
        [SerializeField] private float _elasticity = 1f;
        [SerializeField] private string _numberFormat = "N0";

        private long _currentDisplayValue;
        private long _targetValue;
        private Tween _countTween;

        public RectTransform AnimationTarget => _animationTarget;

        public void SetValue(long newValue, bool withAnimation = false)
        {
            if (newValue == _targetValue)
                return;
            
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

        public void SetCurrency(ICurrency currency, bool withAnimation = false)
        {
            var value = currency?.GetCount() ?? 0;
            SetValue(value, withAnimation);
        }

        public IPromise Bump()
        {
            if (_bumpTransform == null)
                return Promise.Resolved();
            
            _bumpTransform.DOComplete();
            
            _soundHandler.PlaySound(AudioExtensions.BumpPanelKey);
            return _bumpTransform.DOPunchScale(Vector3.one * _punchStrength, _animationDuration, _vibrato, _elasticity)
                .KillWith(this)
                .AsPromise();
        }

        private void TriggerUpdateEffects()
        {
            if (_confettiParticles != null)
            {
                _confettiParticles.Stop();
                _confettiParticles.Play();
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
                _animationDuration
            ).SetEase(Ease.OutQuad).KillWith(this);
        }

        private void UpdateText(long value)
        {
            if (_currencyText != null)
            {
                _currencyText.text = value.ToString(_numberFormat);
            }
        }
    }
}
