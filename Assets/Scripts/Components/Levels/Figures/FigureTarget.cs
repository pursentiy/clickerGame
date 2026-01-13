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
            // Полная очистка предыдущих состояний
            _fullImageSpriteRenderer.rectTransform.DOKill();
            _outlineImageSpriteRenderer.rectTransform.DOKill();
            _fullImageSpriteRenderer.DOKill();
            _outlineImageSpriteRenderer.DOKill();

            _completeParticles.Stop();
            _completeParticles.Play();

            // 1. Подготовка: никакого нулевого масштаба. 
            // Начнем с 0.95, чтобы движение было едва заметным, но приятным.
            _fullImageSpriteRenderer.gameObject.SetActive(true);
            _fullImageSpriteRenderer.color = new Color(1, 1, 1, 0);
            _fullImageSpriteRenderer.rectTransform.localScale = Vector3.one * 0.95f;

            Sequence s = DOTween.Sequence().SetUpdate(true).KillWith(this);

            // 2. Плавное проявление (Fade) — чуть дольше, чтобы глаз успел заметить
            s.Join(_fullImageSpriteRenderer.DOFade(1f, 0.4f).SetEase(Ease.OutCubic));
    
            // 3. Плавное расширение до 1.0 (без отскоков Back)
            s.Join(_fullImageSpriteRenderer.rectTransform.DOScale(1f, 0.5f).SetEase(Ease.OutQuint));

            // 4. Контур: он должен просто мягко растаять, не мешая основной картинке
            s.Join(_outlineImageSpriteRenderer.DOFade(0f, 0.3f).SetEase(Ease.Linear));
            // Контур чуть-чуть увеличим, создавая эффект "растворения в воздухе"
            s.Join(_outlineImageSpriteRenderer.rectTransform.DOScale(1.05f, 0.4f).SetEase(Ease.OutQuad));

            s.OnComplete(() =>
            {
                _outlineImageSpriteRenderer.gameObject.SetActive(false);
                // Вместо резкого Punch используем очень мягкий затухающий импульс
                _fullImageSpriteRenderer.rectTransform.DOPunchScale(new Vector3(0.02f, 0.02f, 0), 0.3f, 2, 0.5f);
            });
        }
        
        public void SetConnected()
        {
            SetFigureCompletedAnimation();
            SetFigureCompleted(true);
        }
    }
}