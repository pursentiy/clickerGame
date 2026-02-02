using DG.Tweening;
using Installers;
using Services;
using Storage.Snapshots.LevelParams;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens.PuzzleAssembly.Widgets
{
    public class StarsProgressWidget : InjectableMonoBehaviour
    {
        [Header("UI References")] 
        [SerializeField] private Image[] starImages;
        [SerializeField] private Image[] grayStarImages;

        [Header("Settings")] 
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private float _bumpDuration = 0.2f;
        [SerializeField] private float _bumpScaleAmount = 1.2f;

        private float[] _timeThresholds;
        private bool[] _isStarActive;

        public void Initialize(LevelBeatingTimeInfoSnapshot levelBeatingTime)
        {
            if (starImages.Length != 3 || grayStarImages.Length != 3 || levelBeatingTime == null)
            {
                LoggerService.LogError("StarWidget: Ensure you have exactly 3 stars and 3 gray stars.");
                return;
            }
            
            _timeThresholds = new [] {levelBeatingTime.FastestTime, levelBeatingTime.MediumTime, levelBeatingTime.MinimumTime};
            SetupStarsInitialState();
        }
        
        public void ResetWidget()
        {
            ResetStarsAnimations(true);
            SetupStarsInitialState();
        }

        public void OnTimeUpdate(double currentTime)
        {
            if (_isStarActive == null) 
                return;

            for (var i = 0; i < starImages.Length; i++)
            {
                // Если звезда активна, но время превысило порог — "теряем" её
                if (_isStarActive[i] && currentTime > _timeThresholds[i])
                {
                    LoseStar(i);
                }
            }
        }
        
        private void SetupStarsInitialState()
        {
            _isStarActive = new [] { true, true, true };
            
            for (var i = 0; i < 3; i++)
            {
                // Сброс активных звезд (видимые)
                starImages[i].DOKill();
                starImages[i].transform.DOKill();
                starImages[i].color = SetAlpha(starImages[i].color, 1f);
                starImages[i].transform.localScale = Vector3.one;

                // Сброс серых звезд (невидимые в начале)
                grayStarImages[i].DOKill();
                grayStarImages[i].transform.DOKill();
                grayStarImages[i].color = SetAlpha(grayStarImages[i].color, 0f);
                grayStarImages[i].transform.localScale = Vector3.one;
            }
        }

        private void LoseStar(int index)
        {
            _isStarActive[index] = false;

            // 1. Исчезновение цветной звезды
            starImages[index].DOFade(0f, _fadeDuration);

            // 2. Появление серой звезды с анимацией
            grayStarImages[index].transform.localScale = Vector3.zero;
            grayStarImages[index].DOFade(1f, _fadeDuration);
            grayStarImages[index].transform.DOScale(1f, _fadeDuration).SetEase(Ease.OutBack);

            // 3. "Бамп" (пульсация) всех оставшихся активных звезд
            for (var i = 0; i < _isStarActive.Length; i++)
            {
                if (_isStarActive[i])
                {
                    BumpStar(starImages[i].transform);
                }
            }
        }

        private void BumpStar(Transform target)
        {
            target.DOKill(complete: true);
            target.localScale = Vector3.one;
            
            target.DOScale(_bumpScaleAmount, _bumpDuration / 2)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutQuad);
        }
        
        private void OnDestroy()
        {
            ResetStarsAnimations();
        }

        private void ResetStarsAnimations(bool completeAnimation = false)
        {
            foreach (var img in starImages)
            {
                if (img == null) continue;
                img.DOKill(completeAnimation);
                img.transform.DOKill(completeAnimation);
            }

            foreach (var img in grayStarImages)
            {
                if (img == null) continue;
                img.DOKill(completeAnimation);
                img.transform.DOKill(completeAnimation);
            }
        }

        private Color SetAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}