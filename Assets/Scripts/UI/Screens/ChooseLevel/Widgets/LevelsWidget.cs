using System.Collections.Generic;
using System.Linq;
using Common.Data.Info;
using Controllers;
using Extensions;
using Installers;
using Services;
using Storage.Extensions;
using Storage.Snapshots.LevelParams;
using ThirdParty.SuperScrollView.Scripts.GridView;
using ThirdParty.SuperScrollView.Scripts.List;
using UI.Screens.ChooseLevel.LevelItem;
using UnityEngine;
using Zenject;

namespace UI.Screens.ChooseLevel.Widgets
{
    public class LevelsWidget : InjectableMonoBehaviour
    {
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly FlowScreenController _flowScreenController;
        
        [SerializeField] private LoopGridView _loopGridView;

        private int _packId;
        private GridViewAdapter _gridViewAdapter;
        
        public void Initialize(int packId, IReadOnlyCollection<LevelInfo> levelInfos)
        {
            _packId =  packId;
            
            InitializeLevelsItems(levelInfos);
        }
        
        private void InitializeLevelsItems(IReadOnlyCollection<LevelInfo> levelInfos)
        {
            if (levelInfos.IsCollectionNullOrEmpty())
            {
                LoggerService.LogError(this, $"[{nameof(InitializeLevelsItems)}]: {nameof(LevelInfo)} levels params are null or empty.");
                return;
            }

            SetupLevelsList(levelInfos);
        }
        
        private void SetupLevelsList(IReadOnlyCollection<LevelInfo> levelInfos)
        {
            if (levelInfos.IsCollectionNullOrEmpty())
            {
                LoggerService.LogWarning(this, $"[{nameof(SetupLevelsList)}]: {nameof(IReadOnlyCollection<PackInfo>)} is empty");
            }
            if (_gridViewAdapter == null)
            {
                _gridViewAdapter = new GridViewAdapter(_loopGridView);
                _gridViewAdapter.InitScroll(GetMemberItems(levelInfos));
            }
            else
            {
                var items = GetMemberItems(levelInfos);
                _gridViewAdapter.SetData(items);
                _gridViewAdapter.LoopGridView.SetListItemCount(items.Count);
                _gridViewAdapter.LoopGridView.RefreshAllShownItem();
            }
        }
        
        private IList<IListItem> GetMemberItems(IEnumerable<LevelInfo> packsInfos)
        {
            return packsInfos.Select(GetLevelInfo)
                .Where(info => info != null).Select(info => new LevelItemWidgetMediator(info)).ToList<IListItem>();
        }
        
        private LevelItemWidgetInfo GetLevelInfo(LevelInfo levelInfo)
        {
            var earnedStarsForLevel = _progressProvider.GetEarnedStarsForLevel(_packId, levelInfo.LevelId) ?? 0;
            var isLevelAvailable = _progressProvider.IsLevelAvailableToPlay(_packId, levelInfo.LevelId);
            
            return new LevelItemWidgetInfo(
                levelInfo.LevelName, 
                levelInfo.LevelImage, 
                earnedStarsForLevel, 
                levelInfo.LevelDifficulty, 
                isLevelAvailable, 
                () => StartLevel(levelInfo.LevelId));
        }
        
        private void StartLevel(int levelId)
        {
            var levelParamsSnapshot = _progressProvider.GetLevelInfo(_packId, levelId)?.ToSnapshot();
            if (levelParamsSnapshot == null)
            {
                LoggerService.LogWarning(this, $"Cannot start level {levelId} as {nameof(LevelParamsSnapshot)} is null");
                return;
            }

            _flowScreenController.GoToPuzzleAssemblyScreen(levelParamsSnapshot, _packId);
        }
    }
}