using System;
using RSG;

namespace Handlers.UISystem.Screens.Transitions
{
    public class UIScreenTransition
    {
        public Type ToScreenType => _toScreenType;
        public IScreenContext Context => _context;
        public bool DownloadPrefabWithProgress { get; private set; }
        
        private readonly Type _toScreenType;
        private readonly IScreenContext _context;

        protected UIScreenTransition(Type toScreenType, IScreenContext context, bool downloadPrefabWithProgress)
        {
            _toScreenType = toScreenType;
            _context = context;
            DownloadPrefabWithProgress = downloadPrefabWithProgress;
        }
        
        public virtual IPromise PrepareBeforeTransition(bool forward)
        {
            return Promise.Resolved();
        }
        
        public virtual IPromise EnsureContent(bool forward)
        {
            return Promise.Resolved();
        }

        public virtual void OnTransitionEnded(bool forward)
        {
            
        }
    }
}