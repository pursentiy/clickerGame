using Installers;
using ThirdParty.SuperScrollView.Scripts.ListView;
using UnityEngine;
using Utilities;
using Utilities.Disposable;

namespace ThirdParty.SuperScrollView.Scripts.List
{
    public interface IListItem : IDisposeProvider
    {
        void Setup(ItemViewBase view, bool isVisible = false);
        bool CanUsePrefab(GameObject prefab);
        /// <summary>
        /// View release
        /// </summary>
        void ForceViewRelease(ListItemReleaseType type = ListItemReleaseType.Default);
        /// <summary>
        /// Mediator release e.g the whole list is destroyed or mediators list replaced
        /// </summary>
        void MediatorRelease();
    }
    
    public interface IListMediatorOwner
    {
        void Remove(IListItem banner);
    }

    public abstract class ListItemWatcher : IDisposeProvider
    {
        protected IListMediatorOwner Owner { get; }
        
        protected IListItem Item { get; private set; }
        
        protected ListItemWatcher(IListMediatorOwner owner)
        {
            Owner = owner;
            ContainerHolder.CurrentContainer.Inject(this);
        }
        
        public virtual void Watch(IListItem item)
        {
            Item = item;
        }

        public virtual void Unwatch()
        {
            Item = default;
            DisposeService.HandledDispose(this);
        }

        public DisposableCollection ChildDisposables { get; } = new DisposableCollection();
    }
}