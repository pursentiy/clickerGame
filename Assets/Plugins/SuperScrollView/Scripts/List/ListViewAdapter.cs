using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Common.Components.List;
using Plugins.SuperScrollView.Scripts.ListView;
using Services;
using SuperScrollView;

namespace Plugins.SuperScrollView.Scripts.List
{
    public class ListViewAdapter
    {
        private bool _loopItems;

        public LoopListView2 LoopListView { get; set; }
        
        private readonly List<IListItem> _data = new List<IListItem>();

        public ListViewAdapter(LoopListView2 loopListView)
        {
            LoopListView = loopListView;
            LoopListView.ObjectDestroyed += OnListViewDestroyed;
        }

        public void ReleaseListView()
        {
            try
            {
                ClearData(ListItemReleaseType.OnDestroy);
            }
            catch (Exception e)
            {
                LoggerService.LogWarning($"{GetType().Name}: {e}");
            }
        }

        private void OnListViewDestroyed()
        {
            ReleaseListView();
        }

        public void InitScroll(IList<IListItem> data, bool loopItems = false, LoopListViewInitParam initParams = null)
        {
            SetData(data);

            _loopItems = loopItems;
            var itemCount = _loopItems ? -1 : _data.Count;
            LoopListView.InitListView(itemCount, OnGetItemByIndex, initParams);
        }

        public void SetData(IList<IListItem> data, bool forceRelease = true)
        {
            if (forceRelease)
                _data.ForEach(x => x.ForceViewRelease());
            
            foreach (var item in _data)
            {
                if (!data.Contains(item) && forceRelease)
                {
                    item.MediatorRelease();
                }
            }
            
            _data.Clear();
            
            _data.AddRange(data);
        }

        private void ClearData(ListItemReleaseType type = ListItemReleaseType.Default)
        {
            try
            {
                _data.ForEach(x => x.ForceViewRelease(type));
            }
            catch (Exception e)
            {
                LoggerService.LogWarning(e.ToString());
            }
            
            _data.ForEach(x => x.MediatorRelease());
            _data.Clear();
        }

        public List<IListItem> GetData()
        {
            return _data;
        }

        private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index, bool visibleItemOnRefresh, ListItemReleaseType releaseType)
        {
            if (!_loopItems && (index < 0 || index >= _data.Count))
            {
                return null;
            }

            var itemData = GetItemDataByIndex(index, _data);

            if (itemData == null)
            {
                return null;
            }           

            var item = listView.GetListViewItem(SelectPrefab(itemData));
            var view = item.GetComponent<ItemViewBase>();
            
            try
            {
                view.Release(releaseType);
            }
            catch (Exception e)
            {
                LoggerService.LogError($"{nameof(view)}.{nameof(view.Release)}: {e}");
            }           

            try
            {
                itemData.ForceViewRelease();
            }
            catch (Exception e)
            {
                LoggerService.LogError($"{nameof(itemData)}.{nameof(itemData.ForceViewRelease)}: {e}");
            }

            try
            {
                itemData.Setup(view, visibleItemOnRefresh);
            }
            catch (Exception e)
            {
                LoggerService.LogError($"{nameof(itemData)}.{nameof(itemData.Setup)}: {e}");
            }
            
            return item;
        }

        private IListItem GetItemDataByIndex(int index, IReadOnlyList<IListItem> data)
        {
            if (!_loopItems && (index < 0 || index >= _data.Count))
            {
                return default;
            }

            if (index < 0)
            {
                var newIndex = _data.Count + ((index + 1) % _data.Count) - 1;
                return data[newIndex];
            }

            if (index >= data.Count)
            {
                var newIndex = index % _data.Count;
                return data[newIndex];
            }

            return data[index];
        }

        public IListItem GetItemByIndex(int index) => GetItemDataByIndex(index, _data);
        
        private string SelectPrefab(IListItem data)
        {
            var prefab = LoopListView.ItemPrefabDataList.FirstOrDefault(x => data.CanUsePrefab(x.mItemPrefab));

            if (prefab == null)
            {
                LoggerService.LogWarning($"Unable to find the prefab for {data.GetType()}");
                return string.Empty;
            }

            return prefab.mItemPrefab.name;
        }
    }
}