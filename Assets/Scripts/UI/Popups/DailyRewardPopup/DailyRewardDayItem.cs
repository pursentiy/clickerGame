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
            StopAnimations();
            ResetVisuals();

            switch (state)
            {
                case DayItemState.Collected:
                    readyToCollectBlock.SetActive(true);
                    lockedBlock.SetActive(false);
                    alreadyCollectedBlock.SetActive(true);
                    
                    // Убираем серость, оставляем оригинальный цвет иконки
                    rewardIcon.color = Color.white; 
                    rewardCurrencyText.gameObject.SetActive(false);
                    collectedText.text = CollectedTextKey;
                    
                    // Можно чуть притемнить только фон, если нужно отделить от активных
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
                    
                    lockText.text = LockTextKey;
                    
                    // Только для будущих наград делаем иконку сероватой
                    rewardIcon.color = new Color(0.4f, 0.4f, 0.4f, 1f); 
                    if (backgroundImage != null) backgroundImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                    
                    PlayLockedSubtleAnimation();
                    break;
            }
        }

        private void ResetVisuals()
        {
            // Убиваем твины жестко перед сбросом
            rootTransform.DOKill();
            
            rootTransform.localScale = Vector3.one;
            rootTransform.localRotation = Quaternion.identity;
            rootTransform.anchoredPosition = Vector2.zero; // Используем anchoredPosition для UI
            
            rewardIcon.color = Color.white;
            if (backgroundImage != null) backgroundImage.color = Color.white;
        }

        public void PlayCurrentDayAnimation()
        {
            // Используем Sequence для лучшего контроля
            Sequence s = DOTween.Sequence().SetId(this).KillWith(this);
            
            s.Join(rootTransform.DOScale(1f + _readyBounce, 0.8f).SetEase(Ease.InOutQuad));
            s.Join(rootTransform.DORotate(new Vector3(0, 0, _readyRotation), 1.2f).SetEase(Ease.InOutSine));
            
            s.SetLoops(-1, LoopType.Yoyo);
            
            if (glowParticles != null) glowParticles.Play();
        }

        private void PlayLockedSubtleAnimation()
        {
            rootTransform.DORotate(new Vector3(0, 0, _lockedRotation), 2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(this)
                .KillWith(this);
        }

        public void StopAnimations()
        {
            // Убиваем по ID этого объекта, чтобы не затронуть другие айтемы в списке
            DOTween.Kill(this);
            rootTransform.DOKill();
            
            if (glowParticles != null) glowParticles.Stop();
        }

        public IPromise PlayClaimFeedbackAnimation()
        {
            var promise = new Promise();
            StopAnimations();
    
            // Сброс в дефолт
            rootTransform.localScale = Vector3.one;
            rootTransform.localRotation = Quaternion.identity;

            Sequence epicSeq = DOTween.Sequence().SetId(this).KillWith(this);

            // 1. Антиципация (сжатие перед прыжком)
            epicSeq.Append(rootTransform.DOScale(0.85f, 0.15f).SetEase(Ease.OutQuad));

            // 2. Взрывной прыжок вперед с пружиной
            epicSeq.Append(rootTransform.DOScale(1.4f, 0.4f).SetEase(Ease.OutElastic, 0.5f, 0.75f));
    
            // Параллельно тряхнем иконку награды для акцента
            epicSeq.Join(rewardIcon.transform.DOPunchPosition(new Vector3(0, 20, 0), 0.5f, 5, 1f));
    
            // Вспышка фона (если есть Image)
            if (backgroundImage != null)
            {
                epicSeq.Join(backgroundImage.DOColor(Color.white, 0.1f).SetLoops(2, LoopType.Yoyo));
            }

            // 3. Возврат к нормальному состоянию и смена стейта
            epicSeq.Append(rootTransform.DOScale(1.0f, 0.2f).SetEase(Ease.InQuad));
    
            epicSeq.OnComplete(() =>
            {
                SetupState(DayItemState.Collected);
                promise.Resolve();
            });

            return epicSeq.AsPromise();
        }

        public void SetRewardIcon(Sprite icon)
        {
            if (icon != null)
                rewardIcon.sprite = icon;
        }

        private void OnDisable() => StopAnimations();
    }
}