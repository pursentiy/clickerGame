using DG.Tweening;
using UnityEngine;
using Utilities.Disposable;

namespace UI.Screens.WelcomeScreen.Widgets
{
    public class WelcomeScreenAnimationWidget : MonoBehaviour
    {
        [SerializeField] private RectTransform _headerRect;
        [SerializeField] private CanvasGroup _headerCanvasGroup;
        
        [Header("Animation Settings")]
        [SerializeField] private float _duration = 0.8f;
        [SerializeField] private float _startScale = 0.5f;
        [SerializeField] private float _flyOffset = 50f;

        private Vector2 _originalAnchorPos;
        private bool _isInitialized;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            
            _originalAnchorPos = _headerRect.anchoredPosition;
            _isInitialized = true;
        }

        public void ShowAnimation()
        {
            EnsureInitialized();
            
            PrepareHeaderForAnimation();
            AnimateHeader();
        }

        private void PrepareHeaderForAnimation()
        {
            _headerRect.DOKill();
            _headerCanvasGroup?.DOKill();

            _headerRect.localScale = Vector3.one * _startScale;
            _headerRect.anchoredPosition = _originalAnchorPos + new Vector2(0, -_flyOffset);
            
            if (_headerCanvasGroup != null) 
                _headerCanvasGroup.alpha = 0;
        }
        
        private void AnimateHeader()
        {
            _headerRect.DOAnchorPos(_originalAnchorPos, _duration)
                .SetEase(Ease.OutQuart)
                .KillWith(this);

            _headerRect.DOScale(1f, _duration)
                .SetEase(Ease.OutBack)
                .KillWith(this);

            if (_headerCanvasGroup != null)
            {
                _headerCanvasGroup.DOFade(1f, _duration * 0.5f)
                    .SetEase(Ease.Linear)
                    .KillWith(this);
            }
        }
    }
}