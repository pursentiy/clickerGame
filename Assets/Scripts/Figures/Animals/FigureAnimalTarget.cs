using DG.Tweening;
using UnityEngine;

namespace Figures.Animals
{
    public class FigureAnimalTarget : Figure, IFigureAnimalTarget
    {
        [SerializeField] protected Transform _transform;

        private SpriteRenderer _spriteRenderer;
        
        public void SetUpFigure(Sprite sprite, Color color, float scale, Vector3 position)
        {
            var spriteRender = GetComponent<SpriteRenderer>();
            if (spriteRender == null)
            {
                Debug.LogWarning($"No {nameof(SpriteRenderer)} found on object with type {FigureType}");
                return;
            }
            
            SetupSprite(sprite, color, spriteRender);
            SetupTransform(scale, position);
            SetupPolygonCollider();
        }

        private void SetupSprite(Sprite sprite, Color color, SpriteRenderer spriteRender)
        {
            _spriteRenderer = spriteRender;
            _spriteRenderer.sprite = sprite;
            _spriteRenderer.color = color;
        }

        private void SetupTransform(float scale, Vector3 position)
        {
            _transform = gameObject.transform;
            _transform.localScale = new Vector3(scale, scale, 0);
            _transform.position = position;
        }

        private void SetupPolygonCollider()
        {
            var polygon = gameObject.AddComponent(typeof(PolygonCollider2D)) as PolygonCollider2D;
            polygon?.GetShapeHash();
        }

        private void SetFigureColor()
        {
            if (_spriteRenderer == null)
            {
                Debug.LogWarning($"Sprite Renderer is missing in {this}");
                return;
            }

            _spriteRenderer.DOColor(FigureColor, 0.3f);
        }
        
        public void SetConnected()
        {
            SetFigureColor();
            SetFigureCompleted(true);
        }
    }
}