using DG.Tweening;
using UnityEngine;
using Utilities.Disposable;

namespace UI.Popups.DailyRewardPopup.DailyRewardDayItem
{
    public class DailyRewardAlreadyReadyToCollectView : DailyRewardViewBase
    {
        [SerializeField] private ParticleSystem glowParticles;
        [SerializeField] private ParticleSystem dustParticles;
        
        [Header("Sun Rays Settings")]
        [SerializeField] private RectTransform sunRaysTransform;
        [SerializeField] private CanvasGroup sunRaysCanvasGroup;
        [SerializeField] private float rotationDuration = 5f; // Время одного полного оборота
        [SerializeField] private float fadeDuration = 0.4f;   // Длительность появления/затухания

        private Tween _rotationTween;
        
        public void PlayRaysAnimation()
        {
            if (sunRaysTransform == null || sunRaysCanvasGroup == null) return;
            
            KillAllTweens();
            sunRaysTransform.gameObject.SetActive(true);
            sunRaysCanvasGroup.alpha = 0f;
            sunRaysTransform.localScale = Vector3.zero;

            // 2. Красивое появление (прозрачность + масштаб)
            sunRaysCanvasGroup.DOFade(1f, fadeDuration).KillWith(this);
            sunRaysTransform.DOScale(Vector3.one, fadeDuration).SetEase(Ease.OutBack).KillWith(this);

            // 3. Запуск бесконечного вращения
            _rotationTween = sunRaysTransform
                .DORotate(new Vector3(0, 0, 360), rotationDuration, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear)
                .KillWith(this);
        }
        
        public void StopRayAnimation()
        {
            if (sunRaysCanvasGroup == null) return;

            // Плавно затухаем и уменьшаемся
            sunRaysCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad).KillWith(this);
            sunRaysTransform.DOScale(Vector3.zero, fadeDuration).SetEase(Ease.InBack).OnComplete(() => 
            {
                sunRaysTransform.gameObject.SetActive(false);
                KillAllTweens();
            }).KillWith(this);
        }

        public void PlayDust()
        {
            if (dustParticles != null)
                dustParticles.Play();
        }
        
        private void KillAllTweens()
        {
            _rotationTween?.Kill();
            sunRaysCanvasGroup?.DOKill();
            sunRaysTransform?.DOKill();
        }
        
        private void OnDestroy()
        {
            KillAllTweens();
        }
    }
}
