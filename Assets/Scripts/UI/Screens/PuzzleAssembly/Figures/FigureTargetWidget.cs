using DG.Tweening;
using Extensions;
using Installers;
using RSG;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;

namespace UI.Screens.PuzzleAssembly.Figures
{
    public class FigureTargetWidget : InjectableMonoBehaviour, IFigureBase
    {
        [SerializeField] protected Image _fullImageSpriteRenderer;
        [SerializeField] protected Image _outlineImageSpriteRenderer;
        [SerializeField] protected ParticleSystem _completeParticles;
        
        public bool IsCompleted { get; private set; }
        public int Id { get; private set; }
        
        public void Initialize(int id)
        {
            Id = id;
        }

        public void SetFigureCompleted(bool value)
        {
            IsCompleted = value;
        }
        
        public void SetUpFigure(bool isCompleted)
        {
            _outlineImageSpriteRenderer.DOFade(isCompleted ? 0 : 1, 0.01f).KillWith(this);
            _outlineImageSpriteRenderer.gameObject.SetActive(!isCompleted);
            
            _fullImageSpriteRenderer.DOFade(isCompleted ? 1 : 0, 0.01f).KillWith(this);
            _fullImageSpriteRenderer.gameObject.SetActive(isCompleted);
        }
        
        public IPromise SetConnected()
        {
            return SetFigureCompletedAnimation()
                .Then(() =>
                {
                    SetFigureCompleted(true);
                    return Promise.Resolved();
                })
                .CancelWith(this);
        }

        private IPromise SetFigureCompletedAnimation()
        {
            _fullImageSpriteRenderer.rectTransform.DOKill();
            _outlineImageSpriteRenderer.rectTransform.DOKill();
            _fullImageSpriteRenderer.DOKill();
            _outlineImageSpriteRenderer.DOKill();

            _completeParticles.Stop();
            _completeParticles.Play();
            
            _fullImageSpriteRenderer.gameObject.SetActive(true);
            _fullImageSpriteRenderer.color = new Color(1, 1, 1, 0);
            _fullImageSpriteRenderer.rectTransform.localScale = Vector3.one * 0.55f;

            var s = DOTween.Sequence().KillWith(this);
            s.Join(_fullImageSpriteRenderer.DOFade(1f, 0.5f).SetEase(Ease.OutCubic));
            s.Join(_fullImageSpriteRenderer.rectTransform.DOScale(1f, 0.6f).SetEase(Ease.OutQuint));
            s.Join(_outlineImageSpriteRenderer.DOFade(0f, 0.4f).SetEase(Ease.Linear));
            s.Join(_outlineImageSpriteRenderer.rectTransform.DOScale(1.05f, 0.5f).SetEase(Ease.OutQuad));

            s.OnComplete(() =>
            {
                _outlineImageSpriteRenderer.gameObject.SetActive(false);
                _fullImageSpriteRenderer.rectTransform.DOPunchScale(new Vector3(0.02f, 0.02f, 0), 0.3f, 2, 0.6f).KillWith(this);
            });

            return s.AsPromise();
        }
    }
}