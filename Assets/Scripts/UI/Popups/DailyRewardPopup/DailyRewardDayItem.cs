using DG.Tweening;
using Extensions;
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
        [SerializeField] private RectTransform rootTransform; // Остается в LayoutGroup
        [SerializeField] private RectTransform contentHolder; // НОВЫЙ: анимируем ЕГО
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Canvas itemCanvas;

        [Header("State Blocks")]
        [SerializeField] private GameObject readyToCollectBlock;
        [SerializeField] private GameObject lockedBlock;
        [SerializeField] private GameObject alreadyCollectedBlock;

        [Header("Content")]
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TMP_Text rewardCurrencyText;
        [SerializeField] private Image checkIcon;
        [SerializeField] private TMP_Text collectedText;
        [SerializeField] private ParticleSystem glowParticles;
        [SerializeField] private ParticleSystem dustParticles;

        [Header("Animation Settings")]
        [SerializeField] private float _readyBounce = 0.1f;
        
        private int _initialSortingOrder;
        private Vector3 _initialContentScale = Vector3.one;
        
        public RectTransform RootTransform => rootTransform;

        private void Awake()
        {
            if (itemCanvas != null) 
                _initialSortingOrder = itemCanvas.sortingOrder;
            
            if (contentHolder == null) contentHolder = rootTransform; // Fallback
        }

        public void SetupState(DayItemState state)
        {
            StopAnimations();
            ResetVisuals();

            switch (state)
            {
                case DayItemState.Collected:
                    readyToCollectBlock.SetActive(true);
                    lockedBlock.SetActive(false);
                    alreadyCollectedBlock.SetActive(true);
                    rewardIcon.color = Color.white; 
                    rewardCurrencyText.gameObject.SetActive(false);
                    if (backgroundImage != null) backgroundImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
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
                    rewardIcon.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                    PlayLockedSubtleAnimation();
                    break;
            }
        }

        private void ResetVisuals()
        {
            contentHolder.DOKill();
            contentHolder.localScale = _initialContentScale;
            contentHolder.localRotation = Quaternion.identity;
            contentHolder.anchoredPosition = Vector2.zero;
            
            if (itemCanvas != null)
            {
                itemCanvas.overrideSorting = false;
                itemCanvas.sortingOrder = _initialSortingOrder;
            }
        }

        public void PlayCurrentDayAnimation()
        {
            StopAnimations();
            // Делаем более "живое" дыхание
            contentHolder.DOScale(_initialContentScale * (1f + _readyBounce), 1.2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(this);
            
            contentHolder.DORotate(new Vector3(0, 0, 3f), 1.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(this);

            if (glowParticles != null && !glowParticles.isPlaying) glowParticles.Play();
        }

        private void PlayLockedSubtleAnimation()
        {
            contentHolder.DOPunchPosition(new Vector3(2f, 0, 0), 2f, 1, 0.5f)
                .SetEase(Ease.Linear)
                .SetLoops(-1)
                .SetId(this);
        }

        public IPromise PlayClaimFeedbackAnimation()
        {
            var promise = new Promise();
            StopAnimations();

            if (itemCanvas != null)
            {
                itemCanvas.overrideSorting = true;
                itemCanvas.sortingOrder = _initialSortingOrder + 100;
            }

            // СОЧНАЯ АНИМАЦИЯ (Squash & Stretch + Pop)
            var seq = DOTween.Sequence().SetId(this);
            
            // 1. Предвкушение (сжимаем вниз)
            seq.Append(contentHolder.DOScale(new Vector3(1.2f, 0.7f, 1f), 0.1f).SetEase(Ease.OutQuad));
            
            // 2. Взрыв (выстреливаем вверх)
            seq.Append(contentHolder.DOScale(new Vector3(0.8f, 1.4f, 1f), 0.15f).SetEase(Ease.OutBack));
            seq.Join(contentHolder.DOLocalMoveY(40f, 0.15f).SetRelative().SetEase(Ease.OutCubic));
            
            // 3. Удар (возвращаемся и "бахаем")
            seq.Append(contentHolder.DOScale(new Vector3(1.1f, 1.1f, 1f), 0.1f).SetEase(Ease.InBack));
            seq.Join(contentHolder.DOLocalMoveY(-40f, 0.1f).SetRelative().SetEase(Ease.InCubic));
            
            seq.AppendCallback(() => {
                if (dustParticles != null) dustParticles.Play();
                contentHolder.DOShakeRotation(0.3f, 15f, 30);
                SetupState(DayItemState.Collected);
            });
            
            // 4. Финальный отскок в нормальный размер
            seq.Append(contentHolder.DOScale(1f, 0.2f).SetEase(Ease.OutElastic));

            seq.OnComplete(() => promise.SafeResolve());

            return promise;
        }

        public void StopAnimations()
        {
            DOTween.Kill(this);
            contentHolder.DOKill();
            if (glowParticles != null) glowParticles.Stop();
        }

        public void SetRewardIcon(Sprite icon) => rewardIcon.sprite = icon;
        private void OnDisable() => StopAnimations();
    }
}