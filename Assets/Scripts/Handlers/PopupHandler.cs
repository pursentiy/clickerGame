using DG.Tweening;
using Installers;
using Popup;
using RSG;
using UnityEngine;

namespace Handlers
{
    public class PopupHandler : InjectableMonoBehaviour, IPopupHandler
    {
        [SerializeField] private RectTransform _popupCanvasTransform;
        [SerializeField] private SettingsPopup _settingsPopup;
        
        private PopupBase _currentPopupBase;
        
        public void ShowSettings()
        {
            TryHideOtherPopup().Then(() =>
            {
                _currentPopupBase = Instantiate(_settingsPopup, _popupCanvasTransform);
                ShowPopup();
            });
        }

        public IPromise TryHideOtherPopup()
        {
            return _currentPopupBase == null ? Promise.Resolved() : HideCurrentPopup();
        }
        
        private IPromise ShowPopup()
        {
            
            var showPromise = new Promise();
            var currentAlpha = _currentPopupBase.PopupBackgroundImage.color.a;
            DOTween.Sequence().Append(_currentPopupBase.PopupBaseTransform.DOScale(new Vector3(0, 0, 0), 0.01f))
                .Append(_currentPopupBase.PopupBackgroundImage.DOFade(0, 0.01f))
                .AppendCallback(() =>
                {
                    _currentPopupBase.PopupOriginTransform.gameObject.SetActive(true);
                })
                .Append(_currentPopupBase.PopupBaseTransform.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(_currentPopupBase.PopupShowAnimationCurve))
                .Join(_currentPopupBase.PopupBackgroundImage.DOFade(currentAlpha, 0.45f))
                .OnComplete(() => { showPromise.Resolve(); });

            return showPromise;
        }

        public IPromise HideCurrentPopup()
        {
            var hidePromise = new Promise();
            DOTween.Sequence()
                .Append(_currentPopupBase.PopupBaseTransform.DOScale(new Vector3(0f, 0f, 0f), 0.4f).SetEase(_currentPopupBase.PopupHideAnimationCurve))
                .Join(_currentPopupBase.PopupBackgroundImage.DOFade(0, 0.3f)).OnComplete(() =>
                {
                    Destroy(_currentPopupBase.PopupOriginTransform.gameObject);
                    _currentPopupBase = null;
                    hidePromise.Resolve();
                });

            return hidePromise;
        }
    }
}