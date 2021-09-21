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
            _outlineImageSpriteRenderer.gameObject.SetActive(!isCompleted);
            _fullImageSpriteRenderer.gameObject.SetActive(isCompleted);
        }

        private void SetFigureCompletedAnimationSetColor()
        {
            _fullImageSpriteRenderer.DOFade(1, 0.3f);
        }
        
        public void SetConnected()
        {
            SetFigureCompletedAnimationSetColor();
            SetFigureCompleted(true);
        }
    }
}