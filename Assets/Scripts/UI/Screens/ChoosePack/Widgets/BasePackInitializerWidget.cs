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
using Services;
using Services.CoroutineServices;
using ThirdParty.SuperScrollView.Scripts.GridView;
using ThirdParty.SuperScrollView.Scripts.List;
using UI.Screens.ChoosePack.NoCurrencySequence;
using UI.Screens.ChoosePack.PackLevelItem.Base;
using UnityEngine;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack.Widgets
{
    public abstract class BasePackInitializerWidget : InjectableMonoBehaviour
    {
        [Inject] protected readonly ProgressProvider _progressProvider;
        [Inject] protected readonly FlowScreenController _flowScreenController;
        [Inject] private readonly CoroutineService _coroutineService;
        
        [SerializeField] protected LoopGridView _loopGridView;
        
        protected CurrencyDisplayWidget _currencyDisplayWidget;
        protected AdsButtonWidget _adsButtonWidget;
        protected GridViewAdapter _gridViewAdapter;
        protected IReadOnlyCollection<PackInfo> _packsInfos;

        private bool _gridInitialized;
        private bool _entryAnimationRequested;

        public FSignal GridInitializationDoneSignal { get; } = new();

        protected abstract PackType TargetPackType { get; }
        
        public bool EntranceAnimationsAlreadyTriggered { get; set; }

        protected override void Awake()
        {
            base.Awake();
            
            GridInitializationDoneSignal.MapListener(TryPlayEntranceAnimation).DisposeWith(this);
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
                    var isUnlocked = _progressProvider.IsPackAvailable(mediator.PackId);
                    mediator.UpdateWidgetUnlock(isUnlocked);
                }
            }
        }

        public void RequestItemsEntranceAnimation()
        {
            _entryAnimationRequested = true;
            TryPlayEntranceAnimation();
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
                .Select((pack, index) => CreatePackWidgetInfo(pack, index))
                .Where(info => info != null)
                .Select(CreateMediator)
                .ToList();
        }
        
        protected abstract IListItem CreateMediator(BasePackItemWidgetInfo info);

        protected BasePackItemWidgetInfo CreatePackWidgetInfo(PackInfo packInfo, int indexInList)
        {
            if (this == null || gameObject == null)
                return null;

            var packId = packInfo.PackId;
            var isUnlocked = _progressProvider.IsPackAvailable(packId);
            var maybeStarsRequired = _progressProvider.GetStarsCountForPackUnlocking(packId);
            var starsRequired = maybeStarsRequired ?? new Stars(0);
            System.Func<bool> getEntranceAlreadyTriggered = () => EntranceAnimationsAlreadyTriggered;

            return CreatePackWidgetInfoInternal(packInfo, packId, isUnlocked, starsRequired, indexInList, getEntranceAlreadyTriggered);
        }

        protected abstract BasePackItemWidgetInfo CreatePackWidgetInfoInternal(PackInfo packInfo, int packId, bool isUnlocked, Stars starsRequired, int indexInList, System.Func<bool> getEntranceAnimationsAlreadyTriggered);

        protected void OnAvailablePackClicked(PackInfo packInfo)
        {
            _flowScreenController.GoToChooseLevelScreen(packInfo);
        }
            
        protected void OnUnavailablePackClicked()
        {
            if (_currencyDisplayWidget == null || _adsButtonWidget == null)
            {
                LoggerService.LogWarning(this, $"[{nameof(OnUnavailablePackClicked)}]: {nameof(CurrencyDisplayWidget)} or {nameof(AdsButtonWidget)} is null");
            }
            StateMachine
                .CreateMachine(new VisualizeNotEnoughCurrencyContext(_currencyDisplayWidget, _adsButtonWidget))
                .StartSequence<VisualizeNotEnoughCurrencyState>()
                .FinishWith(this);
        }
    }
}
