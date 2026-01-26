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
        [SerializeField] RectTransform _bumpAnimationContainer;
        [SerializeField] RectTransform _floatingAnimationContainer;
        
        [Header("Animation Settings")]
        [SerializeField] private float _bumpDuration = 0.5f;
        
        [Header("Floating Settings")]
        [SerializeField] private float _floatAmplitude = 15f;
        [SerializeField] private float _floatDuration = 2f;
        [SerializeField] private float _rotateAngle = 3f;

        private Sequence _floatingSequence;
        
        public Button Button => _button;
        public RectTransform RectTransform => _container;

        private void Start()
        {
            StartFloating();
        }

        public IPromise BumpButton()
        {
            _bumpAnimationContainer.DOComplete();
    
            var sequence = DOTween.Sequence().KillWith(this);
    
            sequence.Append(_bumpAnimationContainer.DOLocalJump(Vector3.zero, 25f, 1, _bumpDuration).SetEase(Ease.OutQuad));
            sequence.Join(_bumpAnimationContainer.DOPunchRotation(new Vector3(0, 0, 12f), _bumpDuration, 8, 1f));
            sequence.Join(_bumpAnimationContainer.DOPunchScale(new Vector3(0.2f, -0.1f, 0), _bumpDuration, 5, 0.5f));

            return sequence.AsPromise();
        }
        
        private void StartFloating()
        {
            _floatingSequence?.Kill();
            
            _floatingSequence = DOTween.Sequence()
                .SetUpdate(true) // Чтобы работало даже при паузе, если нужно
                .SetLoops(-1, LoopType.Yoyo) // Бесконечно туда-обратно
                .KillWith(this);
            
            _floatingSequence.Append(_floatingAnimationContainer.DOAnchorPosY(_floatAmplitude, _floatDuration)
                .SetEase(Ease.InOutSine));
            
            _floatingSequence.Join(_floatingAnimationContainer.DORotate(new Vector3(0, 0, _rotateAngle), _floatDuration)
                .SetEase(Ease.InOutSine));
        }
        
        private void OnDestroy()
        {
            _floatingSequence?.Kill();
        }
    }
}