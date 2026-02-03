using System.Collections.Generic;
using System.Linq;
using Common.Currency;
using Common.Data.Info;
using Components.UI;
using Controllers;
using Extensions;
using Handlers.UISystem;
using Handlers.UISystem.Screens.Transitions;
using Installers;
using Services;
using ThirdParty.SuperScrollView.Scripts.GridView;
using ThirdParty.SuperScrollView.Scripts.List;
using UI.Screens.ChooseLevel;
using UI.Screens.ChoosePack.NoCurrencySequence;
using UI.Screens.ChoosePack.PackLevelItem;
using UnityEngine;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack.Widgets
{
    public class PacksWidget : InjectableMonoBehaviour
    {
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly ProgressController _progressController;
        [Inject] private readonly FlowScreenController _flowScreenController;
        
        [SerializeField] private LoopGridView _loopGridView;
        
        private IReadOnlyCollection<PackInfo> _allPacksInfos;
        private CurrencyDisplayWidget _currencyDisplayWidget;
        private AdsButtonWidget _adsButtonWidget;
        private GridViewAdapter _gridViewAdapter;

        public void Initialize(CurrencyDisplayWidget currencyDisplayWidget, AdsButtonWidget adsButtonWidget)
        {
            _currencyDisplayWidget =  currencyDisplayWidget;
            _adsButtonWidget =  adsButtonWidget;
            
            InitializePackButtons();
        }
        
        public void UpdatePacksState()
        {
            SetupPacksList();
        }
        
        private void InitializePackButtons()
        {
            _allPacksInfos = _progressProvider.GetAllPacks();
            if (_allPacksInfos.IsCollectionNullOrEmpty())
            {
                LoggerService.LogError(this, $"[{nameof(InitializePackButtons)}]: {nameof(ProgressProvider)} packs params are null or empty.");
                return;
            }

            SetupPacksList();
        }
        
        private void SetupPacksList()
        {
            if (_allPacksInfos.IsCollectionNullOrEmpty())
            {
                LoggerService.LogWarning(this, $"[{nameof(SetupPacksList)}]: {nameof(IReadOnlyCollection<PackInfo>)} is empty");
            }
            if (_gridViewAdapter == null)
            {
                _gridViewAdapter = new GridViewAdapter(_loopGridView);
                _gridViewAdapter.InitScroll(GetMemberItems(_allPacksInfos, false));
            }
            else
            {
                var items = GetMemberItems(_allPacksInfos, true);
                _gridViewAdapter.LoopGridView.SetListItemCount(items.Count);
                _gridViewAdapter.SetData(items);
                _gridViewAdapter.LoopGridView.RefreshAllShownItem();
            }
        }
        
        private IList<IListItem> GetMemberItems(IEnumerable<PackInfo> packsInfos, bool shouldAnimate)
        {
            return packsInfos.Select(i => GetPackWidgetInfo(i, shouldAnimate))
                .Where(info => info != null).Select(info => new PackItemWidgetMediator(info)).ToList<IListItem>();
        }
        
        private PackItemWidgetInfo GetPackWidgetInfo(PackInfo packInfo, bool shouldAnimate)
        {
            if (this == null || gameObject == null)
                return null;

            var packId = packInfo.PackId;
            var isUnlocked = _progressProvider.IsPackAvailable(packId);
            var maybeStarsRequired = _progressProvider.GetStarsCountForPackUnlocking(packId);
            var starsRequired = maybeStarsRequired ?? new Stars(0);
                
            return new PackItemWidgetInfo(packInfo.PackName, packInfo.PackImagePrefab, packId, isUnlocked,
                () => OnAvailablePackClicked(packInfo), OnUnavailablePackClicked, starsRequired, shouldAnimate);
        }
        
        private void OnAvailablePackClicked(PackInfo packInfo)
        {
            _flowScreenController.GoToChooseLevelScreen(packInfo);
        }
            
        private void OnUnavailablePackClicked()
        {
            if (_currencyDisplayWidget == null || _adsButtonWidget == null)
            {
                LoggerService.LogWarning(this,  $"[{nameof(OnUnavailablePackClicked)}]: {nameof(CurrencyDisplayWidget)} or {nameof(AdsButtonWidget)} is null");
            }
            StateMachine
                .CreateMachine(new VisualizeNotEnoughCurrencyContext(_currencyDisplayWidget, _adsButtonWidget))
                .StartSequence<VisualizeNotEnoughCurrencyState>()
                .FinishWith(this);
        }
    }
}