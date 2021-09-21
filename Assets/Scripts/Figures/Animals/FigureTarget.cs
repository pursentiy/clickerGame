using DG.Tweening;
using UnityEngine;

namespace Figures.Animals
{
    public class FigureTarget : Figure, IFigureTarget
    {
        [SerializeField] protected Transform _transform;
        [SerializeField] protected SpriteRenderer _fullImageSpriteRenderer;
        [SerializeField] protected SpriteRenderer _outlineImageSpriteRenderer;

        private SpriteRenderer _spriteRenderer;
        
        public void SetUpFigure(bool isCompleted)
        {
            _outlineImageSpriteRenderer.DOFade(isCompleted ? 0 : 1, 0.01f);
            _outlineImageSpriteRenderer.gameObject.SetActive(!isCompleted);
            
            _fullImageSpriteRenderer.DOFade(isCompleted ? 1 : 0, 0.01f);
            _fullImageSpriteRenderer.gameObject.SetActive(isCompleted);
        }

        private void SetFigureCompletedAnimationSetColor()
        {
            _fullImageSpriteRenderer.gameObject.SetActive(true);
            _fullImageSpriteRenderer.DOFade(1, 0.3f);

            _outlineImageSpriteRenderer.DOFade(0, 0.2f).OnComplete(() =>
            {
                _outlineImageSpriteRenderer.gameObject.SetActive(false);
            });
        }
        
        public void SetConnected()
        {
            SetFigureCompletedAnimationSetColor();
            SetFigureCompleted(true);
        }
    }
}