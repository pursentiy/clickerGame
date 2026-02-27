using Components.UI;
using DG.Tweening;
using Extensions;
using Services.DailyReward;
using UI.Screens.WelcomeScreen.DailyRewardsState;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.WelcomeScreen.Widgets
{
    public class DailyRewardsButton : ButtonWithTimerBase
    {
        [Inject] private readonly DailyRewardsInfoProvider _infoProvider;

        [Header("References")]
        [SerializeField] private Button button;
        [SerializeField] private RectTransform buttonTransform;
        [SerializeField] private ParticleSystem glowParticles;

        [Header("Animation Settings")]
        [SerializeField] private float pulseScale = 1.08f;
        [SerializeField] private float duration = 1.2f;

        private Sequence _activeSequence;

        public void Initialize()
        {
            button?.onClick.MapListenerWithSound(OnButtonClicked).DisposeWith(this);
            
            Refresh();
        }

        protected void OnEnable() => Refresh();

        private void Refresh()
        {
            StopTimer();
            StopAnimations();

            var status = _infoProvider.GetRewardStatus();

            if (status.IsAvailable)
            {
                if (TimerText != null) TimerText.text = AvailableText;
                StartAnimations();
            }
            else if (status.TimeUntilNext.TotalSeconds > 0)
            {
                StartTimer(status.TimeUntilNext, Refresh);
            }
        }

        private void OnButtonClicked()
        {
            StateMachine.CreateMachine(null)
                .StartSequence<TryShowDailyRewardsPopupState>()
                .FinishWith(this);
        }

        private void StartAnimations()
        {
            if (buttonTransform == null) return;
    
            glowParticles?.Play();
    
            // 1. Полная очистка и сброс перед стартом
            buttonTransform.DOKill();
            buttonTransform.localScale = Vector3.one;
            buttonTransform.localRotation = Quaternion.identity;
            buttonTransform.anchoredPosition = Vector3.zero; // Сбрасываем позицию!

            _activeSequence = DOTween.Sequence();

            _activeSequence
                // === ОСНОВНОЙ ЦИКЛ ПУЛЬСАЦИИ ===
                // Очень медленно увеличиваем (Ease.InOutSine гарантирует плавный старт и стоп)
                .Append(buttonTransform.DOScale(pulseScale, duration * 0.7f).SetEase(Ease.InOutSine))
                // Так же медленно возвращаем в норму
                .Append(buttonTransform.DOScale(1f, duration * 0.7f).SetEase(Ease.InOutSine))
        
                // === ПАРАЛЛЕЛЬНОЕ ПАРИШЕЕ ДВИЖЕНИЕ ===
                // Это самое важное для "спокойствия". Мы Join (объединяем) плавные качания.
        
                // Покачивание ВВЕРХ-ВНИЗ (Floating)
                .Join(buttonTransform.DOAnchorPosY(5f, duration * 1.4f).SetEase(Ease.InOutSine).SetLoops(2, LoopType.Yoyo))
        
                // Очень легкий наклон влево-вправо (как маятник, но очень медленно)
                // new Vector3(0, 0, 3f) - 3 градуса, это почти незаметно, но глаз это ловит.
                .Join(buttonTransform.DORotate(new Vector3(0, 0, 3f), duration * 1.4f).SetEase(Ease.InOutQuad).SetLoops(2, LoopType.Yoyo))
        
                // Зацикливаем бесконечно
                .SetLoops(-1)
                .SetId(this)
                .KillWith(this);
        }

        private void StopAnimations()
        {
            _activeSequence?.Kill();
            glowParticles?.Stop();
            
            if (buttonTransform != null)
            {
                buttonTransform.DOKill();
                buttonTransform.localScale = Vector3.one;
                buttonTransform.localEulerAngles = Vector3.zero;
            }
        }

        protected override void OnDisable() 
        {
            base.OnDisable();
            
            StopAnimations();
        }
    }
}