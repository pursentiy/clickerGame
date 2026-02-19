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
using Services;
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
        
        [SerializeField] protected LoopGridView _loopGridView;
        
        protected CurrencyDisplayWidget _currencyDisplayWidget;
        protected AdsButtonWidget _adsButtonWidget;
        protected GridViewAdapter _gridViewAdapter;
        protected IReadOnlyCollection<PackInfo> _packsInfos;

        protected abstract PackType TargetPackType { get; }

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
        }
        
        protected virtual IList<IListItem> GetMemberItems(IEnumerable<PackInfo> packsInfos)
        {
            return packsInfos.Select(CreatePackWidgetInfo)
                .Where(info => info != null)
                .Select(CreateMediator)
                .ToList();
        }
        
        protected abstract IListItem CreateMediator(BasePackItemWidgetInfo info);

        protected BasePackItemWidgetInfo CreatePackWidgetInfo(PackInfo packInfo)
        {
            if (this == null || gameObject == null)
                return null;

            var packId = packInfo.PackId;
            var isUnlocked = _progressProvider.IsPackAvailable(packId);
            var maybeStarsRequired = _progressProvider.GetStarsCountForPackUnlocking(packId);
            var starsRequired = maybeStarsRequired ?? new Stars(0);
                
            return CreatePackWidgetInfoInternal(packInfo, packId, isUnlocked, starsRequired);
        }

        protected abstract BasePackItemWidgetInfo CreatePackWidgetInfoInternal(PackInfo packInfo, int packId, bool isUnlocked, Stars starsRequired);

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
