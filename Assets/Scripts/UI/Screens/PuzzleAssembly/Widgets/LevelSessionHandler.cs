using System;
using System.Linq;
using Common.Data.Info;
using DG.Tweening;
using Extensions;
using Handlers;
using Installers;
using Level.FinishLevelSequence;
using Level.Hud;
using Level.Widgets;
using RSG;
using Services;
using Services.CoroutineServices;
using Services.ScreenBlocker;
using Storage;
using Storage.Snapshots.LevelParams;
using UI.Popups.CompleteLevelInfoPopup;
using UI.Screens.PuzzleAssembly.Figures;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.PuzzleAssembly.Widgets
{
    public class LevelSessionHandler : InjectableMonoBehaviour
    {
        [Inject] private readonly LevelsParamsStorageData _levelsParamsStorageData;
        [Inject] private readonly LevelHelperService _levelHelperService;
        [Inject] private readonly LevelInfoTrackerService _levelInfoTrackerService;
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly ProgressProvider _progressProvider;
        
        [SerializeField] private PuzzlesWidget _puzzlesWidget;
        [SerializeField] private PuzzleDraggingWidget _puzzleDraggingWidget;
        [SerializeField] private ParticleSystem _finishLevelParticles;
        [SerializeField] private ScrollRect _scrollRect;

        private LevelParamsSnapshot _levelParamsSnapshot;
        private string _levelTrackingId;
        private int _packId;
        private int _levelId;

        public void Initialize(LevelParamsSnapshot levelParamsSnapshot, int packId, int levelId)
        {
            _packId = packId;
            _levelId = levelId;
            _levelTrackingId = LevelTrackingExtensions.GetLevelTrackingId(packId, levelId);
            _levelParamsSnapshot = levelParamsSnapshot;
            
            _soundHandler.PlaySound("start");
            
            StartLevelTracking();
            InitializePuzzles(packId, levelId);

            _puzzleDraggingWidget.CheckLevelCompletionSignal.MapListener(TryHandleLevelCompletion).DisposeWith(this);
        }

        private void TryHandleLevelCompletion()
        {
            if (!_levelHelperService.IsLevelCompeted(_levelParamsSnapshot))
                return;

            var levelPlayedTime = _levelInfoTrackerService.CurrentLevelPlayingTime;
            StopLevelTracking();

            var hasLevelBeenCompletedBefore = _progressProvider.HasLevelBeenCompletedBefore(_packId, _levelId);
            var levelStatus = _levelHelperService.GetCompletedLevelStatus(hasLevelBeenCompletedBefore);
            
            var maybeOldEarnedStars = _progressProvider.GetEarnedStarsForLevel(_packId, _levelId);
            var initialStarsForLevel = maybeOldEarnedStars ?? 0;
            var earnedStarsForLevel = _levelHelperService.EvaluateEarnedStars(_levelParamsSnapshot, levelPlayedTime);

            PlayFinishParticles();
            
            StartLevelCompletionSequence(_packId, _levelId, earnedStarsForLevel, initialStarsForLevel, levelPlayedTime, levelStatus);
        }

        private void StartLevelCompletionSequence(int packId, int levelId, int currentStarsForLevel, int initialStarsForLevel, float levelPlayedTime, CompletedLevelStatus completedLevelStatus)
        {
            StateMachine
                .CreateMachine(new FinishLevelContext(packId, levelId, currentStarsForLevel, initialStarsForLevel, levelPlayedTime, _packInfo, completedLevelStatus))
                .StartSequence<ShowCompletePopupState>()
                .FinishWith(this);
        }

        
        
        private void PlayFinishParticles()
        {
            if (_finishLevelParticles.isPlaying)
                _finishLevelParticles.Stop();

            _finishLevelParticles.Simulate(0);
            _finishLevelParticles.Play();
        }
        
        public void LockScroll(bool value)
        {
            _scrollRect.horizontal = !value;
        }

        private void InitializePuzzles(int packId, int levelId)
        {
            var figuresTargetList = _levelsParamsStorageData.GetTargetFigures(packId, levelId);
            var figuresMenuList = _levelsParamsStorageData.GetMenuFigures(packId, levelId);
            
            _puzzlesWidget.Initialize(figuresTargetList, figuresMenuList);
        }
        
        private void StartLevelTracking()
        {
            _levelInfoTrackerService.StartLevelTracking(_levelTrackingId);
        }

        private void StopLevelTracking()
        {
            _levelInfoTrackerService.StopLevelTracking();
            _levelInfoTrackerService.ClearData();
        }
    }
}