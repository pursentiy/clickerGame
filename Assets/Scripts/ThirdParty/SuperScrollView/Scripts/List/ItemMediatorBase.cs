using System.Diagnostics;
using Installers;
using Services;
using ThirdParty.SuperScrollView.Scripts.ListView;

namespace ThirdParty.SuperScrollView.Scripts.List
{
    public abstract class ItemMediatorBase<TData, TView> : IItemMediator where TView : class, IItemView
    {
        protected TData Data { get; set; }
        public TView View { get; private set; }
        
        private readonly int _localId;
        
        private readonly LoggerUI _logger = LoggerUI.Get();

        protected ItemMediatorBase()
        {
            _localId = ItemMediatorId.GlobalId++;
            ContainerHolder.CurrentContainer.Inject(this);
        }

        public void SetData(TData data)
        {
            Data = data;
        }
        
        public void SetInfo(TView view, TData data, bool isVisibleOnRefresh = false)
        {
            _logger.Debug("Set Info: " + GetDescription() + " with " + view.GetDescription());
            
            if (View != null)
            {
                LoggerService.LogWarning("Set info is called while previous View is not released: " + GetDescription() + " with " 
                           + (View != null ? View.GetDescription() : "null") + " for " + view.GetDescription());
                ForceReset();
            }
            
            View = view;
            View.OnRelease += OnViewRelease;

            Data = data;
            
            OnInitialize(isVisibleOnRefresh);
        }

        public TData GetData()
        {
            return Data;
        }

        private void OnViewRelease(ListItemReleaseType type)
        {
            _logger.Debug("View Release: " + GetDescription() + " with " + (View != null ? View.GetDescription() : "null"));
            
            OnRelease(type);
            View = null;
        }

        protected virtual void OnInitialize(bool isVisibleOnRefresh)
        {
            
        }

        protected virtual void OnRelease(ListItemReleaseType type = ListItemReleaseType.Default)
        {
            
        }

        protected void ForceReset(ListItemReleaseType type = ListItemReleaseType.Default)
        {
            _logger.Debug("Force Reset: " + GetDescription() + " with " + (View != null ? View.GetDescription() : "null"));

            View?.Release(type);
        }

        public string GetDescription()
        {
            return $"{GetType().Name}:{_localId}";
        }
    }
    
    public static class ItemMediatorId
    {
        public static int GlobalId { get; set; }
    }

    public class LoggerUI
    {
        private static LoggerUI _instance;
        
        public static LoggerUI Get()
        {
            return _instance ?? (_instance = new LoggerUI());
        }
        
        [Conditional("LOGS_UI")]
        public void Debug(string message)
        {
            LoggerService.LogDebug(message);
        }
    }
    
    public interface IItemMediator
    {
        
    }
}