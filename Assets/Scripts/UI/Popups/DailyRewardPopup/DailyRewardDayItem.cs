using DG.Tweening;
using RSG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups.DailyRewardPopup
{
    public class DailyRewardDayItem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform rootTransform;
        [SerializeField] private Image backgroundImage;

        [Header("State Blocks")]
        [SerializeField] private GameObject readyToCollectBlock;
        [SerializeField] private GameObject lockedBlock;
        [SerializeField] private GameObject alreadyCollectedBlock;

        [Header("Content")]
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TMP_Text rewardCurrencyText;
        [SerializeField] private Image lockIcon;
        [SerializeField] private TMP_Text lockText;
        [SerializeField] private Image checkIcon;
        [SerializeField] private TMP_Text collectedText;
        [SerializeField] private ParticleSystem glowParticles;
        [SerializeField] private CanvasGroup contentCanvasGroup;

        [Header("Ready Animation Settings")]
        [SerializeField] private float _readyBounce = 0.1f;
        [SerializeField] private float _readyRotation = 5f;

        [Header("Locked Animation Settings")]
        [SerializeField] private float _lockedRotation = 2f;

        private const string LockTextKey = "be unlocked soon";
        private const string CollectedTextKey = "collected";
        
        public RectTransform RootTransform => rootTransform;

        public void SetupState(DayItemState state)
        {
            // Полная остановка всего перед настройкой
            StopAnimations();
            ResetVisuals();

            switch (state)
            {
                case DayItemState.Collected:
                    readyToCollectBlock.SetActive(true);
                    lockedBlock.SetActive(false);
                    alreadyCollectedBlock.SetActive(true);
                    
                    rewardIcon.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    rewardCurrencyText.gameObject.SetActive(false);
                    collectedText.text = CollectedTextKey;
                    break;

                case DayItemState.ReadyToReceive:
                    readyToCollectBlock.SetActive(true);
                    lockedBlock.SetActive(false);
                    alreadyCollectedBlock.SetActive(false);
                    
                    rewardIcon.color = Color.white;
                    rewardCurrencyText.gameObject.SetActive(true);
                    PlayCurrentDayAnimation();
                    break;

                case DayItemState.ToBeCollected:
                    readyToCollectBlock.SetActive(false);
                    lockedBlock.SetActive(true);
                    alreadyCollectedBlock.SetActive(false);
                    
                    lockText.text = LockTextKey;
                    PlayLockedSubtleAnimation();
                    break;
            }
        }

        private void ResetVisuals()
        {
            RootTransform.localScale = Vector3.one;
            RootTransform.localRotation = Quaternion.identity;
            // Сбрасываем только локальную позицию, не трогаем анкоры
            RootTransform.localPosition = Vector3.zero; 
            contentCanvasGroup.alpha = 1f;
            rewardIcon.color = Color.white;
            if (backgroundImage != null) backgroundImage.color = Color.white;
        }

        public void PlayCurrentDayAnimation()
        {
            StopAnimations();

            // 1. Увеличиваем и плавно пульсируем
            RootTransform.DOScale(1f + _readyBounce, 0.8f)
                .SetEase(Ease.InOutQuad)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(RootTransform);

            // 2. Игривое вращение
            RootTransform.DORotate(new Vector3(0, 0, _readyRotation), 1.2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(RootTransform);

            if (glowParticles != null) glowParticles.Play();
        }

        private void PlayLockedSubtleAnimation()
        {
            StopAnimations();
            
            // Просто легкое покачивание для "будущих" элементов
            RootTransform.DORotate(new Vector3(0, 0, _lockedRotation), 2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(RootTransform);
        }

        public void StopAnimations()
        {
            // Убиваем все твины именно на этом объекте
            RootTransform.DOKill(true);
            if (glowParticles != null) glowParticles.Stop();
        }

        public IPromise PlayClaimFeedbackAnimation()
        {
            var promise = new Promise();
            StopAnimations();

            // Вместо изменения позиции используем Punch — это не ломает верстку
            RootTransform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 10, 1f)
                .OnComplete(() =>
                {
                    SetupState(DayItemState.Collected);
                    promise.Resolve();
                });

            return promise;
        }

        public void SetRewardIcon(Sprite icon)
        {
            if (icon != null)
                rewardIcon.sprite = icon;
        }

        private void OnDisable() => StopAnimations();
    }
}