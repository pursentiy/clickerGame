using DG.Tweening;
using RSG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;

namespace UI.Popups.DailyRewardPopup
{
    public enum DayItemState
    {
        Collected,
        ReadyToReceive,
        ToBeCollected
    }

    public class DailyRewardDayItem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform RootTransform;

        public RectTransform RootTransformRef => RootTransform;

        [Header("State Blocks (match hierarchy: ReadyToCollectBlock, LockedBlock, AlreadyCollectedBlock)")]
        [SerializeField] private GameObject readyToCollectBlock;
        [SerializeField] private GameObject lockedBlock;
        [SerializeField] private GameObject alreadyCollectedBlock;

        [Header("Block Content (optional; for icon, text, check)")]
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TMP_Text rewardCurrencyText;
        [SerializeField] private Image lockIcon;
        [SerializeField] private TMP_Text lockText;
        [SerializeField] private Image checkIcon;
        [SerializeField] private TMP_Text collectedText;
        [SerializeField] private ParticleSystem glowParticles;
        [SerializeField] private CanvasGroup disabledElementCanvasGroup;

        [Header("Animation")]
        [SerializeField] private float _bounceScale = 1.15f;
        [SerializeField] private float _bounceDuration = 0.5f;
        [SerializeField] private float _shakeStrength = 5f;
        [SerializeField] private int _shakeVibrato = 10;
        [SerializeField] private float _shakeRandomness = 90f;

        private const string LockTextKey = "be unlocked soon";
        private const string CollectedTextKey = "collected";

        /// <summary>
        /// Sets visual state by enabling the right block and content. Call from mediator after setting icon.
        /// </summary>
        public void SetupState(DayItemState state)
        {
            switch (state)
            {
                case DayItemState.Collected:
                    readyToCollectBlock.SetActive(true);
                    lockedBlock.SetActive(false);
                    alreadyCollectedBlock.SetActive(true);
                    rewardIcon.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    rewardCurrencyText.gameObject.SetActive(false);
                    collectedText.text = CollectedTextKey;
                    disabledElementCanvasGroup.alpha = 0f;
                    StopAnimations();
                    break;

                case DayItemState.ReadyToReceive:
                    readyToCollectBlock.SetActive(true);
                    lockedBlock.SetActive(false);
                    alreadyCollectedBlock.SetActive(false);
                    rewardIcon.color = Color.white;
                    rewardCurrencyText.gameObject.SetActive(true);
                    disabledElementCanvasGroup.alpha = 0f;
                    PlayCurrentDayAnimation();
                    break;

                case DayItemState.ToBeCollected:
                    readyToCollectBlock.SetActive(false);
                    lockedBlock.SetActive(true);
                    alreadyCollectedBlock.SetActive(false);
                    lockText.text = LockTextKey;
                    disabledElementCanvasGroup.alpha = 0.5f;
                    StopAnimations();
                    break;
            }
        }

        public void SetRewardIcon(Sprite icon)
        {
            if (icon != null)
                rewardIcon.sprite = icon;
        }

        /// <summary>
        /// Plays glow, bounce and shake for the current claimable day.
        /// </summary>
        public void PlayCurrentDayAnimation()
        {
            RootTransform.DOKill();

            glowParticles.Stop();
            glowParticles.Play();

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
            RootTransform.DOKill();
            glowParticles.Stop();
        }

        /// <summary>
        /// One-shot punch and optional fade for claim feedback. Returns a promise that completes when done.
        /// </summary>
        public IPromise PlayClaimFeedbackAnimation()
        {
            var promise = new Promise();
            RootTransform.DOKill();
            var seq = DOTween.Sequence().KillWith(RootTransform.gameObject);
            seq.Append(RootTransform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 10, 0.9f));
            seq.Join(disabledElementCanvasGroup.DOFade(0f, 0.4f).SetDelay(0.1f));
            seq.OnComplete(() => promise.Resolve());
            return promise;
        }

        private void OnDisable()
        {
            StopAnimations();
        }
    }
}
