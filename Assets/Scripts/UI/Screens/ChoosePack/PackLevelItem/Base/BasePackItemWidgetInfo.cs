using System;
using UnityEngine;

namespace UI.Screens.ChoosePack.PackLevelItem.Base
{
    public abstract class BasePackItemWidgetInfo
    {
        protected BasePackItemWidgetInfo(string packName, GameObject packImagePrefab, int packId, bool isUnlocked,
            Action onClickAction, Action onLockedClickAction, int starsRequired, int indexInList = 0,
            Func<bool> getEntranceAnimationsAlreadyTriggered = null)
        {
            PackName = packName;
            PackImagePrefab = packImagePrefab;
            PackId = packId;
            IsUnlocked = isUnlocked;
            OnClickAction = onClickAction;
            OnLockedClickAction = onLockedClickAction;
            StarsRequired = starsRequired;
            IndexInList = indexInList;
            _getEntranceAnimationsAlreadyTriggered = getEntranceAnimationsAlreadyTriggered;
        }

        public string PackName { get; }
        public GameObject PackImagePrefab { get; }
        public int PackId { get; }
        public bool IsUnlocked { get; set; }
        public Action OnClickAction { get; }
        public Action OnLockedClickAction { get; }
        public int StarsRequired { get; }
        public int IndexInList { get; }
        private readonly Func<bool> _getEntranceAnimationsAlreadyTriggered;

        /// <summary>
        /// Set to true when entrance animation should play on next View initialization.
        /// </summary>
        public bool EntranceAnimationRequested { get; set; }

        public bool GetEntranceAnimationsAlreadyTriggered() => _getEntranceAnimationsAlreadyTriggered?.Invoke() ?? false;
    }
}
