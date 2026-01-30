using System;
using System.Collections.Generic;
using Services;
using SuperScrollView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Plugins.SuperScrollView.Scripts.ListView
{
   
    [System.Serializable]
    public class ItemPrefabConfData
    {
        public GameObject mItemPrefab = null;
        public float mPadding = 0;
        public int mInitCreateCount = 0;
        public float mStartPosOffset = 0;
    }


    public class LoopListViewInitParam
    {
        // all the default values
        public float mDistanceForRecycle0 = 300; //mDistanceForRecycle0 should be larger than mDistanceForNew0
        public float mDistanceForNew0 = 200;
        public float mDistanceForRecycle1 = 300;//mDistanceForRecycle1 should be larger than mDistanceForNew1
        public float mDistanceForNew1 = 200;
        public float mSmoothDumpRate = 0.3f;
        public float mSnapFinishThreshold = 0.01f;
        public float mSnapVecThreshold = 145;
        public float mItemDefaultWithPaddingSize = 20;//item's default size (with padding)

        public static LoopListViewInitParam CopyDefaultInitParam()
        {
            return new LoopListViewInitParam();
        }
    }


    public class LoopListView2 : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private class SnapData
        {
            public SnapStatus mSnapStatus = SnapStatus.NoTargetSet;
            public int mSnapTargetIndex = 0;
            public float mTargetSnapVal = 0;
            public float mCurSnapVal = 0;
            public bool mIsForceSnapTo = false;
            public void Clear()
            {
                mSnapStatus = SnapStatus.NoTargetSet;
                mIsForceSnapTo = false;
            }
        }

        public event Action ObjectDestroyed;

        private bool _destroyed;

        private void OnDestroy()
        {
            _destroyed = true;
            
            ObjectDestroyed?.Invoke();
            ObjectDestroyed = null;
        }

        private Dictionary<string, ItemPool> mItemPoolDict = new Dictionary<string, ItemPool>();
        private List<ItemPool> mItemPoolList = new List<ItemPool>();
        [SerializeField] private List<ItemPrefabConfData> mItemPrefabDataList = new List<ItemPrefabConfData>();

        [SerializeField] private ListItemArrangeType mArrangeType = ListItemArrangeType.TopToBottom;
        public ListItemArrangeType ArrangeType { get => mArrangeType;
            set => mArrangeType = value;
        }

        private List<LoopListViewItem2> mItemList = new List<LoopListViewItem2>();
        private RectTransform mContainerTrans;
        private ScrollRect mScrollRect = null;
        private RectTransform mScrollRectTransform = null;
        private RectTransform mViewPortRectTransform = null;
        private float mItemDefaultWithPaddingSize = 20;
        private int mItemTotalCount = 0;
        private bool mIsVertList = false;
        private System.Func<LoopListView2, int, bool, ListItemReleaseType, LoopListViewItem2> mOnGetItemByIndex;
        private Vector3[] mItemWorldCorners = new Vector3[4];
        private Vector3[] mViewPortRectLocalCorners = new Vector3[4];
        private int mCurReadyMinItemIndex = 0;
        private int mCurReadyMaxItemIndex = 0;
        private bool mNeedCheckNextMinItem = true;
        private bool mNeedCheckNextMaxItem = true;
        private ItemPosMgr mItemPosMgr = null;
        private float mDistanceForRecycle0 = 300;
        private float mDistanceForNew0 = 200;
        private float mDistanceForRecycle1 = 300;
        private float mDistanceForNew1 = 200;
        [SerializeField] private bool mSupportScrollBar = true;
        private bool mIsDraging = false;
        private PointerEventData mPointerEventData = null;
        public System.Action mOnBeginDragAction = null;
        public System.Action mOnDragingAction = null;
        public System.Action mOnEndDragAction = null;
        private int mLastItemIndex = 0;
        private float mLastItemPadding = 0;
        private float mSmoothDumpVel = 0;
        private float mSmoothDumpRate = 0.3f;
        private float mSnapFinishThreshold = 0.1f;
        private float mSnapVecThreshold = 145;
        [SerializeField] private bool mItemSnapEnable = false;


        private Vector3 mLastFrameContainerPos = Vector3.zero;
        public System.Action<LoopListView2,LoopListViewItem2> mOnSnapItemFinished = null;
        public System.Action<LoopListView2, LoopListViewItem2> mOnSnapNearestChanged = null;
        private int mCurSnapNearestItemIndex = -1;
        private Vector2 mAdjustedVec;
        private bool mNeedAdjustVec = false;
        private int mLeftSnapUpdateExtraCount = 1;
        [SerializeField] private Vector2 mViewPortSnapPivot = Vector2.zero;
        [SerializeField] private Vector2 mItemSnapPivot = Vector2.zero;
        private ClickEventListener mScrollBarClickEventListener = null;
        private SnapData mCurSnapData = new SnapData();
        private Vector3 mLastSnapCheckPos = Vector3.zero;
        private bool mStopRefreshingOnUpdate = false;
        private bool mListViewInited = false;
        private int mListUpdateCheckFrameCount = 0;

        public List<ItemPrefabConfData> ItemPrefabDataList => mItemPrefabDataList;

        public List<LoopListViewItem2> ItemList => mItemList;

        public bool IsVertList => mIsVertList;

        public int ItemTotalCount => mItemTotalCount;

        public RectTransform ContainerTrans => mContainerTrans;

        public ScrollRect ScrollRect => mScrollRect;

        public bool IsDraging => mIsDraging;
        
        public void StopRefreshingListOnUpdate() => mStopRefreshingOnUpdate = true;

        public bool ItemSnapEnable
        {
            get => mItemSnapEnable;
            set => mItemSnapEnable = value;
        }

        public bool SupportScrollBar
        {
            get => mSupportScrollBar;
            set => mSupportScrollBar = value;
        }

        public ItemPrefabConfData GetItemPrefabConfData(string prefabName)
        {
            foreach (var data in mItemPrefabDataList)
            {
                if (data.mItemPrefab == null)
                {
                    if (!_destroyed)
                        Debug.LogError("A item prefab is null");
                    continue;
                }
                if (prefabName == data.mItemPrefab.name)
                {
                    return data;
                }

            }
            return null;
        }

        public void OnItemPrefabChanged(string prefabName)
        {
            var data = GetItemPrefabConfData(prefabName);
            if(data == null)
            {
                return;
            }
            ItemPool pool = null;
            if (mItemPoolDict.TryGetValue(prefabName, out pool) == false)
            {
                return;
            }
            var firstItemIndex = -1;
            var pos = Vector3.zero;
            if(mItemList.Count > 0)
            {
                firstItemIndex = mItemList[0].ItemIndex;
                pos = mItemList[0].CachedRectTransform.anchoredPosition3D;
            }
            RecycleAllItem();
            ClearAllTmpRecycledItem();
            pool.DestroyAllItem();
            pool.Init(data.mItemPrefab, data.mPadding, data.mStartPosOffset,data.mInitCreateCount, mContainerTrans);
            if(firstItemIndex >= 0)
            {
                RefreshAllShownItemWithFirstIndexAndPos(firstItemIndex, pos);
            }
        }

        /*
        InitListView method is to initiate the LoopListView2 component. There are 3 parameters:
        itemTotalCount: the total item count in the listview. If this parameter is set -1, then means there are infinite items, and scrollbar would not be supported, and the ItemIndex can be from –MaxInt to +MaxInt. If this parameter is set a value >=0 , then the ItemIndex can only be from 0 to itemTotalCount -1.
        onGetItemByIndex: when a item is getting in the scrollrect viewport, and this Action will be called with the item’ index as a parameter, to let you create the item and update its content.
        */
        public void InitListView(int itemTotalCount,
            System.Func<LoopListView2, int, bool, ListItemReleaseType, LoopListViewItem2> onGetItemByIndex,
            LoopListViewInitParam initParam = null)
        {
            if(initParam != null)
            {
                mDistanceForRecycle0 = initParam.mDistanceForRecycle0;
                mDistanceForNew0 = initParam.mDistanceForNew0;
                mDistanceForRecycle1 = initParam.mDistanceForRecycle1;
                mDistanceForNew1 = initParam.mDistanceForNew1;
                mSmoothDumpRate = initParam.mSmoothDumpRate;
                mSnapFinishThreshold = initParam.mSnapFinishThreshold;
                mSnapVecThreshold = initParam.mSnapVecThreshold;
                mItemDefaultWithPaddingSize = initParam.mItemDefaultWithPaddingSize;
            }
            mScrollRect = gameObject.GetComponent<ScrollRect>();
            if (mScrollRect == null)
            {
                Debug.LogError("ListView Init Failed! ScrollRect component not found!");
                return;
            }
            if(mDistanceForRecycle0 <= mDistanceForNew0)
            {
                Debug.LogError("mDistanceForRecycle0 should be bigger than mDistanceForNew0");
            }
            if (mDistanceForRecycle1 <= mDistanceForNew1)
            {
                Debug.LogError("mDistanceForRecycle1 should be bigger than mDistanceForNew1");
            }
            mCurSnapData.Clear();
            mItemPosMgr = new ItemPosMgr(mItemDefaultWithPaddingSize);
            mScrollRectTransform = mScrollRect.GetComponent<RectTransform>();
            mContainerTrans = mScrollRect.content;
            mViewPortRectTransform = mScrollRect.viewport;
            if (mViewPortRectTransform == null)
            {
                mViewPortRectTransform = mScrollRectTransform;
            }
            if (mScrollRect.horizontalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport && mScrollRect.horizontalScrollbar != null)
            {
                Debug.LogError("ScrollRect.horizontalScrollbarVisibility cannot be set to AutoHideAndExpandViewport");
            }
            if (mScrollRect.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport && mScrollRect.verticalScrollbar != null)
            {
                Debug.LogError("ScrollRect.verticalScrollbarVisibility cannot be set to AutoHideAndExpandViewport");
            }
            mIsVertList = (mArrangeType == ListItemArrangeType.TopToBottom || mArrangeType == ListItemArrangeType.BottomToTop);
            mScrollRect.horizontal = !mIsVertList;
            mScrollRect.vertical = mIsVertList;
            SetScrollbarListener();
            AdjustPivot(mViewPortRectTransform);
            AdjustAnchor(mContainerTrans);
            AdjustContainerPivot(mContainerTrans);
            InitItemPool();
            mOnGetItemByIndex = onGetItemByIndex;
            if(mListViewInited == true)
            {
                Debug.LogError("LoopListView2.InitListView method can be called only once.");
            }
            mListViewInited = true;
            mStopRefreshingOnUpdate = false;
            ResetListView();
            //SetListItemCount(itemTotalCount, true);
            mCurSnapData.Clear();
            mItemTotalCount = itemTotalCount;
            if (mItemTotalCount < 0)
            {
                mSupportScrollBar = false;
            }
            if (mSupportScrollBar)
            {
                mItemPosMgr.SetItemMaxCount(mItemTotalCount);
            }
            else
            {
                mItemPosMgr.SetItemMaxCount(0);
            }
            mCurReadyMaxItemIndex = 0;
            mCurReadyMinItemIndex = 0;
            mLeftSnapUpdateExtraCount = 1;
            mNeedCheckNextMaxItem = true;
            mNeedCheckNextMinItem = true;
            UpdateContentSize();
        }

        private void SetScrollbarListener()
        {
            mScrollBarClickEventListener = null;
            Scrollbar curScrollBar = null;
            if (mIsVertList && mScrollRect.verticalScrollbar != null)
            {
                curScrollBar = mScrollRect.verticalScrollbar;

            }
            if (!mIsVertList && mScrollRect.horizontalScrollbar != null)
            {
                curScrollBar = mScrollRect.horizontalScrollbar;
            }
            if(curScrollBar == null)
            {
                return;
            }
            var listener = ClickEventListener.Get(curScrollBar.gameObject);
            mScrollBarClickEventListener = listener;
            listener.SetPointerUpHandler(OnPointerUpInScrollBar);
            listener.SetPointerDownHandler(OnPointerDownInScrollBar);
        }

        private void OnPointerDownInScrollBar(GameObject obj)
        {
            mCurSnapData.Clear();
        }

        private void OnPointerUpInScrollBar(GameObject obj)
        {
            ForceSnapUpdateCheck();
        }

        public void ResetListView(bool resetPos = true)
        {
            if (mViewPortRectTransform == null)
                return;
            
            mViewPortRectTransform.GetLocalCorners(mViewPortRectLocalCorners);
            if(resetPos)
            {
                mContainerTrans.anchoredPosition3D = Vector3.zero;
            }
            ForceSnapUpdateCheck();
        }


        /*
        This method may use to set the item total count of the scrollview at runtime. 
        If this parameter is set -1, then means there are infinite items,
        and scrollbar would not be supported, and the ItemIndex can be from –MaxInt to +MaxInt. 
        If this parameter is set a value >=0 , then the ItemIndex can only be from 0 to itemTotalCount -1.  
        If resetPos is set false, then the scrollrect’s content position will not changed after this method finished.
        */
        public void SetListItemCount(int itemCount, bool resetPos = true)
        {
            if(itemCount == mItemTotalCount)
            {
                return;
            }
            mCurSnapData.Clear();
            mItemTotalCount = itemCount;
            if (mItemTotalCount < 0)
            {
                mSupportScrollBar = false;
            }

            mItemPosMgr.SetItemMaxCount(mSupportScrollBar ? mItemTotalCount : 0);
            
            if (mItemTotalCount == 0)
            {
                mCurReadyMaxItemIndex = 0;
                mCurReadyMinItemIndex = 0;
                mNeedCheckNextMaxItem = false;
                mNeedCheckNextMinItem = false;
                RecycleAllItem();
                ClearAllTmpRecycledItem();
                UpdateContentSize();
                return;
            }
            if(mCurReadyMaxItemIndex >= mItemTotalCount)
            {
                mCurReadyMaxItemIndex = mItemTotalCount - 1;
            }
            mLeftSnapUpdateExtraCount = 1;
            mNeedCheckNextMaxItem = true;
            mNeedCheckNextMinItem = true;
            if (resetPos)
            {
                MovePanelToItemIndex(0, 0);
                return;
            }
            if (mItemList.Count == 0)
            {
                MovePanelToItemIndex(0, 0);
                return;
            }
            var maxItemIndex = mItemTotalCount - 1;
            var lastItemIndex = mItemList[mItemList.Count - 1].ItemIndex;
            if (lastItemIndex <= maxItemIndex)
            {
                UpdateContentSize();
                UpdateAllShownItemsPos();
                return;
            }
            MovePanelToItemIndex(maxItemIndex, 0);

        }

        //To get the visible item by itemIndex. If the item is not visible, then this method return null.
        public LoopListViewItem2 GetShownItemByItemIndex(int itemIndex)
        {
            var count = mItemList.Count;
            if (count == 0)
            {
                return null;
            }
            if (itemIndex < mItemList[0].ItemIndex || itemIndex > mItemList[count - 1].ItemIndex)
            {
                return null;
            }
            var i = itemIndex - mItemList[0].ItemIndex;
            return mItemList[i];
        }

        public int ShownItemCount => mItemList.Count;

        public float ViewPortSize
        {
            get
            {
                if (mIsVertList)
                {
                    return mViewPortRectTransform.rect.height;
                }
                else
                {
                    return mViewPortRectTransform.rect.width;
                }
            }
        }

        public float ViewPortWidth => mViewPortRectTransform.rect.width;

        public float ViewPortHeight => mViewPortRectTransform.rect.height;


        /*
         All visible items is stored in a List<LoopListViewItem2> , which is named mItemList;
         this method is to get the visible item by the index in visible items list. The parameter index is from 0 to mItemList.Count.
        */
        public LoopListViewItem2 GetShownItemByIndex(int index)
        {
            var count = mItemList.Count;
            if(index < 0 || index >= count)
            {
                return null;
            }
            return mItemList[index];
        }

        public LoopListViewItem2 GetShownItemByIndexWithoutCheck(int index)
        {
            return mItemList[index];
        }

        public int GetIndexInShownItemList(LoopListViewItem2 item)
        {
            if(item == null)
            {
                return -1;
            }
            var count = mItemList.Count;
            if (count == 0)
            {
                return -1;
            }
            for (var i = 0; i < count; ++i)
            {
                if (mItemList[i] == item)
                {
                    return i;
                }
            }
            return -1;
        }


        public void DoActionForEachShownItem(System.Action<LoopListViewItem2,object> action,object param)
        {
            if(action == null)
            {
                return;
            }
            var count = mItemList.Count;
            if(count == 0)
            {
                return;
            }
            for (var i = 0; i < count; ++i)
            {
                action(mItemList[i],param);
            }
        }


        public LoopListViewItem2 NewListViewItem(string itemPrefabName)
        {
            ItemPool pool = null;
            if (mItemPoolDict.TryGetValue(itemPrefabName, out pool) == false)
            {
                return null;
            }
            var item = pool.GetItem();
            var rf = item.GetComponent<RectTransform>();
            rf.SetParent(mContainerTrans);
            rf.localScale = Vector3.one;
            rf.anchoredPosition3D = Vector3.zero;
            rf.localEulerAngles = Vector3.zero;
            item.ParentListView = this;
            return item;
        }

        /*
        For a vertical scrollrect, when a visible item’s height changed at runtime, then this method should be called to let the LoopListView2 component reposition all visible items’ position.
        For a horizontal scrollrect, when a visible item’s width changed at runtime, then this method should be called to let the LoopListView2 component reposition all visible items’ position.
        */
        public void OnItemSizeChanged(int itemIndex)
        {
            var item = GetShownItemByItemIndex(itemIndex);
            if (item == null)
            {
                return;
            }
            if (mSupportScrollBar)
            {
                if (mIsVertList)
                {
                    SetItemSize(itemIndex, item.CachedRectTransform.rect.height, item.Padding);
                }
                else
                {
                    SetItemSize(itemIndex, item.CachedRectTransform.rect.width, item.Padding);
                }
            }
            UpdateContentSize();
            UpdateAllShownItemsPos();
        }


        /*
        To update a item by itemIndex.if the itemIndex-th item is not visible, then this method will do nothing.
        Otherwise this method will first call onGetItemByIndex(itemIndex) to get a updated item and then reposition all visible items'position. 
        */
        public void RefreshItemByItemIndex(int itemIndex)
        {
            var count = mItemList.Count;
            if (count == 0)
            {
                return;
            }
            if (itemIndex < mItemList[0].ItemIndex || itemIndex > mItemList[count - 1].ItemIndex)
            {
                return;
            }
            var firstItemIndex = mItemList[0].ItemIndex;
            var i = itemIndex - firstItemIndex;
            var curItem = mItemList[i];
            var pos = curItem.CachedRectTransform.anchoredPosition3D;
            RecycleItemTmp(curItem);
            var newItem = GetNewItemByIndex(itemIndex);
            if (newItem == null)
            {
                RefreshAllShownItemWithFirstIndex(firstItemIndex);
                return;
            }
            mItemList[i] = newItem;
            if(mIsVertList)
            {
                pos.x = newItem.StartPosOffset;
            }
            else
            {
                pos.y = newItem.StartPosOffset;
            }
            newItem.CachedRectTransform.anchoredPosition3D = pos;
            OnItemSizeChanged(itemIndex);
            ClearAllTmpRecycledItem();
        }

        //snap move will finish at once.
        public void FinishSnapImmediately()
        {
            UpdateSnapMove(true);
        }

        /*
        This method will move the scrollrect content’s position to ( the positon of itemIndex-th item + offset ),
        and offset is from 0 to scrollrect viewport size. 
        */
        public void MovePanelToItemIndex(int itemIndex, float offset)
        {
            mScrollRect.StopMovement();
            mCurSnapData.Clear();
            if (mItemTotalCount == 0)
            {
                return;
            }
            if(itemIndex < 0 && mItemTotalCount > 0)
            {
                return;
            }
            if (mItemTotalCount > 0 && itemIndex >= mItemTotalCount)
            {
                itemIndex = mItemTotalCount - 1;
            }
            if (offset < 0)
            {
                offset = 0;
            }
            var pos = Vector3.zero;
            var viewPortSize = ViewPortSize;
            if (offset > viewPortSize)
            {
                offset = viewPortSize;
            }
            if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                var containerPos = mContainerTrans.anchoredPosition3D.y;
                if (containerPos < 0)
                {
                    containerPos = 0;
                }
                pos.y = -containerPos - offset;
            }
            else if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                var containerPos = mContainerTrans.anchoredPosition3D.y;
                if (containerPos > 0)
                {
                    containerPos = 0;
                }
                pos.y = -containerPos + offset;
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                var containerPos = mContainerTrans.anchoredPosition3D.x;
                if (containerPos > 0)
                {
                    containerPos = 0;
                }
                pos.x = -containerPos + offset;
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                var containerPos = mContainerTrans.anchoredPosition3D.x;
                if (containerPos < 0)
                {
                    containerPos = 0;
                }
                pos.x = -containerPos - offset;
            }

            RecycleAllItem();
            var newItem = GetNewItemByIndex(itemIndex);
            if (newItem == null)
            {
                ClearAllTmpRecycledItem();
                return;
            }
            if (mIsVertList)
            {
                pos.x = newItem.StartPosOffset;
            }
            else
            {
                pos.y = newItem.StartPosOffset;
            }
            newItem.CachedRectTransform.anchoredPosition3D = pos;
            if (mSupportScrollBar)
            {
                if (mIsVertList)
                {
                    SetItemSize(itemIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                }
                else
                {
                    SetItemSize(itemIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                }
            }
            mItemList.Add(newItem);
            UpdateContentSize();
            UpdateListView(viewPortSize + 100, viewPortSize + 100, viewPortSize, viewPortSize);
            AdjustPanelPos();
            ClearAllTmpRecycledItem();
            ForceSnapUpdateCheck();
            UpdateSnapMove(false,true);
        }

        //update all visible items.
        public void RefreshAllShownItem(int newItemsAdded = 0)
        {
            var count = mItemList.Count;
            if (count == 0)
            {
                return;
            }

            var newItemIndex = Mathf.Clamp(mItemList[0].ItemIndex + newItemsAdded, 0, mItemTotalCount - 1);
            RefreshAllShownItemWithFirstIndex(newItemIndex);
        }

        public int GetCurrentLastShownItemIndex()
        {
            if (mItemList == null || mItemList.Count == 0)
                return -1;

            return mItemList[0].ItemIndex;
        }

        public void RefreshAllShownItemWithFirstIndex(int firstItemIndex)
        {
            var count = mItemList.Count;
            if (count == 0)
            {
                return;
            }
            var firstItem = mItemList[0];
            var pos = firstItem.CachedRectTransform.anchoredPosition3D;
            RecycleAllItem();
            for (var i = 0; i < count; ++i)
            {
                var curIndex = firstItemIndex + i;
                var newItem = GetNewItemByIndex(curIndex, true);
                if (newItem == null)
                {
                    break;
                }
                if (mIsVertList)
                {
                    pos.x = newItem.StartPosOffset;
                }
                else
                {
                    pos.y = newItem.StartPosOffset;
                }
                newItem.CachedRectTransform.anchoredPosition3D = pos;
                if (mSupportScrollBar)
                {
                    if (mIsVertList)
                    {
                        SetItemSize(curIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                    }
                    else
                    {
                        SetItemSize(curIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                    }
                }

                mItemList.Add(newItem);
            }
            UpdateContentSize();
            UpdateAllShownItemsPos();
            ClearAllTmpRecycledItem();
        }


        public void RefreshAllShownItemWithFirstIndexAndPos(int firstItemIndex,Vector3 pos)
        {
            RecycleAllItem();
            var newItem = GetNewItemByIndex(firstItemIndex);
            if (newItem == null)
            {
                return;
            }
            if (mIsVertList)
            {
                pos.x = newItem.StartPosOffset;
            }
            else
            {
                pos.y = newItem.StartPosOffset;
            }
            newItem.CachedRectTransform.anchoredPosition3D = pos;
            if (mSupportScrollBar)
            {
                if (mIsVertList)
                {
                    SetItemSize(firstItemIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                }
                else
                {
                    SetItemSize(firstItemIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                }
            }
            mItemList.Add(newItem);
            UpdateContentSize();
            UpdateAllShownItemsPos();
            UpdateListView(mDistanceForRecycle0, mDistanceForRecycle1, mDistanceForNew0, mDistanceForNew1);
            ClearAllTmpRecycledItem();
        }


        private void RecycleItemTmp(LoopListViewItem2 item)
        {
            if (item == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(item.ItemPrefabName))
            {
                return;
            }
            ItemPool pool = null;
            if (mItemPoolDict.TryGetValue(item.ItemPrefabName, out pool) == false)
            {
                return;
            }
            pool.RecycleItem(item);

        }


        private void ClearAllTmpRecycledItem()
        {
            var count = mItemPoolList.Count;
            for(var i = 0;i<count;++i)
            {
                mItemPoolList[i].ClearTmpRecycledItem();
            }
        }


        private void RecycleAllItem()
        {
            if (mItemList.Count <= 0)
                return;
            
            mItemList.Reverse();
            foreach (var item in mItemList)
            {
                RecycleItemTmp(item);
            }
            mItemList.Clear();
        }


        private void AdjustContainerPivot(RectTransform rtf)
        {
            var pivot = rtf.pivot;
            if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                pivot.y = 0;
            }
            else if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                pivot.y = 1;
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                pivot.x = 0;
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                pivot.x = 1;
            }
            rtf.pivot = pivot;
        }


        private void AdjustPivot(RectTransform rtf)
        {
            var pivot = rtf.pivot;

            if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                pivot.y = 0;
            }
            else if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                pivot.y = 1;
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                pivot.x = 0;
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                pivot.x = 1;
            }
            rtf.pivot = pivot;
        }

        private void AdjustContainerAnchor(RectTransform rtf)
        {
            var anchorMin = rtf.anchorMin;
            var anchorMax = rtf.anchorMax;
            if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                anchorMin.y = 0;
                anchorMax.y = 0;
            }
            else if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                anchorMin.y = 1;
                anchorMax.y = 1;
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                anchorMin.x = 0;
                anchorMax.x = 0;
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                anchorMin.x = 1;
                anchorMax.x = 1;
            }
            rtf.anchorMin = anchorMin;
            rtf.anchorMax = anchorMax;
        }


        private void AdjustAnchor(RectTransform rtf)
        {
            var anchorMin = rtf.anchorMin;
            var anchorMax = rtf.anchorMax;
            if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                anchorMin.y = 0;
                anchorMax.y = 0;
            }
            else if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                anchorMin.y = 1;
                anchorMax.y = 1;
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                anchorMin.x = 0;
                anchorMax.x = 0;
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                anchorMin.x = 1;
                anchorMax.x = 1;
            }
            rtf.anchorMin = anchorMin;
            rtf.anchorMax = anchorMax;
        }

        private void InitItemPool()
        {
            foreach (var data in mItemPrefabDataList)
            {
                if (data.mItemPrefab == null)
                {
                    if (!_destroyed)
                        Debug.LogError("A item prefab is null ");
                    continue;
                }
                var prefabName = data.mItemPrefab.name;
                if (mItemPoolDict.ContainsKey(prefabName))
                {
                    Debug.LogError("A item prefab with name " + prefabName + " has existed!");
                    continue;
                }
                var rtf = data.mItemPrefab.GetComponent<RectTransform>();
                if (rtf == null)
                {
                    Debug.LogError("RectTransform component is not found in the prefab " + prefabName);
                    continue;
                }
                AdjustAnchor(rtf);
                AdjustPivot(rtf);
                var tItem = data.mItemPrefab.GetComponent<LoopListViewItem2>();
                if (tItem == null)
                {
                    data.mItemPrefab.AddComponent<LoopListViewItem2>();
                }
                var pool = new ItemPool();
                pool.Init(data.mItemPrefab, data.mPadding,data.mStartPosOffset, data.mInitCreateCount, mContainerTrans);
                mItemPoolDict.Add(prefabName, pool);
                mItemPoolList.Add(pool);
            }
        }



        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            mIsDraging = true;
            CacheDragPointerEventData(eventData);
            mCurSnapData.Clear();
            if(mOnBeginDragAction != null)
            {
                mOnBeginDragAction();
            }
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            mIsDraging = false;
            mPointerEventData = null;
            if (mOnEndDragAction != null)
            {
                mOnEndDragAction();
            }
            ForceSnapUpdateCheck();
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            CacheDragPointerEventData(eventData);
            if (mOnDragingAction != null)
            {
                mOnDragingAction();
            }
        }

        private void CacheDragPointerEventData(PointerEventData eventData)
        {
            if (mPointerEventData == null)
            {
                mPointerEventData = new PointerEventData(EventSystem.current);
            }
            mPointerEventData.button = eventData.button;
            mPointerEventData.position = eventData.position;
            mPointerEventData.pointerPressRaycast = eventData.pointerPressRaycast;
            mPointerEventData.pointerCurrentRaycast = eventData.pointerCurrentRaycast;
        }

        private LoopListViewItem2 GetNewItemByIndex(int index, bool isVisible = false, ListItemReleaseType listItemReleaseType = ListItemReleaseType.Default)
        {
            if(mSupportScrollBar && index < 0)
            {
                return null;
            }
            if(mItemTotalCount > 0 && index >= mItemTotalCount)
            {
                return null;
            }

            LoopListViewItem2 newItem = null;

            try
            {
                newItem = mOnGetItemByIndex(this, index, isVisible, listItemReleaseType);
            }
            catch (Exception e)
            {
                LoggerService.LogWarning("Unable to retrieve item at index " + index + " with error: " + e);
            }
           
            if (newItem == null)
            {
                return null;
            }
            
            newItem.ItemIndex = index;
            newItem.ItemCreatedCheckFrameCount = mListUpdateCheckFrameCount;
            return newItem;
        }


        private void SetItemSize(int itemIndex, float itemSize,float padding)
        {
            mItemPosMgr.SetItemSize(itemIndex, itemSize+padding);
            if(itemIndex >= mLastItemIndex)
            {
                mLastItemIndex = itemIndex;
                mLastItemPadding = padding;
            }
        }

        private bool GetPlusItemIndexAndPosAtGivenPos(float pos, ref int index, ref float itemPos)
        {
            return mItemPosMgr.GetItemIndexAndPosAtGivenPos(pos, ref index, ref itemPos);
        }


        private float GetItemPos(int itemIndex)
        {
            return mItemPosMgr.GetItemPos(itemIndex);
        }

      
        public Vector3 GetItemCornerPosInViewPort(LoopListViewItem2 item, ItemCornerEnum corner = ItemCornerEnum.LeftBottom)
        {
            item.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
            return mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[(int)corner]);
        }


        private void AdjustPanelPos()
        {
            var count = mItemList.Count;
            if (count == 0)
            {
                return;
            }
            UpdateAllShownItemsPos();
            var viewPortSize = ViewPortSize;
            var contentSize = GetContentPanelSize();
            if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                if (contentSize <= viewPortSize)
                {
                    var pos = mContainerTrans.anchoredPosition3D;
                    pos.y = 0;
                    mContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(mItemList[0].StartPosOffset,0,0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var topPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                if (topPos0.y < mViewPortRectLocalCorners[1].y)
                {
                    var pos = mContainerTrans.anchoredPosition3D;
                    pos.y = 0;
                    mContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(mItemList[0].StartPosOffset, 0, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var downPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                var d = downPos1.y - mViewPortRectLocalCorners[0].y;
                if (d > 0)
                {
                    var pos = mItemList[0].CachedRectTransform.anchoredPosition3D;
                    pos.y = pos.y - d;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = pos;
                    UpdateAllShownItemsPos();
                    return;
                }
            }
            else if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                if (contentSize <= viewPortSize)
                {
                    var pos = mContainerTrans.anchoredPosition3D;
                    pos.y = 0;
                    mContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(mItemList[0].StartPosOffset, 0, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var downPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                if (downPos0.y > mViewPortRectLocalCorners[0].y)
                {
                    var pos = mContainerTrans.anchoredPosition3D;
                    pos.y = 0;
                    mContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(mItemList[0].StartPosOffset, 0, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var topPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var d = mViewPortRectLocalCorners[1].y - topPos1.y;
                if (d > 0)
                {
                    var pos = mItemList[0].CachedRectTransform.anchoredPosition3D;
                    pos.y = pos.y + d;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = pos;
                    UpdateAllShownItemsPos();
                    return;
                }
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                if (contentSize <= viewPortSize)
                {
                    var pos = mContainerTrans.anchoredPosition3D;
                    pos.x = 0;
                    mContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(0,mItemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var leftPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                if (leftPos0.x > mViewPortRectLocalCorners[1].x)
                {
                    var pos = mContainerTrans.anchoredPosition3D;
                    pos.x = 0;
                    mContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(0, mItemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var rightPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                var d = mViewPortRectLocalCorners[2].x - rightPos1.x;
                if (d > 0)
                {
                    var pos = mItemList[0].CachedRectTransform.anchoredPosition3D;
                    pos.x = pos.x + d;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = pos;
                    UpdateAllShownItemsPos();
                    return;
                }
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                if (contentSize <= viewPortSize)
                {
                    var pos = mContainerTrans.anchoredPosition3D;
                    pos.x = 0;
                    mContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(0, mItemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var rightPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                if (rightPos0.x < mViewPortRectLocalCorners[2].x)
                {
                    var pos = mContainerTrans.anchoredPosition3D;
                    pos.x = 0;
                    mContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = new Vector3(0, mItemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var leftPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var d = leftPos1.x - mViewPortRectLocalCorners[1].x;
                if (d > 0)
                {
                    var pos = mItemList[0].CachedRectTransform.anchoredPosition3D;
                    pos.x = pos.x - d;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = pos;
                    UpdateAllShownItemsPos();
                    return;
                }
            }



        }


        private void Update()
        {
            if(mListViewInited == false || mStopRefreshingOnUpdate)
            {
                return;
            }
            if(mNeedAdjustVec)
            {
                mNeedAdjustVec = false;
                if(mIsVertList)
                {
                    if(mScrollRect.velocity.y * mAdjustedVec.y > 0)
                    {
                        mScrollRect.velocity = mAdjustedVec;
                    }
                }
                else
                {
                    if (mScrollRect.velocity.x * mAdjustedVec.x > 0)
                    {
                        mScrollRect.velocity = mAdjustedVec;
                    }
                }
                
            }
            if (mSupportScrollBar)
            {
                mItemPosMgr.Update(false);
            }
            UpdateSnapMove();
            UpdateListView(mDistanceForRecycle0, mDistanceForRecycle1, mDistanceForNew0, mDistanceForNew1);
            ClearAllTmpRecycledItem();
            mLastFrameContainerPos = mContainerTrans.anchoredPosition3D;
        }

        //update snap move. if immediate is set true, then the snap move will finish at once.
        private void UpdateSnapMove(bool immediate = false, bool forceSendEvent = false)
        {
            if (mItemSnapEnable == false)
            {
                return;
            }
            if (mIsVertList)
            {
                UpdateSnapVertical(immediate,forceSendEvent);
            }
            else
            {
                UpdateSnapHorizontal(immediate,forceSendEvent);
            }
        }



        public void UpdateAllShownItemSnapData()
        {
            if (mItemSnapEnable == false)
            {
                return;
            }
            var count = mItemList.Count;
            if (count == 0)
            {
                return;
            }
            var pos = mContainerTrans.anchoredPosition3D;
            var tViewItem0 = mItemList[0];
            tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
            float start = 0;
            float end = 0;
            float itemSnapCenter = 0;
            float snapCenter = 0;
            if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                snapCenter = -(1 - mViewPortSnapPivot.y) * mViewPortRectTransform.rect.height;
                var topPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                start = topPos1.y;
                end = start - tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start - tViewItem0.ItemSize * (1 - mItemSnapPivot.y);
                for (var i = 0; i < count; ++i)
                {
                    mItemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                    if ((i + 1) < count)
                    {
                        start = end;
                        end = end - mItemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start - mItemList[i + 1].ItemSize * (1 - mItemSnapPivot.y);
                    }
                }
            }
            else if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                snapCenter = mViewPortSnapPivot.y * mViewPortRectTransform.rect.height;
                var bottomPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                start = bottomPos1.y;
                end = start + tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start + tViewItem0.ItemSize * mItemSnapPivot.y;
                for (var i = 0; i < count; ++i)
                {
                    mItemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                    if ((i + 1) < count)
                    {
                        start = end;
                        end = end + mItemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start + mItemList[i + 1].ItemSize * mItemSnapPivot.y;
                    }
                }
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                snapCenter = -(1 - mViewPortSnapPivot.x) * mViewPortRectTransform.rect.width;
                var rightPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                start = rightPos1.x;
                end = start - tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start - tViewItem0.ItemSize * (1 - mItemSnapPivot.x);
                for (var i = 0; i < count; ++i)
                {
                    mItemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                    if ((i + 1) < count)
                    {
                        start = end;
                        end = end - mItemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start - mItemList[i + 1].ItemSize * (1 - mItemSnapPivot.x);
                    }
                }
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                snapCenter = mViewPortSnapPivot.x * mViewPortRectTransform.rect.width;
                var leftPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                start = leftPos1.x;
                end = start + tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start + tViewItem0.ItemSize * mItemSnapPivot.x;
                for (var i = 0; i < count; ++i)
                {
                    mItemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                    if ((i + 1) < count)
                    {
                        start = end;
                        end = end + mItemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start + mItemList[i + 1].ItemSize * mItemSnapPivot.x;
                    }
                }
            }
        }


        private void UpdateSnapVertical(bool immediate = false, bool forceSendEvent = false)
        {
            if(mItemSnapEnable == false)
            {
                return;
            }
            var count = mItemList.Count;
            if (count == 0)
            {
                return;
            }
            var pos = mContainerTrans.anchoredPosition3D;
            var needCheck = (pos.y != mLastSnapCheckPos.y);
            mLastSnapCheckPos = pos;
            if (!needCheck)
            {
                if (mLeftSnapUpdateExtraCount > 0)
                {
                    mLeftSnapUpdateExtraCount--;
                    needCheck = true;
                }
            }
            if (needCheck)
            {
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var curIndex = -1;
                float start = 0;
                float end = 0;
                float itemSnapCenter = 0;
                var curMinDist = float.MaxValue;
                float curDist = 0;
                float curDistAbs = 0;
                float snapCenter = 0; 
                if (mArrangeType == ListItemArrangeType.TopToBottom)
                {
                    snapCenter = -(1 - mViewPortSnapPivot.y) * mViewPortRectTransform.rect.height;
                    var topPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                    start = topPos1.y;
                    end = start - tViewItem0.ItemSizeWithPadding;
                    itemSnapCenter = start - tViewItem0.ItemSize * (1-mItemSnapPivot.y);
                    for (var i = 0; i < count; ++i)
                    {
                        curDist = snapCenter - itemSnapCenter;
                        curDistAbs = Mathf.Abs(curDist);
                        if (curDistAbs < curMinDist)
                        {
                            curMinDist = curDistAbs;
                            curIndex = i;
                        }
                        else
                        {
                            break;
                        }
                        
                        if ((i + 1) < count)
                        {
                            start = end;
                            end = end - mItemList[i + 1].ItemSizeWithPadding;
                            itemSnapCenter = start - mItemList[i + 1].ItemSize * (1 - mItemSnapPivot.y);
                        }
                    }
                }
                else if(mArrangeType == ListItemArrangeType.BottomToTop)
                {
                    snapCenter = mViewPortSnapPivot.y * mViewPortRectTransform.rect.height;
                    var bottomPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                    start = bottomPos1.y;
                    end = start + tViewItem0.ItemSizeWithPadding;
                    itemSnapCenter = start + tViewItem0.ItemSize * mItemSnapPivot.y;
                    for (var i = 0; i < count; ++i)
                    {
                        curDist = snapCenter - itemSnapCenter;
                        curDistAbs = Mathf.Abs(curDist);
                        if (curDistAbs < curMinDist)
                        {
                            curMinDist = curDistAbs;
                            curIndex = i;
                        }
                        else
                        {
                            break;
                        }

                        if ((i + 1) < count)
                        {
                            start = end;
                            end = end + mItemList[i + 1].ItemSizeWithPadding;
                            itemSnapCenter = start + mItemList[i + 1].ItemSize *  mItemSnapPivot.y;
                        }
                    }
                }

                if (curIndex >= 0)
                {
                    var oldNearestItemIndex = mCurSnapNearestItemIndex;
                    mCurSnapNearestItemIndex = mItemList[curIndex].ItemIndex;
                    if (forceSendEvent || mItemList[curIndex].ItemIndex != oldNearestItemIndex)
                    {
                        if (mOnSnapNearestChanged != null)
                        {
                            mOnSnapNearestChanged(this,mItemList[curIndex]);
                        }
                    }
                }
                else
                {
                    mCurSnapNearestItemIndex = -1;
                }
            }
            if (CanSnap() == false)
            {
                ClearSnapData();
                return;
            }
            var v = Mathf.Abs(mScrollRect.velocity.y);
            UpdateCurSnapData();
            if (mCurSnapData.mSnapStatus != SnapStatus.SnapMoving)
            {
                return;
            }
            if (v > 0)
            {
                mScrollRect.StopMovement();
            }
            var old = mCurSnapData.mCurSnapVal;
            mCurSnapData.mCurSnapVal = Mathf.SmoothDamp(mCurSnapData.mCurSnapVal, mCurSnapData.mTargetSnapVal, ref mSmoothDumpVel, mSmoothDumpRate);
            var dt = mCurSnapData.mCurSnapVal - old;
                
            if (immediate || Mathf.Abs(mCurSnapData.mTargetSnapVal - mCurSnapData.mCurSnapVal) < mSnapFinishThreshold)
            {
                pos.y = pos.y + mCurSnapData.mTargetSnapVal - old;
                mCurSnapData.mSnapStatus = SnapStatus.SnapMoveFinish;
                if (mOnSnapItemFinished != null)
                {
                    var targetItem = GetShownItemByItemIndex(mCurSnapNearestItemIndex);
                    if(targetItem != null)
                    {
                        mOnSnapItemFinished(this,targetItem);
                    }
                }
            }
            else
            {
                pos.y = pos.y + dt;
            }

            if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                var maxY = mViewPortRectLocalCorners[0].y + mContainerTrans.rect.height;
                pos.y = Mathf.Clamp(pos.y, 0, maxY);
                mContainerTrans.anchoredPosition3D = pos;
            }
            else if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                var minY = mViewPortRectLocalCorners[1].y - mContainerTrans.rect.height;
                pos.y = Mathf.Clamp(pos.y, minY, 0);
                mContainerTrans.anchoredPosition3D = pos;
            }

        }


        private void UpdateCurSnapData()
        {
            var count = mItemList.Count;
            if (count == 0)
            {
                mCurSnapData.Clear();
                return;
            }

            if (mCurSnapData.mSnapStatus == SnapStatus.SnapMoveFinish)
            {
                if (mCurSnapData.mSnapTargetIndex == mCurSnapNearestItemIndex)
                {
                    return;
                }
                mCurSnapData.mSnapStatus = SnapStatus.NoTargetSet;
            }
            if (mCurSnapData.mSnapStatus == SnapStatus.SnapMoving)
            {
                if ( (mCurSnapData.mSnapTargetIndex == mCurSnapNearestItemIndex) || mCurSnapData.mIsForceSnapTo)
                {
                    return;
                }
                mCurSnapData.mSnapStatus = SnapStatus.NoTargetSet;
            }
            if (mCurSnapData.mSnapStatus == SnapStatus.NoTargetSet)
            {
                var nearestItem = GetShownItemByItemIndex(mCurSnapNearestItemIndex);
                if (nearestItem == null)
                {
                    return;
                }
                mCurSnapData.mSnapTargetIndex = mCurSnapNearestItemIndex;
                mCurSnapData.mSnapStatus = SnapStatus.TargetHasSet;
                mCurSnapData.mIsForceSnapTo = false;
            }
            if (mCurSnapData.mSnapStatus == SnapStatus.TargetHasSet)
            {
                var targetItem = GetShownItemByItemIndex(mCurSnapData.mSnapTargetIndex);
                if (targetItem == null)
                {
                    mCurSnapData.Clear();
                    return;
                }
                UpdateAllShownItemSnapData();
                mCurSnapData.mTargetSnapVal = targetItem.DistanceWithViewPortSnapCenter;
                mCurSnapData.mCurSnapVal = 0;
                mCurSnapData.mSnapStatus = SnapStatus.SnapMoving;
            }

        }
        //Clear current snap target and then the LoopScrollView2 will auto snap to the CurSnapNearestItemIndex.
        public void ClearSnapData()
        {
            mCurSnapData.Clear();
        }

        public void SetSnapTargetItemIndex(int itemIndex)
        {
            mCurSnapData.mSnapTargetIndex = itemIndex;
            mCurSnapData.mSnapStatus = SnapStatus.TargetHasSet;
            mCurSnapData.mIsForceSnapTo = true;
        }

        //Get the nearest item index with the viewport snap point.
        public int CurSnapNearestItemIndex => mCurSnapNearestItemIndex;

        public void ForceSnapUpdateCheck()
        {
            if(mLeftSnapUpdateExtraCount <= 0)
            {
                mLeftSnapUpdateExtraCount = 1;
            }
        }

        private void UpdateSnapHorizontal(bool immediate = false, bool forceSendEvent = false)
        {
            if (mItemSnapEnable == false)
            {
                return;
            }
            var count = mItemList.Count;
            if (count == 0)
            {
                return;
            }
            var pos = mContainerTrans.anchoredPosition3D;
            var needCheck = (pos.x != mLastSnapCheckPos.x);
            mLastSnapCheckPos = pos;
            if (!needCheck)
            {
                if(mLeftSnapUpdateExtraCount > 0)
                {
                    mLeftSnapUpdateExtraCount--;
                    needCheck = true;
                }
            }
            if (needCheck)
            {
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var curIndex = -1;
                float start = 0;
                float end = 0;
                float itemSnapCenter = 0;
                var curMinDist = float.MaxValue;
                float curDist = 0;
                float curDistAbs = 0;
                float snapCenter = 0;
                if (mArrangeType == ListItemArrangeType.RightToLeft)
                {
                    snapCenter = -(1 - mViewPortSnapPivot.x) * mViewPortRectTransform.rect.width;
                    var rightPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                    start = rightPos1.x;
                    end = start - tViewItem0.ItemSizeWithPadding;
                    itemSnapCenter = start - tViewItem0.ItemSize * (1 - mItemSnapPivot.x);
                    for (var i = 0; i < count; ++i)
                    {
                        curDist = snapCenter - itemSnapCenter;
                        curDistAbs = Mathf.Abs(curDist);
                        if (curDistAbs < curMinDist)
                        {
                            curMinDist = curDistAbs;
                            curIndex = i;
                        }
                        else
                        {
                            break;
                        }

                        if ((i + 1) < count)
                        {
                            start = end;
                            end = end - mItemList[i + 1].ItemSizeWithPadding;
                            itemSnapCenter = start - mItemList[i + 1].ItemSize * (1 - mItemSnapPivot.x);
                        }
                    }
                }
                else if (mArrangeType == ListItemArrangeType.LeftToRight)
                {
                    snapCenter = mViewPortSnapPivot.x * mViewPortRectTransform.rect.width;
                    var leftPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                    start = leftPos1.x;
                    end = start + tViewItem0.ItemSizeWithPadding;
                    itemSnapCenter = start + tViewItem0.ItemSize * mItemSnapPivot.x;
                    for (var i = 0; i < count; ++i)
                    {
                        curDist = snapCenter - itemSnapCenter;
                        curDistAbs = Mathf.Abs(curDist);
                        if (curDistAbs < curMinDist)
                        {
                            curMinDist = curDistAbs;
                            curIndex = i;
                        }
                        else
                        {
                            break;
                        }

                        if ((i + 1) < count)
                        {
                            start = end;
                            end = end + mItemList[i + 1].ItemSizeWithPadding;
                            itemSnapCenter = start + mItemList[i + 1].ItemSize * mItemSnapPivot.x;
                        }
                    }
                }


                if (curIndex >= 0)
                {
                    var oldNearestItemIndex = mCurSnapNearestItemIndex;
                    mCurSnapNearestItemIndex = mItemList[curIndex].ItemIndex;
                    if (forceSendEvent || mItemList[curIndex].ItemIndex != oldNearestItemIndex)
                    {
                        if (mOnSnapNearestChanged != null)
                        {
                            mOnSnapNearestChanged(this, mItemList[curIndex]);
                        }
                    }
                }
                else
                {
                    mCurSnapNearestItemIndex = -1;
                }
            }
            if (CanSnap() == false)
            {
                ClearSnapData();
                return;
            }
            var v = Mathf.Abs(mScrollRect.velocity.x);
            UpdateCurSnapData();
            if(mCurSnapData.mSnapStatus != SnapStatus.SnapMoving)
            {
                return;
            }
            if (v > 0)
            {
                mScrollRect.StopMovement();
            }
            var old = mCurSnapData.mCurSnapVal;
            mCurSnapData.mCurSnapVal = Mathf.SmoothDamp(mCurSnapData.mCurSnapVal, mCurSnapData.mTargetSnapVal, ref mSmoothDumpVel, mSmoothDumpRate);
            var dt = mCurSnapData.mCurSnapVal - old;

            if (immediate || Mathf.Abs(mCurSnapData.mTargetSnapVal - mCurSnapData.mCurSnapVal) < mSnapFinishThreshold)
            {
                pos.x = pos.x + mCurSnapData.mTargetSnapVal - old;
                mCurSnapData.mSnapStatus = SnapStatus.SnapMoveFinish;
                if (mOnSnapItemFinished != null)
                {
                    var targetItem = GetShownItemByItemIndex(mCurSnapNearestItemIndex);
                    if (targetItem != null)
                    {
                        mOnSnapItemFinished(this, targetItem);
                    }
                }
            }
            else
            {
                pos.x = pos.x + dt;
            }
                
            if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                var minX = mViewPortRectLocalCorners[2].x - mContainerTrans.rect.width;
                pos.x = Mathf.Clamp(pos.x, minX, 0);
                mContainerTrans.anchoredPosition3D = pos;
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                var maxX = mViewPortRectLocalCorners[1].x + mContainerTrans.rect.width;
                pos.x = Mathf.Clamp(pos.x, 0, maxX);
                mContainerTrans.anchoredPosition3D = pos;
            }
        }

        private bool CanSnap()
        {
            if (mIsDraging)
            {
                return false;
            }
            if (mScrollBarClickEventListener != null)
            {
                if (mScrollBarClickEventListener.IsPressd)
                {
                    return false;
                }
            }

            if (mIsVertList)
            {
                if(mContainerTrans.rect.height <= ViewPortHeight)
                {
                    return false;
                }
            }
            else
            {
                if (mContainerTrans.rect.width <= ViewPortWidth)
                {
                    return false;
                }
            }

            float v = 0;
            if (mIsVertList)
            {
                v = Mathf.Abs(mScrollRect.velocity.y);
            }
            else
            {
                v = Mathf.Abs(mScrollRect.velocity.x);
            }
            if (v > mSnapVecThreshold)
            {
                return false;
            }
            if (v < 2)
            {
                return true;
            }
            float diff = 3;
            var pos = mContainerTrans.anchoredPosition3D;
            if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                var minX = mViewPortRectLocalCorners[2].x - mContainerTrans.rect.width;
                if (pos.x < (minX - diff) || pos.x > diff)
                {
                    return false;
                }
            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                var maxX = mViewPortRectLocalCorners[1].x + mContainerTrans.rect.width;
                if (pos.x > (maxX + diff) || pos.x < -diff)
                {
                    return false;
                }
            }
            else if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                var maxY = mViewPortRectLocalCorners[0].y + mContainerTrans.rect.height;
                if (pos.y > (maxY + diff) || pos.y < -diff)
                {
                    return false;
                }
            }
            else if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                var minY = mViewPortRectLocalCorners[1].y - mContainerTrans.rect.height;
                if (pos.y < (minY - diff) || pos.y > diff)
                {
                    return false;
                }
            }
            return true;
        }



        public void UpdateListView(float distanceForRecycle0, float distanceForRecycle1, float distanceForNew0, float distanceForNew1)
        {
            mListUpdateCheckFrameCount++;
            if (mIsVertList)
            {
                var needContinueCheck = true;
                var checkCount = 0;
                var maxCount = 9999;
                while (needContinueCheck)
                {
                    checkCount++;
                    if(checkCount >= maxCount)
                    {
                        Debug.LogError("UpdateListView Vertical while loop " + checkCount + " times! something is wrong!");
                        break;
                    }
                    needContinueCheck = UpdateForVertList(distanceForRecycle0, distanceForRecycle1, distanceForNew0, distanceForNew1);
                }
            }
            else
            {
                var needContinueCheck = true;
                var checkCount = 0;
                var maxCount = 9999;
                while (needContinueCheck)
                {
                    checkCount++;
                    if (checkCount >= maxCount)
                    {
                        Debug.LogError("UpdateListView  Horizontal while loop " + checkCount + " times! something is wrong!");
                        break;
                    }
                    needContinueCheck = UpdateForHorizontalList(distanceForRecycle0, distanceForRecycle1, distanceForNew0, distanceForNew1);
                }
            }

        }


        private bool UpdateForVertList(float distanceForRecycle0,float distanceForRecycle1,float distanceForNew0, float distanceForNew1)
        {
            if (mItemTotalCount == 0)
            {
                if(mItemList.Count > 0)
                {
                    RecycleAllItem();
                }
                return false;
            }
            if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                var itemListCount = mItemList.Count;
                if (itemListCount == 0)
                {
                    var curY = mContainerTrans.anchoredPosition3D.y;
                    if (curY < 0)
                    {
                        curY = 0;
                    }
                    var index = 0;
                    var pos = -curY;
                    if (mSupportScrollBar)
                    {
                        if( GetPlusItemIndexAndPosAtGivenPos(curY, ref index, ref pos) == false)
                        {
                            return false;
                        }
                        pos = -pos;
                    }
                    var newItem = GetNewItemByIndex(index, listItemReleaseType: ListItemReleaseType.OnScroll);
                    if (newItem == null)
                    {
                        return false;
                    }
                    if (mSupportScrollBar)
                    {
                        SetItemSize(index, newItem.CachedRectTransform.rect.height, newItem.Padding);
                    }
                    mItemList.Add(newItem);
                    newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, pos, 0);
                    UpdateContentSize();
                    return true;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var topPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var downPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);

                if (!mIsDraging && tViewItem0.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                    && downPos0.y - mViewPortRectLocalCorners[1].y > distanceForRecycle0)
                {
                    mItemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }

                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var topPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var downPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                if (!mIsDraging && tViewItem1.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                    && mViewPortRectLocalCorners[0].y - topPos1.y > distanceForRecycle1)
                {
                    mItemList.RemoveAt(mItemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }
                
                if (mViewPortRectLocalCorners[0].y - downPos1.y < distanceForNew1)
                {
                    if(tViewItem1.ItemIndex > mCurReadyMaxItemIndex)
                    {
                        mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                        mNeedCheckNextMaxItem = true;
                    }
                    var nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= mCurReadyMaxItemIndex || mNeedCheckNextMaxItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex, listItemReleaseType: ListItemReleaseType.OnScroll);
                        if (newItem == null)
                        {
                            mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                            mNeedCheckNextMaxItem = false;
                            CheckIfNeedUpdataItemPos();
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                            }
                            mItemList.Add(newItem);
                            var y = tViewItem1.CachedRectTransform.anchoredPosition3D.y - tViewItem1.CachedRectTransform.rect.height - tViewItem1.Padding;
                            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, y, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();

                            if (nIndex > mCurReadyMaxItemIndex)
                            {
                                mCurReadyMaxItemIndex = nIndex;
                            }
                            return true;
                        }
                    }
                }

                if (topPos0.y - mViewPortRectLocalCorners[1].y < distanceForNew0)
                {
                    if(tViewItem0.ItemIndex < mCurReadyMinItemIndex)
                    {
                        mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                        mNeedCheckNextMinItem = true;
                    }
                    var nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= mCurReadyMinItemIndex || mNeedCheckNextMinItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex, listItemReleaseType: ListItemReleaseType.OnScroll);
                        if (newItem == null)
                        {
                            mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                            mNeedCheckNextMinItem = false;
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                            }
                            mItemList.Insert(0, newItem);
                            var y = tViewItem0.CachedRectTransform.anchoredPosition3D.y + newItem.CachedRectTransform.rect.height + newItem.Padding;
                            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, y, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();
                            if (nIndex < mCurReadyMinItemIndex)
                            {
                                mCurReadyMinItemIndex = nIndex;
                            }
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (mItemList.Count == 0)
                {
                    var curY = mContainerTrans.anchoredPosition3D.y;
                    if (curY > 0)
                    {
                        curY = 0;
                    }
                    var index = 0;
                    var pos = -curY;
                    if (mSupportScrollBar)
                    {
                        if(GetPlusItemIndexAndPosAtGivenPos(-curY, ref index, ref pos) == false)
                        {
                            return false;
                        }
                    }
                    var newItem = GetNewItemByIndex(index, listItemReleaseType: ListItemReleaseType.OnScroll);
                    if (newItem == null)
                    {
                        return false;
                    }
                    if (mSupportScrollBar)
                    {
                        SetItemSize(index, newItem.CachedRectTransform.rect.height, newItem.Padding);
                    }
                    mItemList.Add(newItem);
                    newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, pos, 0);
                    UpdateContentSize();
                    return true;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var topPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var downPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);

                if (!mIsDraging && tViewItem0.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                    && mViewPortRectLocalCorners[0].y - topPos0.y > distanceForRecycle0)
                {
                    mItemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }

                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var topPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var downPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                if (!mIsDraging && tViewItem1.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                     && downPos1.y - mViewPortRectLocalCorners[1].y > distanceForRecycle1)
                {
                    mItemList.RemoveAt(mItemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }

                if (topPos1.y - mViewPortRectLocalCorners[1].y < distanceForNew1)
                {
                    if (tViewItem1.ItemIndex > mCurReadyMaxItemIndex)
                    {
                        mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                        mNeedCheckNextMaxItem = true;
                    }
                    var nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= mCurReadyMaxItemIndex || mNeedCheckNextMaxItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex, listItemReleaseType: ListItemReleaseType.OnScroll);
                        if (newItem == null)
                        {
                            mNeedCheckNextMaxItem = false;
                            CheckIfNeedUpdataItemPos();
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                            }
                            mItemList.Add(newItem);
                            var y = tViewItem1.CachedRectTransform.anchoredPosition3D.y + tViewItem1.CachedRectTransform.rect.height + tViewItem1.Padding;
                            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, y, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();
                            if (nIndex > mCurReadyMaxItemIndex)
                            {
                                mCurReadyMaxItemIndex = nIndex;
                            }
                            return true;
                        }
                    }
                }

                if (mViewPortRectLocalCorners[0].y - downPos0.y < distanceForNew0)
                {
                    if (tViewItem0.ItemIndex < mCurReadyMinItemIndex)
                    {
                        mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                        mNeedCheckNextMinItem = true;
                    }
                    var nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= mCurReadyMinItemIndex || mNeedCheckNextMinItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex, listItemReleaseType: ListItemReleaseType.OnScroll);
                        if (newItem == null)
                        {
                            mNeedCheckNextMinItem = false;
                            return false;
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                            }
                            mItemList.Insert(0, newItem);
                            var y = tViewItem0.CachedRectTransform.anchoredPosition3D.y - newItem.CachedRectTransform.rect.height - newItem.Padding;
                            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, y, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();
                            if (nIndex < mCurReadyMinItemIndex)
                            {
                                mCurReadyMinItemIndex = nIndex;
                            }
                            return true;
                        }
                    }
                }
            }

            return false;
        }


        private bool UpdateForHorizontalList(float distanceForRecycle0, float distanceForRecycle1, float distanceForNew0, float distanceForNew1)
        {
            if (mItemTotalCount == 0)
            {
                if (mItemList.Count > 0)
                {
                    RecycleAllItem();
                }
                return false;
            }
            if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                if (mItemList.Count == 0)
                {
                    var curX = mContainerTrans.anchoredPosition3D.x;
                    if (curX > 0)
                    {
                        curX = 0;
                    }
                    var index = 0;
                    var pos = -curX;
                    if (mSupportScrollBar)
                    {
                        if(GetPlusItemIndexAndPosAtGivenPos(-curX, ref index, ref pos) == false)
                        {
                            return false;
                        }
                    }
                    var newItem = GetNewItemByIndex(index, listItemReleaseType: ListItemReleaseType.OnScroll);
                    if (newItem == null)
                    {
                        return false;
                    }
                    if (mSupportScrollBar)
                    {
                        SetItemSize(index, newItem.CachedRectTransform.rect.width, newItem.Padding);
                    }
                    mItemList.Add(newItem);
                    newItem.CachedRectTransform.anchoredPosition3D = new Vector3(pos, newItem.StartPosOffset, 0);
                    UpdateContentSize();
                    return true;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var leftPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var rightPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);

                if (!mIsDraging && tViewItem0.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                    && mViewPortRectLocalCorners[1].x - rightPos0.x > distanceForRecycle0)
                {
                    mItemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }

                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var leftPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var rightPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                if (!mIsDraging && tViewItem1.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                    && leftPos1.x - mViewPortRectLocalCorners[2].x> distanceForRecycle1)
                {
                    mItemList.RemoveAt(mItemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }
                
                if (rightPos1.x - mViewPortRectLocalCorners[2].x < distanceForNew1)
                {
                    if (tViewItem1.ItemIndex > mCurReadyMaxItemIndex)
                    {
                        mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                        mNeedCheckNextMaxItem = true;
                    }
                    var nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= mCurReadyMaxItemIndex || mNeedCheckNextMaxItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex, listItemReleaseType: ListItemReleaseType.OnScroll);
                        if (newItem == null)
                        {
                            mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                            mNeedCheckNextMaxItem = false;
                            CheckIfNeedUpdataItemPos();
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            }
                            mItemList.Add(newItem);
                            var x = tViewItem1.CachedRectTransform.anchoredPosition3D.x + tViewItem1.CachedRectTransform.rect.width + tViewItem1.Padding;
                            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();

                            if (nIndex > mCurReadyMaxItemIndex)
                            {
                                mCurReadyMaxItemIndex = nIndex;
                            }
                            return true;
                        }
                    }
                }

                if ( mViewPortRectLocalCorners[1].x - leftPos0.x < distanceForNew0)
                {
                    if (tViewItem0.ItemIndex < mCurReadyMinItemIndex)
                    {
                        mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                        mNeedCheckNextMinItem = true;
                    }
                    var nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= mCurReadyMinItemIndex || mNeedCheckNextMinItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex, listItemReleaseType: ListItemReleaseType.OnScroll);
                        if (newItem == null)
                        {
                            mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                            mNeedCheckNextMinItem = false;
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            }
                            mItemList.Insert(0, newItem);
                            var x = tViewItem0.CachedRectTransform.anchoredPosition3D.x - newItem.CachedRectTransform.rect.width - newItem.Padding;
                            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();
                            if (nIndex < mCurReadyMinItemIndex)
                            {
                                mCurReadyMinItemIndex = nIndex;
                            }
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (mItemList.Count == 0)
                {
                    var curX = mContainerTrans.anchoredPosition3D.x;
                    if (curX < 0)
                    {
                        curX = 0;
                    }
                    var index = 0;
                    var pos = -curX;
                    if (mSupportScrollBar)
                    {
                        if(GetPlusItemIndexAndPosAtGivenPos(curX, ref index, ref pos) == false)
                        {
                            return false;
                        }
                        pos = -pos;
                    }
                    var newItem = GetNewItemByIndex(index, listItemReleaseType: ListItemReleaseType.OnScroll);
                    if (newItem == null)
                    {
                        return false;
                    }
                    if (mSupportScrollBar)
                    {
                        SetItemSize(index, newItem.CachedRectTransform.rect.width, newItem.Padding);
                    }
                    mItemList.Add(newItem);
                    newItem.CachedRectTransform.anchoredPosition3D = new Vector3(pos, newItem.StartPosOffset, 0);
                    UpdateContentSize();
                    return true;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var leftPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var rightPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);

                if (!mIsDraging && tViewItem0.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                    && leftPos0.x - mViewPortRectLocalCorners[2].x > distanceForRecycle0)
                {
                    mItemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }

                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var leftPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var rightPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                if (!mIsDraging && tViewItem1.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                    && mViewPortRectLocalCorners[1].x - rightPos1.x > distanceForRecycle1)
                {
                    mItemList.RemoveAt(mItemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!mSupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }

                if (mViewPortRectLocalCorners[1].x - leftPos1.x  < distanceForNew1)
                {
                    if (tViewItem1.ItemIndex > mCurReadyMaxItemIndex)
                    {
                        mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                        mNeedCheckNextMaxItem = true;
                    }
                    var nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= mCurReadyMaxItemIndex || mNeedCheckNextMaxItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex, listItemReleaseType: ListItemReleaseType.OnScroll);
                        if (newItem == null)
                        {
                            mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                            mNeedCheckNextMaxItem = false;
                            CheckIfNeedUpdataItemPos();
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            }
                            mItemList.Add(newItem);
                            var x = tViewItem1.CachedRectTransform.anchoredPosition3D.x - tViewItem1.CachedRectTransform.rect.width - tViewItem1.Padding;
                            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();

                            if (nIndex > mCurReadyMaxItemIndex)
                            {
                                mCurReadyMaxItemIndex = nIndex;
                            }
                            return true;
                        }
                    }
                }

                if (rightPos0.x - mViewPortRectLocalCorners[2].x < distanceForNew0)
                {
                    if (tViewItem0.ItemIndex < mCurReadyMinItemIndex)
                    {
                        mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                        mNeedCheckNextMinItem = true;
                    }
                    var nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= mCurReadyMinItemIndex || mNeedCheckNextMinItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex, listItemReleaseType: ListItemReleaseType.OnScroll);
                        if (newItem == null)
                        {
                            mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                            mNeedCheckNextMinItem = false;
                        }
                        else
                        {
                            if (mSupportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            }
                            mItemList.Insert(0, newItem);
                            var x = tViewItem0.CachedRectTransform.anchoredPosition3D.x + newItem.CachedRectTransform.rect.width + newItem.Padding;
                            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();
                            if (nIndex < mCurReadyMinItemIndex)
                            {
                                mCurReadyMinItemIndex = nIndex;
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        private float GetContentPanelSize()
        {
            if (mSupportScrollBar)
            {
                var tTotalSize = mItemPosMgr.mTotalSize > 0 ? (mItemPosMgr.mTotalSize - mLastItemPadding) : 0;
                if(tTotalSize < 0)
                {
                    tTotalSize = 0;
                }
                return tTotalSize;
            }
            var count = mItemList.Count;
            if (count == 0)
            {
                return 0;
            }
            if (count == 1)
            {
                return mItemList[0].ItemSize;
            }
            if (count == 2)
            {
                return mItemList[0].ItemSizeWithPadding + mItemList[1].ItemSize;
            }
            float s = 0;
            for (var i = 0; i < count - 1; ++i)
            {
                s += mItemList[i].ItemSizeWithPadding;
            }
            s += mItemList[count - 1].ItemSize;
            return s;
        }


        private void CheckIfNeedUpdataItemPos()
        {
            var count = mItemList.Count;
            if (count == 0)
            {
                return;
            }
            if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                var firstItem = mItemList[0];
                var lastItem = mItemList[mItemList.Count - 1];
                var viewMaxY = GetContentPanelSize();
                if (firstItem.TopY > 0 || (firstItem.ItemIndex == mCurReadyMinItemIndex && firstItem.TopY != 0))
                {
                    UpdateAllShownItemsPos();
                    return;
                }
                if ((-lastItem.BottomY) > viewMaxY || (lastItem.ItemIndex == mCurReadyMaxItemIndex && (-lastItem.BottomY) != viewMaxY))
                {
                    UpdateAllShownItemsPos();
                    return;
                }

            }
            else if (mArrangeType == ListItemArrangeType.BottomToTop)
            {
                var firstItem = mItemList[0];
                var lastItem = mItemList[mItemList.Count - 1];
                var viewMaxY = GetContentPanelSize();
                if (firstItem.BottomY < 0 || (firstItem.ItemIndex == mCurReadyMinItemIndex && firstItem.BottomY != 0))
                {
                    UpdateAllShownItemsPos();
                    return;
                }
                if (lastItem.TopY > viewMaxY || (lastItem.ItemIndex == mCurReadyMaxItemIndex && lastItem.TopY != viewMaxY))
                {
                    UpdateAllShownItemsPos();
                    return;
                }
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                var firstItem = mItemList[0];
                var lastItem = mItemList[mItemList.Count - 1];
                var viewMaxX = GetContentPanelSize();
                if (firstItem.LeftX < 0 || (firstItem.ItemIndex == mCurReadyMinItemIndex && firstItem.LeftX != 0))
                {
                    UpdateAllShownItemsPos();
                    return;
                }
                if ((lastItem.RightX) > viewMaxX || (lastItem.ItemIndex == mCurReadyMaxItemIndex && lastItem.RightX != viewMaxX))
                {
                    UpdateAllShownItemsPos();
                    return;
                }

            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                var firstItem = mItemList[0];
                var lastItem = mItemList[mItemList.Count - 1];
                var viewMaxX = GetContentPanelSize();
                if (firstItem.RightX > 0 || (firstItem.ItemIndex == mCurReadyMinItemIndex && firstItem.RightX != 0))
                {
                    UpdateAllShownItemsPos();
                    return;
                }
                if ((-lastItem.LeftX) > viewMaxX || (lastItem.ItemIndex == mCurReadyMaxItemIndex && (-lastItem.LeftX) != viewMaxX))
                {
                    UpdateAllShownItemsPos();
                    return;
                }
            }
        }

        private void UpdateAllShownItemsPos()
        {
            var count = mItemList.Count;
            if (count == 0)
            {
                return;
            }

            mAdjustedVec = (mContainerTrans.anchoredPosition3D - mLastFrameContainerPos) / Time.deltaTime;

            if (mArrangeType == ListItemArrangeType.TopToBottom)
            {
                float pos = 0;
                if (mSupportScrollBar)
                {
                    pos = -GetItemPos(mItemList[0].ItemIndex);
                }
                var pos1 = mItemList[0].CachedRectTransform.anchoredPosition3D.y;
                var d = pos - pos1;
                var curY = pos;
                for (var i = 0; i < count; ++i)
                {
                    var item = mItemList[i];
                    item.CachedRectTransform.anchoredPosition3D = new Vector3(item.StartPosOffset, curY, 0);
                    curY = curY - item.CachedRectTransform.rect.height - item.Padding;
                }
                if(Math.Abs(d) > float.Epsilon)
                {
                    Vector2 p = mContainerTrans.anchoredPosition3D;
                    p.y = p.y - d;
                    mContainerTrans.anchoredPosition3D = p;
                }
                
            }
            else if(mArrangeType == ListItemArrangeType.BottomToTop)
            {
                float pos = 0;
                if (mSupportScrollBar)
                {
                    pos = GetItemPos(mItemList[0].ItemIndex);
                }
                var pos1 = mItemList[0].CachedRectTransform.anchoredPosition3D.y;
                var d = pos - pos1;
                var curY = pos;
                for (var i = 0; i < count; ++i)
                {
                    var item = mItemList[i];
                    item.CachedRectTransform.anchoredPosition3D = new Vector3(item.StartPosOffset, curY, 0);
                    curY = curY + item.CachedRectTransform.rect.height + item.Padding;
                }
                if(d != 0)
                {
                    var p = mContainerTrans.anchoredPosition3D;
                    p.y = p.y - d;
                    mContainerTrans.anchoredPosition3D = p;
                }
            }
            else if (mArrangeType == ListItemArrangeType.LeftToRight)
            {
                float pos = 0;
                if (mSupportScrollBar)
                {
                    pos = GetItemPos(mItemList[0].ItemIndex);
                }
                var pos1 = mItemList[0].CachedRectTransform.anchoredPosition3D.x;
                var d = pos - pos1;
                var curX = pos;
                for (var i = 0; i < count; ++i)
                {
                    var item = mItemList[i];
                    item.CachedRectTransform.anchoredPosition3D = new Vector3(curX, item.StartPosOffset, 0);
                    curX = curX + item.CachedRectTransform.rect.width + item.Padding;
                }
                if (d != 0)
                {
                    var p = mContainerTrans.anchoredPosition3D;
                    p.x = p.x - d;
                    mContainerTrans.anchoredPosition3D = p;
                }

            }
            else if (mArrangeType == ListItemArrangeType.RightToLeft)
            {
                float pos = 0;
                if (mSupportScrollBar)
                {
                    pos = -GetItemPos(mItemList[0].ItemIndex);
                }
                var pos1 = mItemList[0].CachedRectTransform.anchoredPosition3D.x;
                var d = pos - pos1;
                var curX = pos;
                for (var i = 0; i < count; ++i)
                {
                    var item = mItemList[i];
                    item.CachedRectTransform.anchoredPosition3D = new Vector3(curX, item.StartPosOffset, 0);
                    curX = curX - item.CachedRectTransform.rect.width - item.Padding;
                }
                if (d != 0)
                {
                    var p = mContainerTrans.anchoredPosition3D;
                    p.x = p.x - d;
                    mContainerTrans.anchoredPosition3D = p;
                }

            }
            if (mIsDraging)
            {
                mScrollRect.OnBeginDrag(mPointerEventData);
                mScrollRect.Rebuild(CanvasUpdate.PostLayout);
                mScrollRect.velocity = mAdjustedVec;
                mNeedAdjustVec = true;
            }
        }
        
        private void UpdateContentSize()
        {
            var size = GetContentPanelSize();
            if (mIsVertList)
            {
                if(Math.Abs(mContainerTrans.rect.height - size) > float.Epsilon)
                {
                    mContainerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
                }
            }
            else
            {
                if(Math.Abs(mContainerTrans.rect.width - size) > float.Epsilon)
                {
                    mContainerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                }
            }
        }
    }
}
