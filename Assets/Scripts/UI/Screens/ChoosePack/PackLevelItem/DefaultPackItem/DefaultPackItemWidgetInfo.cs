using System;
using System.Collections.Generic;
using Common.Currency;
using UnityEngine;
using UI.Screens.ChoosePack.PackLevelItem.Base;
using UI.Screens.ChoosePack.PackLevelItem.Base.PackClickAction;

namespace UI.Screens.ChoosePack.PackLevelItem.DefaultPackItem
{
    public class DefaultPackItemWidgetInfo : BasePackItemWidgetInfo
    {
        public DefaultPackItemWidgetInfo(string packName, GameObject packImagePrefab, int packId, bool isUnlocked,
            Action<IPackClickAction> onPackClicked, List<ICurrency> currencyToUnlock, int indexInList = 0,
            Func<bool> getEntranceAnimationsAlreadyTriggered = null)
            : base(packName, packImagePrefab, packId, isUnlocked, onPackClicked, currencyToUnlock, indexInList, getEntranceAnimationsAlreadyTriggered)
        {
        }
    }
}
