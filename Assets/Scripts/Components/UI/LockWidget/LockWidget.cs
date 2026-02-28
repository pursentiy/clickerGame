using RSG;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

namespace CoinMaster.App.UI.Screens.Common.Events
{
    public class LockWidget : MonoBehaviour
    {
        [SerializeField] private Animator _lockAnimator;
        [SerializeField] private List<ParticleSystem> _animatedParticles;

        private Promise _unlockAnimationPromise;
        private int _unlockedFromLevel;
        private bool _isVisible;

        private static readonly int UnlockAnimation = Animator.StringToHash("StartUnlocking");

        public void HideWidget(bool hide)
        {
            gameObject.TrySetActive(hide);
        }

        public IPromise PlayUnlockAnimation()
        {
            _unlockAnimationPromise = new Promise(this);
            
            _lockAnimator.SetTrigger(UnlockAnimation);
            
            if (!_isVisible && _animatedParticles != null)
            {
                foreach (var animatedSystem in _animatedParticles)
                {
                    var emissionModule = animatedSystem.emission;
                    emissionModule.enabled = false;
                    animatedSystem.gameObject.SetActive(false);
                }
            }

            return _unlockAnimationPromise;
        }

        /// <summary>
        /// Used by animator
        /// </summary>
        public void FinishUnlockAnimation()
        {
            if(_unlockAnimationPromise == null || !_unlockAnimationPromise.CanBeCanceled)
                return;
            
            _unlockAnimationPromise.Resolve();
        }

        public void AdaptToVisibility(bool isLockVisible)
        {
            _isVisible = isLockVisible;
        }
    }
}
