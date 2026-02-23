using System;
using System.Collections.Generic;
using Common.Currency;
using UnityEngine;
using UI.Screens.ChoosePack.PackLevelItem.Base;
using UI.Screens.ChoosePack.PackLevelItem.Base.PackClickAction;

namespace UI.Screens.ChoosePack.PackLevelItem.FreemiumPackItem
{
    public class FreemiumPackItemWidgetInfo : BasePackItemWidgetInfo
    {
        public FreemiumPackItemWidgetInfo(string packName, GameObject packImagePrefab, int packId, bool isUnlocked,
            Action<IPackClickAction> onPackClicked, List<ICurrency> currencyToUnlock, int indexInList = 0,
            Func<bool> getEntranceAnimationsAlreadyTriggered = null)
            : base(packName, packImagePrefab, packId, isUnlocked, onPackClicked, currencyToUnlock, indexInList, getEntranceAnimationsAlreadyTriggered)
        {
        }
    }
}
