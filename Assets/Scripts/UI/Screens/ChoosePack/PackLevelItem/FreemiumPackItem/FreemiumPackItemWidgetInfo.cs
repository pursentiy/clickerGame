using System;
using UnityEngine;
using UI.Screens.ChoosePack.PackLevelItem.Base;

namespace UI.Screens.ChoosePack.PackLevelItem.FreemiumPackItem
{
    public class FreemiumPackItemWidgetInfo : BasePackItemWidgetInfo
    {
        public FreemiumPackItemWidgetInfo(string packName, GameObject packImagePrefab, int packId, bool isUnlocked,
            Action onClickAction, Action onLockedClickAction, int starsRequired, int indexInList = 0,
            Func<bool> getEntranceAnimationsAlreadyTriggered = null)
            : base(packName, packImagePrefab, packId, isUnlocked, onClickAction, onLockedClickAction, starsRequired, indexInList, getEntranceAnimationsAlreadyTriggered)
        {
        }
    }
}
