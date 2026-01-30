using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Currency;
using Common.Data.Info;
using Components.UI;
using Extensions;
using Handlers.UISystem;
using Handlers.UISystem.Screens.Transitions;
using Installers;
using Services;
using ThirdParty.SuperScrollView.Scripts.GridView;
using ThirdParty.SuperScrollView.Scripts.List;
using ThirdParty.SuperScrollView.Scripts.ListView;
using UI.Screens.ChooseLevel;
using UI.Screens.ChoosePack.NoCurrencySequence;
using UI.Screens.ChoosePack.PackLevelItem;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack.Widgets
{
    public class PacksWidget : InjectableMonoBehaviour
    {
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly ProgressController _progressController;
        [Inject] private readonly UIManager _uiManager;
        
        [SerializeField] private PackItemWidget _packItemWidgetPrefab;
        [SerializeField] private LoopGridView _loopGridView;
        [SerializeField] private RectTransform _packsContainer;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroupPrefab;
        [Range(1, 5)]
        [SerializeField] private int _rowPacksCount = 2;
        
        private List<HorizontalLayoutGroup> _horizontalGroups = new();
        private List<PackItemWidget> _packItems = new();
        private IReadOnlyCollection<PackInfo> _allPacksInfos;
        private CurrencyDisplayWidget _currencyDisplayWidget;
        private AdsButtonWidget _adsButtonWidget;
        private ListViewAdapter _listViewAdapter;

        public void Initialize(CurrencyDisplayWidget currencyDisplayWidget, AdsButtonWidget adsButtonWidget)
        {
            _currencyDisplayWidget =  currencyDisplayWidget;
            _adsButtonWidget =  adsButtonWidget;
            
            InitializePackButtons();
        }
        
        public void UpdatePacksState()
        {
            if (_packItems.IsCollectionNullOrEmpty())
                return;

            foreach (var packItemWidget in _packItems)
            {
                if (packItemWidget == null)
                {
                    LoggerService.LogWarning(this, $"[{nameof(UpdatePacksState)}]: {nameof(PackItemWidget)} is null");
                    continue;
                }
                
                var packId = packItemWidget.PackId;
                var packInfo = _progressProvider.GetPackInfo(packId);
                if (packInfo == null)
                {
                    LoggerService.LogWarning(this, $"[{nameof(UpdatePacksState)}]: {nameof(PackInfo)} is null for pack id {packId}");
                    continue;
                }
                
                var isUnlocked = _progressProvider.IsPackAvailable(packId);
                var maybeStarsRequired = _progressProvider.GetStarsCountForPackUnlocking(packId);
                if (!maybeStarsRequired.HasValue)
                {
                    LoggerService.LogWarning(this, $"[{nameof(UpdatePacksState)}]: cannot get stars for unlocking pack with id {packId}");
                }
                
                var starsRequired = maybeStarsRequired ?? new Stars(0);
                packItemWidget.UpdateState(isUnlocked, () => OnAvailablePackClicked(packInfo), OnUnavailablePackClicked, starsRequired);
            }
        }
        
        private void InitializePackButtons()
        {
            if (_packItemWidgetPrefab == null)
            {
                LoggerService.LogWarning(this, $"[{nameof(InitializePackButtons)}]: {nameof(PackItemWidget)} is null");
                return;
            }
            
            _allPacksInfos = _progressProvider.GetAllPacks();
            if (_allPacksInfos.IsCollectionNullOrEmpty())
            {
                LoggerService.LogError(this, $"[{nameof(InitializePackButtons)}]: {nameof(ProgressProvider)} packs params are null or empty.");
                return;
            }

            SetupPacksList();
        }
        
        private IEnumerator InitializePacksRoutine(PackItemWidget packItemWidgetPrefab, RectTransform packsContainer, IEnumerable<PackInfo> packsInfos)
        {
            HorizontalLayoutGroup oldHorizontalLayoutGroup = null;
            var index = 0;

            foreach (var packInfo in packsInfos)
            {
                if (this == null || gameObject == null)
                    yield break;

                var horizontalLayoutGroup = TryInstantiateHorizontalLayoutGroup(oldHorizontalLayoutGroup, packsContainer, index);
                oldHorizontalLayoutGroup = horizontalLayoutGroup;

                var packId = packInfo.PackId;
                var packItemWidget = Instantiate(packItemWidgetPrefab, horizontalLayoutGroup.transform);
                _packItems.Add(packItemWidget);
        
                var isUnlocked = _progressProvider.IsPackAvailable(packId);
                var maybeStarsRequired = _progressProvider.GetStarsCountForPackUnlocking(packId);
                var starsRequired = maybeStarsRequired ?? new Stars(0);
                
                packItemWidget.Initialize(packInfo.PackName, packInfo.PackImagePrefab, packId, isUnlocked,
                    () => OnAvailablePackClicked(packInfo), OnUnavailablePackClicked, starsRequired);
        
                index++;

                yield return null;
            }
        }
        
        private HorizontalLayoutGroup TryInstantiateHorizontalLayoutGroup(HorizontalLayoutGroup maybeHorizontalLayoutGroup, RectTransform packsContainer, int itemIndex)
        {
            if (maybeHorizontalLayoutGroup == null || itemIndex % _rowPacksCount == 0)
            {
                var group = Instantiate(_horizontalLayoutGroupPrefab, _packsContainer);
                _horizontalGroups.Add(group);
                return group;
            }

            return maybeHorizontalLayoutGroup;
        }
        
        private void OnAvailablePackClicked(PackInfo packInfo)
        {
            _progressController.SetCurrentPackId(packInfo.PackId);
            var context = new ChooseLevelScreenContext(packInfo);
            _uiManager.ScreensHandler.PushScreen(new FadeScreenTransition(typeof(ChooseLevelScreenMediator), context));
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
        
        private void SetupPacksList()
        {
            if (_allPacksInfos.IsCollectionNullOrEmpty())
            {
                LoggerService.LogWarning(this, $"[{nameof(SetupPacksList)}]: {nameof(IReadOnlyCollection<PackInfo>)} is empty");
            }
            if (_listViewAdapter == null)
            {
                _listViewAdapter = new ListViewAdapter(_loopGridView);
                _listViewAdapter.InitScroll(GetMemberItems(), initParams: LoopListView2Extensions.GetInitParams());
            }
            else
            {
                var items = GetMemberItems();
                _listViewAdapter.SetData(items);
                _listViewAdapter.LoopListView.SetListItemCount(items.Count);
                _listViewAdapter.LoopListView.RefreshAllShownItem();
                _listViewAdapter.LoopListView.StopRefreshingListOnPopupBeginHide(this);
            }
        }
        
        private IList<IListItem> GetMemberItems(IEnumerable<PackInfo> packsInfos)
        {
            return packsInfos.Select(GetPackWidgetInfo)
                .Where(info => info != null).Select(info => new PackItemWidgetMediator(info)).ToList<IListItem>();
        }
        
        private PackItemWidgetInfo GetPackWidgetInfo(PackInfo packInfo)
        {
            if (this == null || gameObject == null)
                return null;

            var packId = packInfo.PackId;
            var isUnlocked = _progressProvider.IsPackAvailable(packId);
            var maybeStarsRequired = _progressProvider.GetStarsCountForPackUnlocking(packId);
            var starsRequired = maybeStarsRequired ?? new Stars(0);
                
            return new PackItemWidgetInfo(packInfo.PackName, packInfo.PackImagePrefab, packId, isUnlocked,
                () => OnAvailablePackClicked(packInfo), OnUnavailablePackClicked, starsRequired);
        }
    }
}