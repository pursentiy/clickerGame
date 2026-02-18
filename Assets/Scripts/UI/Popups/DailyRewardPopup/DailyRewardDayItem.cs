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
        [SerializeField] private RectTransform rootTransform;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Canvas itemCanvas; // Обязательно добавьте Canvas на префаб

        [Header("State Blocks")]
        [SerializeField] private GameObject readyToCollectBlock;
        [SerializeField] private GameObject lockedBlock;
        [SerializeField] private GameObject alreadyCollectedBlock;

        [Header("Content Content")]
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TMP_Text rewardCurrencyText;
        [SerializeField] private Image checkIcon;
        [SerializeField] private TMP_Text collectedText;
        [SerializeField] private ParticleSystem glowParticles;
        [SerializeField] private ParticleSystem dustParticles;

        [Header("Animation Settings")]
        [SerializeField] private float _readyBounce = 0.12f;
        [SerializeField] private float _readyRotation = 6f;

        private const string LockTextKey = "be unlocked soon";
        private const string CollectedTextKey = "COLLECTED";
        
        private int _initialSortingOrder;
        
        public RectTransform RootTransform => rootTransform;

        private void Awake()
        {
            if (itemCanvas != null) 
                _initialSortingOrder = itemCanvas.sortingOrder;
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
                    collectedText.text = CollectedTextKey;
                    
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
                    if (backgroundImage != null) backgroundImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                    
                    PlayLockedSubtleAnimation();
                    break;
            }
        }

        private void ResetVisuals()
        {
            rootTransform.DOKill();
            rootTransform.localScale = Vector3.one;
            rootTransform.localRotation = Quaternion.identity;
            
            // Важно: возвращаем в локальный ноль ячейки
            rootTransform.anchoredPosition = Vector2.zero; 
            
            if (itemCanvas != null)
            {
                itemCanvas.overrideSorting = false;
                itemCanvas.sortingOrder = _initialSortingOrder;
            }
        }

        public void PlayCurrentDayAnimation()
        {
            StopAnimations();
            Sequence s = DOTween.Sequence().SetId(this).KillWith(this);
            s.Join(rootTransform.DOScale(1f + _readyBounce, 0.8f).SetEase(Ease.InOutQuad));
            s.Join(rootTransform.DORotate(new Vector3(0, 0, _readyRotation), 1f).SetEase(Ease.InOutSine));
            s.SetLoops(-1, LoopType.Yoyo);

            if (glowParticles != null) glowParticles.Play();
        }

        private void PlayLockedSubtleAnimation()
        {
            StopAnimations();
            // Едва заметное покачивание для заблокированных
            rootTransform.DORotate(new Vector3(0, 0, 1.5f), 2.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(this)
                .KillWith(this);
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

            var epicSeq = DOTween.Sequence().SetId(this).KillWith(this);
            epicSeq.Append(rootTransform.DOScale(0.8f, 0.15f).SetEase(Ease.OutQuad));
            epicSeq.Append(rootTransform.DOScale(1.5f, 0.3f).SetEase(Ease.OutBack));
            epicSeq.Append(rootTransform.DOScale(1.0f, 0.2f).SetEase(Ease.InQuad));
            
            epicSeq.AppendCallback(() =>
            {
                if (dustParticles != null) dustParticles.Play();
                //rootTransform.DOShakeRotation(0.3f, 12f, 25).KillWith(this);
            });

            epicSeq.OnComplete(() =>
            {
                SetupState(DayItemState.Collected);
                promise.SafeResolve();
            });

            return promise;
        }

        public void StopAnimations()
        {
            DOTween.Kill(this);
            rootTransform.DOKill();
            if (glowParticles != null) glowParticles.Stop();
        }

        public void SetRewardIcon(Sprite icon) => rewardIcon.sprite = icon;
        private void OnDisable() => StopAnimations();
    }
}