using UnityEngine;

namespace Common.Rewards
{
    public class RewardAnimationItem : MonoBehaviour
    {
        [SerializeField] private RectTransform _mainTransform;
        [SerializeField] private RectTransform _particlesTransform;
        
        public RectTransform MainTransform => _mainTransform;
        public RectTransform ParticlesTransform => _particlesTransform;
    }
}