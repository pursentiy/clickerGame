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
        
        public void SetUpFigure(bool isCompleted)
        {
            _outlineImageSpriteRenderer.DOFade(isCompleted ? 0 : 1, 0.01f).KillWith(this);
            _outlineImageSpriteRenderer.gameObject.SetActive(!isCompleted);
            
            _fullImageSpriteRenderer.DOFade(isCompleted ? 1 : 0, 0.01f).KillWith(this);
            _fullImageSpriteRenderer.gameObject.SetActive(isCompleted);
        }

        private void SetFigureCompletedAnimationSetColor()
        {
            _fullImageSpriteRenderer.gameObject.SetActive(true);
            _fullImageSpriteRenderer.DOFade(1, 0.3f).KillWith(this);

            _outlineImageSpriteRenderer.DOFade(0, 0.2f)
                .OnComplete(() => _outlineImageSpriteRenderer.gameObject.SetActive(false))
                .KillWith(this);
        }
        
        public void SetConnected()
        {
            SetFigureCompletedAnimationSetColor();
            SetFigureCompleted(true);
        }
    }
}