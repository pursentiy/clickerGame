using DG.Tweening;
using Extensions;
using RSG;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;

namespace UI.Screens.ChoosePack.Widgets
{
    public class AdsButtonWidget : MonoBehaviour
    {
        [SerializeField] Button _button;
        [SerializeField] RectTransform _container;
        [SerializeField] RectTransform _animationContainer;
        
        [Header("Animation Settings")]
        [SerializeField] private float _bumpDuration = 0.5f;
        
        public Button Button => _button;
        public RectTransform RectTransform => _container;

        public IPromise BumpButton()
        {
            _animationContainer.DOComplete();
    
            var sequence = DOTween.Sequence().KillWith(this);
    
            sequence.Append(_animationContainer.DOLocalJump(Vector3.zero, 25f, 1, _bumpDuration).SetEase(Ease.OutQuad));
            sequence.Join(_animationContainer.DOPunchRotation(new Vector3(0, 0, 12f), _bumpDuration, 8, 1f));
            sequence.Join(_animationContainer.DOPunchScale(new Vector3(0.2f, -0.1f, 0), _bumpDuration, 5, 0.5f));

            return sequence.AsPromise();
        }
    }
}