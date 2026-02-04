using System.Collections.Generic;
using Extensions;
using Handlers.UISystem;
using ThirdParty.SuperScrollView.Scripts.GridView;
using ThirdParty.SuperScrollView.Scripts.ListView;
using Utilities.Disposable;

namespace ThirdParty.SuperScrollView.Scripts.List
{
    public static class LoopListView2Extensions
    {
        public static LoopListViewItem2 GetListViewItem(this LoopListView2 list, string itemPrefabName)
        {
            var listViewItem = list.NewListViewItem(itemPrefabName);

            if (listViewItem != null)
            {
                if (!listViewItem.IsInitHandlerCalled)
                {
                    listViewItem.IsInitHandlerCalled = true;
                }
            }

            return listViewItem;
        }
        
        public static LoopGridViewItem GetGridViewItem(this LoopGridView list, string itemPrefabName)
        {
            var listViewItem = list.NewListViewItem(itemPrefabName);

            if (listViewItem != null)
            {
                if (!listViewItem.IsInitHandlerCalled)
                {
                    listViewItem.IsInitHandlerCalled = true;
                }
            }

            return listViewItem;
        }

        public static ListViewAdapter CreateAdapter(this LoopListView2 listView, IList<IListItem> list, bool loopItems = false, LoopListViewInitParam viewInitParam = null)
        {
            var adapter = new ListViewAdapter(listView);
            adapter.InitScroll(list, loopItems, viewInitParam);
            return adapter;
        }
        
        public static void StopRefreshingListOnPopupBeginHide(this LoopListView2 loopList, UIPopupBase popup)
        {
            if (popup == null || loopList == null)
                return;
            
            popup.OnBeginHideSignal.MapListener(loopList.StopRefreshingListOnUpdate).DisposeWith(popup);
        }

        public static LoopListViewInitParam GetInitParams()
        {
            var initParams = LoopListViewInitParam.CopyDefaultInitParam();
            initParams.mDistanceForNew0 *= 4;
            initParams.mDistanceForNew1 *= 4;
            initParams.mDistanceForRecycle0 *= 4;
            initParams.mDistanceForRecycle1 *= 4;

            return initParams;
        }
    }
}