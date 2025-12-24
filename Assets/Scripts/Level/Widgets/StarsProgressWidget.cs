using DG.Tweening;
using Installers;
using Services;
using Storage.Levels.Params;
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
        private float _maxTime;

        public void Initialize(LevelBeatingTimeInfo levelBeatingTime)
        {
            if (starImages.Length != 3 || levelBeatingTime == null)
            {
                LoggerService.LogError("StarWidget: Ensure you have exactly 3 images and 3 time values.");
                return;
            }
            
            _timeThresholds = new [] {levelBeatingTime.FastestTime, levelBeatingTime.MediumTime, levelBeatingTime.MinimumTime};
            _isStarActive = new [] { true, true, true };
            _maxTime =  levelBeatingTime.MinimumTime;
            
            // Reset visual state safely
            foreach (var img in starImages)
            {
                // Kill any old tweens running on this object to prevent conflicts
                img.DOKill();
                img.transform.DOKill();

                img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
                img.transform.localScale = Vector3.one;
            }
        }

        public void OnTimeUpdate(float currentTime)
        {
            if (_isStarActive == null) return;

            // Check from hardest star (3) down to easiest (1)
            for (var i = 0; i < starImages.Length; i++)
            {
                if (_isStarActive[i] && currentTime > _timeThresholds[i])
                {
                    LoseStar(i);
                }
            }
        }

        private void LoseStar(int index)
        {
            _isStarActive[index] = false;

            // 1. Fade out the lost star
            // DOFade handles the alpha interpolation automatically
            starImages[index].DOFade(0f, _fadeDuration);

            // 2. Bump remaining active stars
            for (int i = 0; i < _isStarActive.Length; i++)
            {
                if (_isStarActive[i])
                {
                    BumpStar(starImages[i].transform);
                }
            }
        }

        private void BumpStar(Transform target)
        {
            // Stop any current scaling to prevent "double scaling" glitches
            target.DOKill(complete: true);
            // Ensure scale is reset to 1 before starting (in case the kill happened mid-animation)
            target.localScale = Vector3.one;

            // Scale up to bumpAmount, then go back to 1 (Yoyo)
            target.DOScale(_bumpScaleAmount, _bumpDuration / 2)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutQuad);
        }

        // Cleanup if the widget is destroyed while animating
        private void OnDestroy()
        {
            foreach (var img in starImages)
            {
                if (img != null)
                {
                    img.DOKill();
                    img.transform.DOKill();
                }
            }
        }
    }
}