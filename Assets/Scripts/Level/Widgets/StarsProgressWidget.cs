using DG.Tweening;
using Installers;
using Services;
using Storage.Snapshots.LevelParams;
using UnityEngine;
using UnityEngine.UI;

namespace Level.Widgets
{
    public class StarsProgressWidget : InjectableMonoBehaviour
    {
        [Header("UI References")] [Tooltip("Order: Star 1, Star 2, Star 3")] [SerializeField]
        private Image[] starImages;

        [Header("Settings")] [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private float _bumpDuration = 0.2f;
        [SerializeField] private float _bumpScaleAmount = 1.2f;

        private float[] _timeThresholds;
        private bool[] _isStarActive;

        public void Initialize(LevelBeatingTimeInfoSnapshot levelBeatingTime)
        {
            if (starImages.Length != 3 || levelBeatingTime == null)
            {
                LoggerService.LogError("StarWidget: Ensure you have exactly 3 images and 3 time values.");
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

        public void OnTimeUpdate(float currentTime)
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
            _isStarActive = new [] { true, true, true };
            
            foreach (var img in starImages)
            {
                img.DOKill();
                img.transform.DOKill();

                img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
                img.transform.localScale = Vector3.one;
            }
        }

        private void LoseStar(int index)
        {
            _isStarActive[index] = false;
            starImages[index].DOFade(0f, _fadeDuration);

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
                if (img == null) 
                    continue;
                
                img.DOKill(completeAnimation);
                img.transform.DOKill(completeAnimation);
            }
        }
    }
}