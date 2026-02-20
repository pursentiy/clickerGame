using System;
using Common.Currency;
using UnityEngine;
using UI.Screens.ChoosePack.PackLevelItem.Base;

namespace UI.Screens.ChoosePack.PackLevelItem.DefaultPackItem
{
    public class DefaultPackItemWidgetInfo : BasePackItemWidgetInfo
    {
        public DefaultPackItemWidgetInfo(string packName, GameObject packImagePrefab, int packId, bool isUnlocked,
            Action onClickAction, Action onLockedClickAction, ICurrency currencyToUnlock, int indexInList = 0,
            Func<bool> getEntranceAnimationsAlreadyTriggered = null)
            : base(packName, packImagePrefab, packId, isUnlocked, onClickAction, onLockedClickAction, currencyToUnlock, indexInList, getEntranceAnimationsAlreadyTriggered)
        {
        }
    }
}
