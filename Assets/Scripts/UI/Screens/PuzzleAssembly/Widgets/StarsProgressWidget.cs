using DG.Tweening;
using Extensions;
using Installers;
using Level.Widgets;
using Services;
using Storage.Snapshots.LevelParams;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace UI.Screens.PuzzleAssembly.Widgets
{
    public class StarsProgressWidget : InjectableMonoBehaviour
    {
        [Inject] private readonly LevelInfoTrackerService _levelInfoTrackerService;
        
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
            
            _levelInfoTrackerService.CurrentLevelPlayingTimeChangedSignal.MapListener(OnTimeUpdate).DisposeWith(this);
        }

        private void OnTimeUpdate(double currentTime)
        {
            if (_isStarActive == null) 
                return;

            for (var i = 0; i < starImages.Length; i++)
            {
                if (_isStarActive[i] && currentTime > _timeThresholds[i])
                {
                    LoseStar(i);
                }
            }
        }
        
        private void SetupStarsInitialState()
        {
            ResetStarsAnimations();
            
            _isStarActive = new [] { true, true, true };
            
            for (var i = 0; i < 3; i++)
            {
                starImages[i].color.SetAlpha(1f);
                starImages[i].transform.localScale = Vector3.one;

                grayStarImages[i].color.SetAlpha( 0f);
                grayStarImages[i].transform.localScale = Vector3.one;
            }
        }

        private void LoseStar(int index)
        {
            _isStarActive[index] = false;
            
            starImages[index].DOFade(0f, _fadeDuration).KillWith(this);
            grayStarImages[index].transform.localScale = Vector3.zero;
            grayStarImages[index].DOFade(1f, _fadeDuration);
            grayStarImages[index].transform.DOScale(1f, _fadeDuration).SetEase(Ease.OutBack).KillWith(this);
            
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
                .SetEase(Ease.OutQuad)
                .KillWith(this);
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
    }
}