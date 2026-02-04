using System;
using System.Collections.Generic;
using System.Linq;
using Services;
using ThirdParty.SuperScrollView.Scripts.GridView;
using ThirdParty.SuperScrollView.Scripts.ListView;

namespace ThirdParty.SuperScrollView.Scripts.List
{
    public partial class GridViewAdapter
    {
        private bool _loopItems;

        public readonly LoopGridView LoopGridView;
        
        private readonly List<IListItem> _data = new List<IListItem>();

        public GridViewAdapter(LoopGridView loopGridView)
        {
            LoopGridView = loopGridView;
            LoopGridView.ObjectDestroyed += OnGridViewDestroyed;
        }       
        
        private void OnGridViewDestroyed()
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

        public void InitScroll(IList<IListItem> data, bool loopItems = false)
        {
            SetData(data);

            _loopItems = loopItems;
            var itemCount = _loopItems ? -1 : _data.Count;
            LoopGridView.InitGridView(itemCount, OnGetItemByIndex);
        }

        public void SetData(IList<IListItem> data)
        {
            _data.ForEach(x => x.ForceViewRelease());
            
            foreach (var item in _data)
            {
                if (!data.Contains(item))
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

        private LoopGridViewItem OnGetItemByIndex(LoopGridView listView, int index, int row, int column)
        {
            LoopGridViewItem item;
            ItemViewBase view;
            IListItem itemData;

            try
            {
                if (!_loopItems && (index < 0 || index >= _data.Count))
                {
                    return null;
                }

                itemData = GetItemDataByIndex(index, _data);

                if (itemData == null)
                {
                    return null;
                }           
            
                item = listView.GetGridViewItem(SelectPrefab(itemData));
                if (item != null)
                    view = item.GetComponent<ItemViewBase>();
                else
                    return null;
            }
            catch (Exception e)
            {
                LoggerService.LogError(e);
                return null;
            }

            try
            {
                if (view != null)
                    view.Release();
            }
            catch (Exception e)
            {
                LoggerService.LogError(e);
            }

            try
            {
                itemData.ForceViewRelease();
            }
            catch (Exception e)
            {
                LoggerService.LogError(e);
            }

            try
            {
                itemData.Setup(view);
            }
            catch (Exception e)
            {
                LoggerService.LogError(e);
            }
            
            return item;
        }
        
        public IReadOnlyCollection<IListItem> GetData()
        {
            return _data;
        }
        
        public IListItem GetItemByIndex(int index) => GetItemDataByIndex(index, _data);

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
        
        private string SelectPrefab(IListItem data)
        {
            var prefab = LoopGridView.ItemPrefabDataList.FirstOrDefault(x => data.CanUsePrefab(x.mItemPrefab));

            if (prefab == null)
            {
                LoggerService.LogWarning($"Unable to find the prefab for {data.GetType()}");
                return string.Empty;
            }

            return prefab.mItemPrefab.name;
        }
    }
}