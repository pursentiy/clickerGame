using DG.Tweening;
using UnityEngine;

namespace Level.Game
{
    public class SunAnimation : MonoBehaviour
    {
        [SerializeField] private RectTransform _sunTransform;
        [SerializeField] private float _sunVerticalAplitude;
        [SerializeField] private float _sunVerticalDuration;
        [SerializeField] private float _sunHorizontalAplitude;
        [SerializeField] private float _sunHorizontalDuration;
        [SerializeField] private AnimationCurve _sunVerticalAnimationCurve;
        [SerializeField] private AnimationCurve _sunHorizontalAnimationCurve;
        
        private void Start()
        {

            AnimateSun();
        }

        private void AnimateSun()
        {
            _sunTransform.DOAnchorPos(new Vector2(_sunTransform.anchoredPosition3D.x, _sunTransform.anchoredPosition3D.y + _sunVerticalAplitude), _sunVerticalDuration)
                .SetEase(_sunVerticalAnimationCurve).SetLoops(-1, LoopType.Yoyo);
            _sunTransform.DORotate(new Vector3(_sunTransform.anchoredPosition3D.x, _sunTransform.anchoredPosition3D.y,_sunTransform.anchoredPosition3D.z + _sunHorizontalAplitude), _sunHorizontalDuration)
                .SetEase(_sunHorizontalAnimationCurve).SetLoops(-1, LoopType.Yoyo);
        }
    }
}