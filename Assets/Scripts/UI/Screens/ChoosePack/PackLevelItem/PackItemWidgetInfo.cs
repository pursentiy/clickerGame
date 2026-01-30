using System;
using UnityEngine;

namespace UI.Screens.ChoosePack.PackLevelItem
{
    public class PackItemWidgetInfo
    {
        public PackItemWidgetInfo(string packName, GameObject packImagePrefab, int packId, bool isUnlocked,
            Action onClickAction, Action onLockedClickAction, int starsRequired)
        {
            PackName = packName;
            PackImagePrefab = packImagePrefab;
            PackId = packId;
            IsUnlocked = isUnlocked;
            OnClickAction = onClickAction;
            OnLockedClickAction = onLockedClickAction;
            StarsRequired = starsRequired;
        }

        public string PackName { get; }
        public GameObject PackImagePrefab { get; }
        public int PackId { get; }
        public bool IsUnlocked { get; }
        public Action OnClickAction { get; }
        public Action OnLockedClickAction { get; }
        public int StarsRequired { get; }
    }
}