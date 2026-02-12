using DG.Tweening;
using RSG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;

namespace UI.Popups.DailyRewardPopup
{
    public class DailyRewardDayItem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform RootTransform;

        public RectTransform RootTransformRef => RootTransform;
        [SerializeField] private Image RewardIcon;
        [SerializeField] private Image LockIcon;
        [SerializeField] private TMP_Text LockText;
        [SerializeField] private Image CheckIcon;
        [SerializeField] private TMP_Text CollectedText;
        [SerializeField] private ParticleSystem GlowParticles;
        [SerializeField] private CanvasGroup CanvasGroup;

        [Header("Animation")]
        [SerializeField] private float _bounceScale = 1.15f;
        [SerializeField] private float _bounceDuration = 0.5f;
        [SerializeField] private float _shakeStrength = 5f;
        [SerializeField] private int _shakeVibrato = 10;
        [SerializeField] private float _shakeRandomness = 90f;

        private const string LockTextKey = "be unlocked soon";
        private const string CollectedTextKey = "collected";

        /// <summary>
        /// Sets visual state and runs animation for current day. Call from mediator after setting icon.
        /// </summary>
        public void SetupState(bool isCollected, bool isCurrent, bool isFuture)
        {
            if (RootTransform == null)
                return;

            // Reward icon visibility and color (grey when collected, white when current)
            if (RewardIcon != null)
            {
                RewardIcon.gameObject.SetActive(true);
                RewardIcon.color = isCollected ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
            }

            // Lock: only for future days; hide for collected and current
            if (LockIcon != null)
                LockIcon.gameObject.SetActive(isFuture);
            if (LockText != null)
            {
                LockText.gameObject.SetActive(isFuture);
                if (isFuture)
                    LockText.text = LockTextKey;
            }

            // Collected: green check and "collected" text only for collected days
            if (CheckIcon != null)
                CheckIcon.gameObject.SetActive(isCollected);
            if (CollectedText != null)
            {
                CollectedText.gameObject.SetActive(isCollected);
                if (isCollected)
                    CollectedText.text = CollectedTextKey;
            }

            // Canvas alpha
            if (CanvasGroup != null)
                CanvasGroup.alpha = isFuture ? 0.5f : 1f;

            if (isCurrent)
                PlayCurrentDayAnimation();
            else
                StopAnimations();
        }

        public void SetRewardIcon(Sprite icon)
        {
            if (RewardIcon != null && icon != null)
                RewardIcon.sprite = icon;
        }

        /// <summary>
        /// Plays glow, bounce and shake for the current claimable day.
        /// </summary>
        public void PlayCurrentDayAnimation()
        {
            if (RootTransform == null)
                return;

            RootTransform.DOKill();

            if (GlowParticles != null)
            {
                GlowParticles.Stop();
                GlowParticles.Play();
            }

            var originalScale = Vector3.one;
            var bounceSequence = DOTween.Sequence().KillWith(RootTransform.gameObject);
            bounceSequence.Append(RootTransform.DOScale(originalScale * _bounceScale, _bounceDuration).SetEase(Ease.OutQuad));
            bounceSequence.Append(RootTransform.DOScale(originalScale, _bounceDuration).SetEase(Ease.InQuad));
            bounceSequence.SetLoops(-1, LoopType.Restart);

            RootTransform.DOShakePosition(1f, strength: _shakeStrength, vibrato: _shakeVibrato, randomness: _shakeRandomness, snapping: false, fadeOut: false)
                .SetLoops(-1, LoopType.Restart)
                .KillWith(RootTransform.gameObject);
        }

        /// <summary>
        /// Stops all tweens and particles on this item.
        /// </summary>
        public void StopAnimations()
        {
            if (RootTransform != null)
                RootTransform.DOKill();
            if (GlowParticles != null)
                GlowParticles.Stop();
        }

        /// <summary>
        /// One-shot punch and optional fade for claim feedback. Returns a promise that completes when done.
        /// </summary>
        public IPromise PlayClaimFeedbackAnimation()
        {
            var promise = new Promise();
            if (RootTransform == null)
            {
                promise.Resolve();
                return promise;
            }

            RootTransform.DOKill();
            var seq = DOTween.Sequence().KillWith(RootTransform.gameObject);
            seq.Append(RootTransform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 10, 0.9f));
            if (CanvasGroup != null)
                seq.Join(CanvasGroup.DOFade(0f, 0.4f).SetDelay(0.1f));
            seq.OnComplete(() => promise.Resolve());
            return promise;
        }

        private void OnDisable()
        {
            StopAnimations();
        }
    }
}
