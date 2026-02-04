using ThirdParty.SuperScrollView.Scripts.ListView;
using UnityEngine;
using Utilities.Disposable;

namespace ThirdParty.SuperScrollView.Scripts.List
{
    public abstract class ListItemMediator<TView, TData> : ItemMediatorBase<TData, TView>, IListItem, IOrderableItem
        where TView : ItemViewBase
    {
        public int Priority { get; }
        public int GlobalId { get; }
        
        protected ListItemMediator(TData data, int priority = 0)
        {
            GlobalId = MediatorsIdGenerator.Next();
            Data = data;
            Priority = priority;
        }

        public void Setup(ItemViewBase view, bool isVisible = false)
        {
            view.Inject();
            SetInfo((TView)view, Data, isVisible);
        }

        public bool CanUsePrefab(GameObject prefab)
        {
            return prefab != null && prefab.GetComponent<TView>() != null;
        }

        public void ForceViewRelease(ListItemReleaseType type = ListItemReleaseType.Default)
        {
            ForceReset(type);
        }

        private ListItemWatcher _watcher;
        
        public void AttachWatcher(ListItemWatcher watcher)
        {
            _watcher = watcher;
            _watcher.Watch(this);
        }
        
        public void MediatorRelease()
        {
            _watcher?.Unwatch();
            _watcher = null;
            DisposeService.HandledDispose(this);
        }

        public DisposableCollection ChildDisposables { get; } = new DisposableCollection();
    }
}