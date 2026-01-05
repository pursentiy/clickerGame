using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;

namespace Components.Levels.Figures
{
    public class FigureTarget : Figure
    {
        [SerializeField] protected RectTransform _rectTransform;
        [SerializeField] protected Image _fullImageSpriteRenderer;
        [SerializeField] protected Image _outlineImageSpriteRenderer;
        [SerializeField] protected ParticleSystem _completeParticles;
        
        public void SetUpFigure(bool isCompleted)
        {
            _outlineImageSpriteRenderer.DOFade(isCompleted ? 0 : 1, 0.01f).KillWith(this);
            _outlineImageSpriteRenderer.gameObject.SetActive(!isCompleted);
            
            _fullImageSpriteRenderer.DOFade(isCompleted ? 1 : 0, 0.01f).KillWith(this);
            _fullImageSpriteRenderer.gameObject.SetActive(isCompleted);
        }

        private void SetFigureCompletedAnimation()
        {
            _completeParticles.Simulate(0);
            _completeParticles.Play();
            
            // 1. Prepare the Full Image (The "Pop In")
            _fullImageSpriteRenderer.gameObject.SetActive(true);
            _fullImageSpriteRenderer.transform.localScale = Vector3.zero; // Start from nothing
            _fullImageSpriteRenderer.color = new Color(1, 1, 1, 0); // Start transparent

            // Sequence for the Full Image
            _fullImageSpriteRenderer.DOFade(1, 0.2f).SetUpdate(true);
            _fullImageSpriteRenderer.transform.DOScale(1.1f, 0.3f) // Overshoot slightly
                .SetEase(Ease.OutBack) // This gives the "bounce" effect
                .OnComplete(() => {
                    _fullImageSpriteRenderer.transform.DOScale(1.0f, 0.15f); // Settle to normal
                })
                .KillWith(this);

            // 2. Prepare the Outline (The "Pop Out/Dissolve")
            _outlineImageSpriteRenderer.transform.DOScale(1.3f, 0.3f) // Scale UP while fading out
                .SetEase(Ease.InQuad);
    
            _outlineImageSpriteRenderer.DOFade(0, 0.25f)
                .SetEase(Ease.InQuad)
                .OnComplete(() => _outlineImageSpriteRenderer.gameObject.SetActive(false))
                .KillWith(this);

            // 3. Optional: Add a subtle punch or shake to the camera or parent container
            // transform.DOPunchScale(new Vector3(0.05f, 0.05f, 0), 0.2f, 10, 1);
        }
        
        public void SetConnected()
        {
            SetFigureCompletedAnimation();
            SetFigureCompleted(true);
        }
    }
}