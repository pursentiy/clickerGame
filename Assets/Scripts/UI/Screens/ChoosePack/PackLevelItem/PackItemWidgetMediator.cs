using System;
using DG.Tweening;
using Extensions;
using Handlers;
using Services;
using ThirdParty.SuperScrollView.Scripts.List;
using UnityEngine;
using Utilities.Disposable;
using Zenject;
using AudioExtensions = Extensions.AudioExtensions;
using Object = UnityEngine.Object;

namespace UI.Screens.ChoosePack.PackLevelItem
{
    public class PackItemWidgetMediator : InjectableListItemMediator<PackItemWidgetView, PackItemWidgetInfo>
    {
        [Inject] private SoundHandler _soundHandler;
        [Inject] private LocalizationService _localization;
        
        private GameObject _packImageInstance;
        private bool _isUnlocked;
        private bool _isInitialized;

        public int PackId => Data.PackId;
        
        public PackItemWidgetMediator(PackItemWidgetInfo data) : base(data)
        {
        }

        protected override void OnInitialize(bool isVisibleOnRefresh)
        {
            base.OnInitialize(isVisibleOnRefresh);

            var packKey = $"pack_{Data.PackName.ToLower()}";
            View.PackText.SetText(_localization.GetValue(packKey));
            
            if (_packImageInstance == null)
                _packImageInstance = Object.Instantiate(Data.PackImagePrefab, View.PackImagePrefabContainer);

            _isInitialized = true;
            UpdateMediator(Data.IsUnlocked, Data.OnClickAction, Data.OnLockedClickAction, Data.StarsRequired, immediate: true);
        }

        public void UpdateWidgetUnlock(bool isUnlocked)
        {
            if (Data.IsUnlocked == isUnlocked)
                return;
            
            Data.IsUnlocked = isUnlocked;
            UpdateMediator(Data.IsUnlocked, Data.OnClickAction, Data.OnLockedClickAction, Data.StarsRequired, false);
        }
        
        private void UpdateMediator(bool isUnlocked, Action onClickAction, Action onLockedClickAction, int starsRequired, bool immediate = false)
        {
            if (!_isInitialized)
            {
                LoggerService.LogWarning(this, $"Widget is not initialized at {nameof(UpdateMediator)}.");
                return;
            }
            
            if (_isUnlocked == isUnlocked && !immediate) 
                return;

            _isUnlocked = isUnlocked;
            View.PackEnterButton.onClick.RemoveAllListeners();

            if (_isUnlocked)
            {
                View.PackEnterButton.onClick.MapListenerWithSound(onClickAction.SafeInvoke).DisposeWith(this);
                
                if (immediate)
                    ApplyInstantUnlock();
                else
                    UnlockWithAnimation();
            }
            else
            {
                View.LockedBlockText.text = _localization.GetFormattedValue("pack_stars_required", $"{starsRequired} <sprite=0>");
                View.PackEnterButton.onClick.MapListenerWithSound(onLockedClickAction.SafeInvoke).DisposeWith(this);
                
                View. FadeImage.gameObject.SetActive(true);
                View.LockedBlockHolder.gameObject.SetActive(true);
            }
        }

        private void ApplyInstantUnlock()
        {
            View.FadeImage.gameObject.SetActive(false);
            View.LockedBlockHolder.gameObject.SetActive(false);
            View.LockedBlockText.gameObject.SetActive(false);
            View.PackImagePrefabContainer.localScale = Vector3.one;
        }
        
        private void UnlockWithAnimation()
        {
            View.LockedBlockHolder.gameObject.SetActive(true);
            _soundHandler.PlaySound(AudioExtensions.PackUnlockedKey);
    
            View.FadeImage.DOFade(0, View.UnlockDuration).SetEase(Ease.Linear)
                .OnComplete(() => View.FadeImage.gameObject.SetActive(false))
                .KillWith(this);

            View.LockedBlockHolder.DOScale(0, View.UnlockDuration).SetEase(Ease.InBack)
                .OnComplete(() => View.LockedBlockHolder.gameObject.SetActive(false))
                .KillWith(this);
    
            View.Holder.DOComplete();
    
            var unlockSequence = DOTween.Sequence().KillWith(View.gameObject);

            unlockSequence.Append(View.Holder.DOLocalMoveY(30f, View.UnlockDuration * 0.3f).SetEase(Ease.OutCubic));
            unlockSequence.AppendInterval(View.UnlockDuration * 0.7f);
            unlockSequence.Append(View.Holder.DOLocalMoveY(0f, View.UnlockDuration * 0.7f).SetEase(Ease.OutBounce));
    
            if (View.UnlockParticles != null)
            {
                DOVirtual.DelayedCall(View.UnlockDuration * 0.3f, () => View.UnlockParticles.Play()).KillWith(this);
            }
        }
    }
}