using System;
using DG.Tweening;
using Extensions;
using Handlers;
using Installers;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace UI.Screens.ChoosePack.Widgets
{
    public class PackItemWidget : InjectableMonoBehaviour
    {
        [Inject] private SoundHandler _soundHandler;
        [Inject] private LocalizationService _localization;

        [SerializeField] private RectTransform _holder;
        [SerializeField] private Image _fadeImage;
        [SerializeField] private RectTransform _packImagePrefabContainer;
        [SerializeField] private TMP_Text _packText;
        [SerializeField] private Button _packEnterButton;
        [SerializeField] private TMP_Text _lockedBlockText;
        [SerializeField] private RectTransform _lockedBlockHolder;
        [SerializeField] private ParticleSystem _unlockParticles;

        [Header("Unlock Animation Settings")]
        [SerializeField] private float _unlockDuration = 0.6f;

        private GameObject _packImageInstance;
        private int _packId;
        private bool _isUnlocked;
        private bool _isInitialized;

        public bool IsUnlocked => _isUnlocked;
        public int PackId => _packId;

        public void Initialize(string packName, GameObject packImagePrefab, int packId, bool isUnlocked, 
            Action onClickAction, Action onLockedClickAction, int starsRequired)
        {
            var packKey = $"pack_{packName.ToLower()}";
            _packText.text = _localization.GetValue(packKey);
            
            if (_packImageInstance == null)
                _packImageInstance = Instantiate(packImagePrefab, _packImagePrefabContainer);

            _packId = packId;

            _isInitialized = true;
            UpdateState(isUnlocked, onClickAction, onLockedClickAction, starsRequired, immediate: true);
        }
        
        public void UpdateState(bool isUnlocked, Action onClickAction, Action onLockedClickAction, int starsRequired, bool immediate = false)
        {
            if (!_isInitialized)
            {
                LoggerService.LogWarning(this, $"Widget is not initialized at {nameof(UpdateState)}.");
                return;
            }
            
            if (_isUnlocked == isUnlocked && !immediate) 
                return;

            _isUnlocked = isUnlocked;
            _packEnterButton.onClick.RemoveAllListeners();

            if (_isUnlocked)
            {
                _packEnterButton.onClick.MapListenerWithSound(onClickAction.SafeInvoke).DisposeWith(this);
                
                if (immediate)
                    ApplyInstantUnlock();
                else
                    UnlockWithAnimation();
            }
            else
            {
                _lockedBlockText.text = _localization.GetFormattedValue("pack_stars_required", $"{starsRequired} <sprite=0>");
                _packEnterButton.onClick.MapListenerWithSound(onLockedClickAction.SafeInvoke).DisposeWith(this);
                
                _fadeImage.gameObject.SetActive(true);
                _lockedBlockHolder.gameObject.SetActive(true);
            }
        }

        private void ApplyInstantUnlock()
        {
            _fadeImage.gameObject.SetActive(false);
            _lockedBlockHolder.gameObject.SetActive(false);
            _lockedBlockText.gameObject.SetActive(false);
            _packImagePrefabContainer.localScale = Vector3.one;
        }
        
        private void UnlockWithAnimation()
        {
            _lockedBlockHolder.gameObject.SetActive(true);
            _soundHandler.PlaySound("pack_unlocked");
    
            _fadeImage.DOFade(0, _unlockDuration).SetEase(Ease.Linear)
                .OnComplete(() => _fadeImage.gameObject.SetActive(false))
                .KillWith(this);

            _lockedBlockHolder.DOScale(0, _unlockDuration).SetEase(Ease.InBack)
                .OnComplete(() => _lockedBlockHolder.gameObject.SetActive(false))
                .KillWith(this);
    
            _holder.DOComplete();
    
            var unlockSequence = DOTween.Sequence().KillWith(gameObject);

            unlockSequence.Append(_holder.DOLocalMoveY(30f, _unlockDuration * 0.3f).SetEase(Ease.OutCubic));
            unlockSequence.AppendInterval(_unlockDuration * 0.7f);
            unlockSequence.Append(_holder.DOLocalMoveY(0f, _unlockDuration * 0.7f).SetEase(Ease.OutBounce));
    
            if (_unlockParticles != null)
            {
                DOVirtual.DelayedCall(_unlockDuration * 0.3f, () => _unlockParticles.Play()).KillWith(this);
            }
        }
    }
}