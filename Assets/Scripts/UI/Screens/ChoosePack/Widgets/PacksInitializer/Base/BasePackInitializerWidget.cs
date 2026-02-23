using System;
using System.Collections.Generic;
using System.Linq;
using Common.Currency;
using Common.Data.Info;
using Components.UI;
using Configurations.Progress;
using Controllers;
using Extensions;
using Installers;
using Plugins.FSignal;
using RSG;
using Services;
using Services.CoroutineServices;
using ThirdParty.SuperScrollView.Scripts.GridView;
using ThirdParty.SuperScrollView.Scripts.List;
using UI.Screens.ChoosePack.PackLevelItem.Base;
using UI.Screens.ChoosePack.PackLevelItem.Base.PackClickAction;
using UI.Screens.ChoosePack.Widgets.PacksInitializer.Sequences.NoCurrencySequence;
using UnityEngine;
using Utilities;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack.Widgets.PacksInitializer.Base
{
    public abstract class BasePackInitializerWidget : InjectableMonoBehaviour
    {
        protected const float MessagePopupFontSize = 175f;

        [Inject] protected readonly ProgressProvider _progressProvider;
        [Inject] protected readonly FlowScreenController _flowScreenController;
        [Inject] private readonly CoroutineService _coroutineService;

        [SerializeField] protected LoopGridView _loopGridView;

        protected CurrencyDisplayWidget _currencyDisplayWidget;
        protected AdsButtonWidget _adsButtonWidget;
        private GridViewAdapter _gridViewAdapter;
        private IReadOnlyCollection<PackInfo> _packsInfos;

        private bool _gridInitialized;
        private bool _entryAnimationRequested;

        public FSignal GridInitializationDoneSignal { get; } = new();
        public bool EntranceAnimationsAlreadyTriggered { get; set; }

        protected abstract PackType TargetPackType { get; }
        protected abstract BasePackItemWidgetInfo CreatePackWidgetInfoInternal(PackInfo packInfo, int packId, bool isUnlocked, List<ICurrency> currencyToUnlock, int indexInList, System.Func<bool> getEntranceAnimationsAlreadyTriggered, Action<IPackClickAction> onPackClicked);
        protected abstract IListItem CreateMediator(BasePackItemWidgetInfo info);
        protected abstract Func<IDisposeProvider, IPromise<MediatorFlowInfo>> GetShowMessagePopupPromiseFunc(RectTransform popupAnchorRect);
        protected abstract void LaunchUnlockPackSequenceOnClick(List<ICurrency> desiredCurrency, PackClickAction clickAction);
  
        protected void OnPackClicked(int packId, IPackClickAction clickAction)
        {
            var packInfo = _progressProvider.GetPackInfo(packId);
            if (packInfo == null)
            {
                LoggerService.LogError(this, $"Pack {packId} doesn't exist at {nameof(OnPackClicked)}");
                return;
            }
            
            var status = _progressProvider.GetPackStatus(packId);
            HandleClickedPackStatus(packInfo, status, clickAction);
        }

        private void HandleClickedPackStatus(PackInfo packInfo, PackStatus status, IPackClickAction clickAction)
        {
            if (status.IsAvailable())
            {
                OnAvailablePackClicked(packInfo);
                return;
            }

            if (status.IsLocked())
            {
                OnLockedPackClicked(packInfo, clickAction);
            }

            if (status.CanBeUnlocked())
            {
                OnUnlockablePackClicked(packInfo, clickAction);
            }
            
            LoggerService.LogWarning(this,  $"Exiting {nameof(HandleClickedPackStatus)} for Pack {packInfo.PackName} with status {status}");
        }
        
        protected void OnLockedPackClicked(PackInfo packInfo, IPackClickAction clickAction)
        {
            var desiredCurrency = _progressProvider.GetCurrencyToUnlock(packInfo.PackId) ?? new List<ICurrency>();
            if (desiredCurrency.IsCollectionNullOrEmpty())
            {
                LoggerService.LogError(this, $"desiredCurrency is null or empty for PackId {packInfo.PackId} at {nameof(OnLockedPackClicked)}");
                return;
            }
            
            LaunchLockedActionPackSequenceOnClick(desiredCurrency, clickAction.AsPackClickAction());
        }

        protected void OnUnlockablePackClicked(PackInfo packInfo, IPackClickAction clickAction)
        {
            var desiredCurrency = _progressProvider.GetCurrencyToUnlock(packInfo.PackId) ?? new List<ICurrency>();
            if (desiredCurrency.IsCollectionNullOrEmpty())
            {
                LoggerService.LogError(this, $"desiredCurrency is null or empty for PackId {packInfo.PackId} at {nameof(OnUnlockablePackClicked)}");
                return;
            }
            
            LaunchUnlockPackSequenceOnClick(desiredCurrency, clickAction.AsPackClickAction());
        }
        
        protected virtual void LaunchLockedActionPackSequenceOnClick(List<ICurrency> desiredCurrency, PackClickAction clickAction)
        {
            if (_currencyDisplayWidget == null)
            {
                LoggerService.LogError(this, $"{nameof(CurrencyDisplayWidget)} is null at  {nameof(LaunchLockedActionPackSequenceOnClick)}");
                return;
            }

            if (_adsButtonWidget == null)
            {
                LoggerService.LogError(this, $"{nameof(_adsButtonWidget)} is null at  {nameof(LaunchLockedActionPackSequenceOnClick)}");
                return;
            }
            
            var popupAnchorRect = EvaluatePopupAnchorRectFromClickAction(clickAction);
            
            StateMachine
                .CreateMachine(new VisualizeNotEnoughCurrencyContext(_currencyDisplayWidget, _adsButtonWidget, desiredCurrency, GetShowMessagePopupPromiseFunc(popupAnchorRect)))
                .StartSequence<VisualizeNotEnoughCurrencyState>()
                .FinishWith(this);
        }

        protected virtual void InitializePackButtons()
        {
            var allPacks = _progressProvider.GetAllPacks();
            if (allPacks.IsCollectionNullOrEmpty())
            {
                LoggerService.LogError(this, $"[{nameof(InitializePackButtons)}]: {nameof(ProgressProvider)} packs params are null or empty.");
                return;
            }

            _packsInfos = allPacks.Where(pack => pack != null && pack.PackType == TargetPackType).ToList();
            SetupPacksList();
        }

        protected virtual void SetupPacksList()
        {
            if (_packsInfos.IsCollectionNullOrEmpty())
            {
                LoggerService.LogWarning(this, $"[{nameof(SetupPacksList)}]: {TargetPackType} packs collection is empty");
            }

            if (_gridViewAdapter == null && _loopGridView != null)
            {
                _gridViewAdapter = new GridViewAdapter(_loopGridView);
                _gridViewAdapter.InitScroll(GetMemberItems(_packsInfos));
            }

            _gridInitialized = true;
            _coroutineService.WaitFrame()
                .ContinueWithResolved(() => GridInitializationDoneSignal?.Dispatch());
        }

        protected virtual IList<IListItem> GetMemberItems(IEnumerable<PackInfo> packsInfos)
        {
            var list = packsInfos.ToList();
            return list
                .Select(CreatePackWidgetInfo)
                .Where(info => info != null)
                .Select(CreateMediator)
                .ToList();
        }
        
        protected RectTransform EvaluatePopupAnchorRectFromClickAction(IPackClickAction clickAction)
        {
            if (clickAction.PopupAnchorRect == null)
            {
                LoggerService.LogWarning(this, $"PopupAnchorRect is null at {nameof(EvaluatePopupAnchorRectFromClickAction)}");
                return GetComponent<RectTransform>();
            }
            
            return clickAction.PopupAnchorRect;
        }

        public void Initialize(CurrencyDisplayWidget currencyDisplayWidget, AdsButtonWidget adsButtonWidget)
        {
            _currencyDisplayWidget = currencyDisplayWidget;
            _adsButtonWidget = adsButtonWidget;

            InitializePackButtons();
        }

        public void UpdatePacksState()
        {
            var data = _gridViewAdapter?.GetData();
            if (data == null)
                return;

            foreach (var item in data)
            {
                if (item is IPackItemWidgetMediator mediator)
                {
                    var isUnlocked = _progressProvider.IsPackAvailableForUnlocking(mediator.PackId);
                    mediator.UpdateWidgetUnlock(isUnlocked);
                }
            }
        }

        public void RequestItemsEntranceAnimation()
        {
            _entryAnimationRequested = true;
            TryPlayEntranceAnimation();
        }

        public void PlayExitAnimations()
        {
            if (!_gridInitialized)
                return;

            var data = _gridViewAdapter?.GetData();
            if (data == null)
                return;

            foreach (var item in data)
            {
                if (item is IPackItemWidgetMediator mediator)
                    mediator.PlayExitAnimation();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            GridInitializationDoneSignal.MapListener(TryPlayEntranceAnimation).DisposeWith(this);
        }

        protected BasePackItemWidgetInfo CreatePackWidgetInfo(PackInfo packInfo, int indexInList)
        {
            if (this == null || gameObject == null)
                return null;

            var packId = packInfo.PackId;
            var isUnlocked = _progressProvider.IsPackAvailableForUnlocking(packId);
            var currencyToUnlock = _progressProvider.GetCurrencyToUnlock(packId) ?? new List<ICurrency>();
            System.Func<bool> getEntranceAlreadyTriggered = () => EntranceAnimationsAlreadyTriggered;
            Action<IPackClickAction> onPackClicked = action => OnPackClicked(packId, action);

            return CreatePackWidgetInfoInternal(packInfo, packId, isUnlocked, currencyToUnlock, indexInList, getEntranceAlreadyTriggered, onPackClicked);
        }

        protected void OnAvailablePackClicked(PackInfo packInfo)
        {
            _flowScreenController.GoToChooseLevelScreen(packInfo);
        }
        

        private void TryPlayEntranceAnimation()
        {
            if (!_entryAnimationRequested || !_gridInitialized)
                return;

            _entryAnimationRequested = false;
            PlayEntranceAnimationsInternal();
        }

        private void PlayEntranceAnimationsInternal()
        {
            var mediators = _gridViewAdapter?.GetData()?.OfType<IPackItemWidgetMediator>().ToList();
            if (mediators == null || mediators.Count == 0) return;

            foreach (var mediator in mediators)
            {
                mediator.RequestEntranceAnimation();
            }

            EntranceAnimationsAlreadyTriggered = true;
        }
    }
}
