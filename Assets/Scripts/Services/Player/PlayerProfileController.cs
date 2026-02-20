using System;
using System.Collections.Generic;
using System.Linq;
using Common.Currency;
using Extensions;
using Plugins.FSignal;
using RSG;
using Services.Base;
using Services.CoroutineServices;
using Storage.Snapshots;
using Utilities.Disposable;
using Zenject;

namespace Services.Player
{
    public class PlayerProfileController : DisposableService
    {
        [Inject] private readonly PlayerRepositoryService _playerRepositoryService;
        [Inject] private readonly PersistentCoroutinesService _coroutinesService;
        
        private ProfileSnapshot _profileSnapshot { get; set; }
        private IPromise _saveDelayPromise;
        private const float SaveDelay = 2.0f;
        private Dictionary<Type, (Func<ICurrency> getter, Action<ICurrency> setter)> _snapshotCurrencyMap;
        
        public bool IsInitialized { get; private set; }
        public FSignal ProfileSnapshotInitializedSignal { get; private set; } = new();
        public IReadOnlyList<PackSnapshot> PacksSnapshot => _profileSnapshot?.PackSnapshots;
        public GameParamsSnapshot TryGetGameParamsSnapshot() => _profileSnapshot?.GameParamsSnapshot;
        public DailyRewardSnapshot TryGetDailyRewardSnapshot() => _profileSnapshot?.DailyRewardSnapshot;
        public Stars Stars => _profileSnapshot?.Stars ?? Stars.Zero;
        public SoftCurrency SoftCurrency => _profileSnapshot?.SoftCurrency ?? SoftCurrency.Zero;
        public HardCurrency HardCurrency => _profileSnapshot?.HardCurrency ?? HardCurrency.Zero;
        
        public bool TryGetCurrency(Type type, out ICurrency currency)
        {
            if (!TryGetCurrencyAccessors(type, out var accessors))
            {
                currency = null;
                return false;
            }

            currency = accessors.getter();
            return true;
        }
        
        public bool UpdateCurrencyAndSave(ICurrency currencyChange, out ICurrency newValue)
        {
            newValue = null;

            if (_profileSnapshot == null || !TryGetCurrencyAccessors(currencyChange.GetType(), out var accessors))
                return false;

            var current = accessors.getter();
            
            if (!InternalCanSpend(current, currencyChange))
            {
                newValue = current;
                return false;
            }
            
            newValue = current.Add(currencyChange.GetCount());
            accessors.setter(newValue);

            SaveProfile(SavePriority.ImmediateSave);
            return true;
        }
        
        public void UpdateDailyRewardAndSave(DailyRewardSnapshot dailyRewardSnapshot, SavePriority savePriority)
        {
            if (_profileSnapshot == null || dailyRewardSnapshot == null)
                return;

            _profileSnapshot.DailyRewardSnapshot = dailyRewardSnapshot;
            SaveProfile(savePriority);
        }
        
        public PackSnapshot TryGetPackSnapshot(int packId)
        {
            return _profileSnapshot?.PackSnapshots.FirstOrDefault(p => p.PackId == packId);
        }
        
        public LevelSnapshot TryGetLevelSnapshot(int packId, int levelId)
        {
            return TryGetPackSnapshot(packId)?.CompletedLevelsSnapshots?.FirstOrDefault(p => p.LevelId == levelId);
        }
        
        public bool CreatePack(PackSnapshot packSnapshot)
        {
            if (_profileSnapshot == null || packSnapshot == null)
                return false;
            
            if (TryGetPackSnapshot(packSnapshot.PackId) != null)
                return false;

            _profileSnapshot.PackSnapshots.Add(packSnapshot);
            return true;
        }
        
        public bool CreateLevel(int packId, LevelSnapshot levelSnapshot, SavePriority savePriority)
        {
            if (_profileSnapshot == null || levelSnapshot == null)
                return false;

            var pack = TryGetPackSnapshot(packId);
            if (pack == null)
                return false;
            
            if (TryGetLevelSnapshot(packId, levelSnapshot.LevelId) != null)
                return false;

            pack.CompletedLevelsSnapshots.Add(levelSnapshot);
            SaveProfile(savePriority);
            return true;
        }
        
        public void Initialize(ProfileSnapshot profileSnapshot)
        {
            if (profileSnapshot == null)
            {
                LoggerService.LogError($"{GetType().Name}.{nameof(Initialize)}: ProfileSnapshot is null");
            }
            
            _profileSnapshot = profileSnapshot;
            InitCurrencyMap();
            IsInitialized = true;
            ProfileSnapshotInitializedSignal.Dispatch();
        }
        
        public void SaveProfile(SavePriority priority = SavePriority.Default)
        {
            if (_profileSnapshot == null)
            {
                LoggerService.LogError(this, $"{nameof(SaveProfile)}: Profile snapshot is null.");
                return;
            }

            if (priority == SavePriority.ImmediateSave)
            {
                ExecuteSave();
            }
            else
            {
                _saveDelayPromise?.SafeCancel();
                _saveDelayPromise = _coroutinesService.WaitForRealtime(SaveDelay).Then(ExecuteSave).CancelWith(this);
            }
        }
        
        public IPromise<ProfileSnapshot> LoadProfile()
        {
            return _playerRepositoryService.LoadPlayerSnapshot();
        }
        
        private void ExecuteSave()
        {
            LoggerService.LogDebug(this, "Executing actual profile save to repository...");

            _playerRepositoryService.SavePlayerSnapshot(_profileSnapshot);
            _saveDelayPromise = null;

            //TODO CATCH AND THEN LOGGING LOGIC
            // .Then(() => LoggerService.LogDebug(this, "Profile successfully saved."))
            // .Catch(e => LoggerService.LogError(this, $"Failed to save profile: {e}"));
        }
        
        private void InitCurrencyMap()
        {
            if (_profileSnapshot == null)
            {
                LoggerService.LogError($"{GetType().Name}.{nameof(InitCurrencyMap)}: ProfileSnapshot is null");
                return;
            }
            
            _snapshotCurrencyMap = new Dictionary<Type, (Func<ICurrency>, Action<ICurrency>)>
            {
                { typeof(Stars), (
                        () => _profileSnapshot.Stars, 
                        (val) => _profileSnapshot.Stars = (Stars)val)
                },
                { typeof(SoftCurrency), (
                        () => _profileSnapshot.SoftCurrency, 
                        (val) => _profileSnapshot.SoftCurrency = (SoftCurrency)val) 
                },
                { typeof(HardCurrency), (
                        () => _profileSnapshot.HardCurrency, 
                        (val) => _profileSnapshot.HardCurrency = (HardCurrency)val) 
                }
            };
        }
        
        private bool TryGetCurrencyAccessors(Type type, out (Func<ICurrency> getter, Action<ICurrency> setter) accessors)
        {
            return _snapshotCurrencyMap.TryGetValue(type, out accessors);
        }
        
        private bool InternalCanSpend(ICurrency current, ICurrency change)
        {
            return change.GetCount() >= 0 || current.GetCount() >= Math.Abs(change.GetCount());
        }

        protected override void OnInitialize()
        {
            
        }

        protected override void OnDisposing()
        {
            
        }
    }
    
    public enum SavePriority
    {
        Default,
        ImmediateSave
    }
}