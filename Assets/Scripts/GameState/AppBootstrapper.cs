using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Handlers;
using Handlers.UISystem;
using Installers;
using Playgama;
using Services;
using Storage;
using Storage.Levels;
using Storage.Snapshots;
using Zenject;
using ScreenHandler = Handlers.ScreenHandler;

namespace GameState
{
    public class AppBootstrapper : InjectableMonoBehaviour
    {
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly PlayerProgressService _playerProgressService;
        [Inject] private readonly SoundHandler _soundHandler;
        [Inject] private readonly ProfileBuilderService _profileBuilderService;
        [Inject] private readonly PlayerRepositoryService _playerRepositoryService;
        [Inject] private readonly PlayerService _playerService;
        [Inject] private readonly ApplicationService _applicationService;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly GlobalSettingsService _globalSettingsService;
        [Inject] private readonly LevelsParamsStorageData _levelsParamsStorage;
        [Inject] private readonly LocalizationService _localizationService;

        protected override void Awake()
        {
            DOTween.Init(true, true, LogBehaviour.Default);

            _applicationService.RegisterDisposableService(_uiManager);
            _applicationService.RegisterDisposableService<TimeService>();
            _applicationService.RegisterDisposableService<PersistentCoroutinesService>();
            _applicationService.RegisterDisposableService<CoroutineService>();
            _applicationService.SetApplicationInitialized();

            LoggerService.LogWarning($"[{nameof(AppBootstrapper)}] Awake finished");
        }

        private bool _isBridgeAuthComplete = false;

        private IEnumerator Start()
        {
            // 1. Ждем инициализацию Bridge и авторизацию игрока
            yield return BridgeAuthenticationRoutine();

            // 2. Ждем загрузку локализации
            LoggerService.LogWarning($"[{nameof(AppBootstrapper)}] Start: Waiting for Localization...");
            yield return _localizationService.InitializeRoutine();

            // Проверка на выживание объекта после долгих ожиданий
            if (this == null) yield break;

            if (!_localizationService.IsInitialized)
            {
                LoggerService.LogError($"[{nameof(AppBootstrapper)}] Localization failed. Stopping.");
                yield break;
            }

            // 3. Запуск основной логики игры
            FinalizeSetup();
        }

        private IEnumerator BridgeAuthenticationRoutine()
        {
            LoggerService.LogWarning("[Bridge] Определение платформы...");

            // Ждем, пока SDK поймет, где он запущен
            while (Bridge.platform.id == "unknown")
            {
                yield return null;
            }

            LoggerService.LogWarning($"[Bridge] Платформа: {Bridge.platform.id}");

            // Если платформа поддерживает авторизацию — запрашиваем её
            if (Bridge.platform.id == "yandex" || Bridge.platform.id == "vk" || Bridge.platform.id == "facebook")
            {
                Bridge.player.Authorize(null, success =>
                {
                    if (success) LoggerService.LogWarning("[Bridge] Игрок авторизован");
                    else LoggerService.LogWarning("[Bridge] Вход как гость");

                    _isBridgeAuthComplete = true;
                });

                // Ждем, пока сработает callback авторизации
                while (!_isBridgeAuthComplete)
                {
                    yield return null;
                }
            }
            else
            {
                LoggerService.LogWarning("[Bridge] Авторизация не требуется для этой платформы.");
            }
        }

        private void FinalizeSetup()
        {
            LoggerService.LogWarning($"[{nameof(AppBootstrapper)}] Proceeding with UI Setup...");

            _uiManager.ShowScreensUI();
            _uiManager.SetupHandlers();
            _globalSettingsService.InitializeProfileSettings();

            SetupSounds();
            SetupProfile(); // Здесь уже можно вызывать LoadProfileRecord через Bridge
            _screenHandler.ShowWelcomeScreen(true);

            LoggerService.LogWarning($"[{nameof(AppBootstrapper)}] Successfully entered game.");
        }

        private void SetupProfile()
        {
            _playerRepositoryService.LoadPlayerSnapshot(OnPlayerSnapshotLoaded);

            void OnPlayerSnapshotLoaded(ProfileSnapshot maybeSnapshot)
            {
                if (maybeSnapshot == null)
                {
                    StartNewProfileSession();
                }
                else
                {
                    StartOldProfileSession(maybeSnapshot);
                }
            }
        }

        private void StartNewProfileSession()
        {
            var playerSnapshot = _profileBuilderService.BuildNewProfileSnapshot();
            _playerService.Initialize(playerSnapshot);
            _playerRepositoryService.SavePlayerSnapshot(playerSnapshot);

            _playerProgressService.InitializeHandler(_levelsParamsStorage.DefaultPacksParamsList);
        }

        private void StartOldProfileSession(ProfileSnapshot profileSnapshot)
        {
            _playerService.Initialize(profileSnapshot);
            _playerProgressService.InitializeHandler(_levelsParamsStorage.DefaultPacksParamsList);
        }

        private void SetupSounds()
        {
            _soundHandler.SetMusicVolume(_globalSettingsService.ProfileSettingsMusic);
            _soundHandler.SetSoundVolume(_globalSettingsService.ProfileSettingsSound);
            _soundHandler.StartAmbience();
        }
    }
}