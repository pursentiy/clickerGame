using System;
using System.Collections.Generic;
using Common.Currency;
using DG.Tweening;
using Extensions;
using Handlers;
using RSG;
using Services;
using Services.FlyingRewardsAnimation;
using ThirdParty.SuperScrollView.Scripts.List;
using ThirdParty.SuperScrollView.Scripts.ListView;
using UnityEngine;
using Utilities.Disposable;
using Zenject;
using AudioExtensions = Extensions.AudioExtensions;
using Object = UnityEngine.Object;

namespace UI.Screens.ChoosePack.PackLevelItem.Base
{
    public interface IPackItemWidgetMediator
    {
        int PackId { get; }
        void UpdateWidgetUnlock(bool isUnlocked);
        void RequestEntranceAnimation();
        void PlayExitAnimation();
    }

    public abstract class BasePackItemWidgetMediator<TView, TInfo> : InjectableListItemMediator<TView, TInfo>, IPackItemWidgetMediator
        where TView : BasePackItemWidgetView
        where TInfo : BasePackItemWidgetInfo
    {
        [Inject] protected readonly SoundHandler _soundHandler;
        [Inject] protected readonly LocalizationService _localization;
        [Inject] protected readonly CurrencyLibraryService _currencyLibraryService;
        
        protected GameObject _packImageInstance;
        private bool _isUnlocked;

        public int PackId => Data.PackId;
        
        protected BasePackItemWidgetMediator(TInfo data) : base(data) { }

        protected override void OnInitialize(bool isVisibleOnRefresh)
        {
            base.OnInitialize(isVisibleOnRefresh);

            // 1. Тексты и контент
            View.PackText.SetText(_localization.GetValue($"pack_{Data.PackName.ToLower()}"));
            
            if (_packImageInstance == null && Data.PackImagePrefab != null)
                _packImageInstance = Object.Instantiate(Data.PackImagePrefab, View.PackImagePrefabContainer);

            // 2. Установка состояния (сразу применяем текущее владение)
            _isUnlocked = Data.IsUnlocked;
            SetupState(_isUnlocked, immediate: true);

            // 3. Подготовка к анимации появления (с задержкой для layout)
            PrepareEntranceDelayed();
        }

        private void PrepareEntranceDelayed()
        {
            if (View.AnimationWidget == null) return;

            // Сразу скрываем если будет анимация
            bool shouldAnimate = Data.EntranceAnimationRequested || !Data.GetEntranceAnimationsAlreadyTriggered();
            if (shouldAnimate)
            {
                View.AnimationWidget.ResetPositionCapture();
            }

            // Даём layout отработать
            DOVirtual.DelayedCall(0.05f, () =>
            {
                if (View == null || View.AnimationWidget == null) return;

                if (shouldAnimate)
                {
                    View.AnimationWidget.Prepare(View.EntranceSlideOffsetY);
                    
                    // Если анимация была запрошена - играем
                    if (Data.EntranceAnimationRequested)
                    {
                        Data.EntranceAnimationRequested = false;
                        PlayEntranceAnimationInternal();
                    }
                }
                else
                {
                    View.AnimationWidget.SetToRest();
                }
            }).KillWith(this);
        }

        public void RequestEntranceAnimation()
        {
            // Если View ещё не инициализирован - ставим флаг, анимация запустится в OnInitialize
            if (View == null)
            {
                Data.EntranceAnimationRequested = true;
                return;
            }

            // View уже есть - подготавливаем с задержкой и играем
            if (View.AnimationWidget != null)
            {
                View.AnimationWidget.ResetPositionCapture();

                DOVirtual.DelayedCall(3.05f, () =>
                {
                    if (View == null || View.AnimationWidget == null) return;
                    View.AnimationWidget.Prepare(View.EntranceSlideOffsetY);
                    PlayEntranceAnimationInternal();
                }).KillWith(this);
            }
            else
            {
                PlayEntranceAnimationInternal();
            }
        }

        private void PlayEntranceAnimationInternal()
        {
            if (View == null || View.AnimationWidget == null)
                return;

            float delay = Data.IndexInList * View.EntranceStaggerDelay;
            
            View.AnimationWidget.PlayEntrance(delay, View.EntranceDuration)
                .CancelWith(this);
        }

        public void PlayExitAnimation()
        {
            if (View == null || View.AnimationWidget == null)
                return;
            
            View.AnimationWidget.PlayExit(View.EntranceSlideOffsetY, View.ExitDuration);
        }

        public void UpdateWidgetUnlock(bool isUnlocked)
        {
            if (_isUnlocked == isUnlocked) return;
            
            _isUnlocked = isUnlocked;
            Data.IsUnlocked = isUnlocked;
            SetupState(_isUnlocked, immediate: false);
        }

        private void SetupState(bool isUnlocked, bool immediate)
        {
            View.PackEnterButton.onClick.RemoveAllListeners();

            if (isUnlocked)
            {
                View.PackEnterButton.onClick.MapListenerWithSound(Data.OnClickAction.SafeInvoke).DisposeWith(this);
                
                if (immediate) ApplyInstantUnlock();
                else UnlockWithAnimation();
            }
            else
            {
                UpdateLockedBlockText();
                var desiredCurrency = Data.CurrencyToUnlock != null ? new List<ICurrency>(Data.CurrencyToUnlock) : new List<ICurrency>();
                View.PackEnterButton.onClick.MapListenerWithSound(() => Data.OnLockedClickAction?.Invoke(desiredCurrency)).DisposeWith(this);
                
                SetLockedVisuals(true);
            }
        }

        private void UpdateLockedBlockText()
        {
            var list = Data.CurrencyToUnlock;
            var currencyToUnlock = list != null && list.Count > 0 ? list[0] : null;
            var currencyName = CurrencyExtensions.GetCurrencyName(currencyToUnlock);
            var spriteAsset = _currencyLibraryService.GetSpriteAsset(currencyName);
            
            if (spriteAsset != null)
                View.LockedBlockText.spriteAsset = spriteAsset;

            var amount = currencyToUnlock?.GetCount() ?? 0;
            View.LockedBlockText.text = _localization.GetFormattedValue("pack_stars_required", $"{amount} <sprite=0>");
        }

        private void SetLockedVisuals(bool locked)
        {
            View.FadeImage.gameObject.SetActive(locked);
            View.LockWidget.HideWidget(locked);
            if (locked)
            {
                View.FadeImage.color = View.FadeImage.color.SetAlpha(View.FadeImageAlpha);
            }
        }

        protected virtual void ApplyInstantUnlock()
        {
            SetLockedVisuals(false);
            View.PackImagePrefabContainer.localScale = Vector3.one;
        }
        
        protected virtual void UnlockWithAnimation()
        {
            _soundHandler.PlaySound(AudioExtensions.PackUnlockedKey);

            View.LockWidget.PlayUnlockAnimation()
                .Then(OnLockAnimationFinished)
                .Then(() => SetLockedVisuals(false))
                .CancelWith(this);

            IPromise OnLockAnimationFinished()
            {
                var seq = DOTween.Sequence().KillWith(View.gameObject);
                seq.Join(View.FadeImage.DOFade(0, View.UnlockDuration).SetEase(Ease.Linear));
                seq.Append(View.Holder.DOLocalMoveY(30f, View.UnlockDuration * 0.3f).SetEase(Ease.OutCubic));
                seq.Append(View.Holder.DOLocalMoveY(0f, View.UnlockDuration * 0.6f).SetEase(Ease.OutBounce));

                // 3. Эффекты
                if (View.UnlockParticles != null)
                    seq.InsertCallback(View.UnlockDuration * 0.3f, () => View.UnlockParticles.Play());

                return seq.AsPromise();
            }
        }

        protected override void OnRelease(ListItemReleaseType type)
        {
            View.Holder.DOKill();
            View.FadeImage.DOKill();
            View.LockWidget.DOKill();
            base.OnRelease(type);
        }
    }
}