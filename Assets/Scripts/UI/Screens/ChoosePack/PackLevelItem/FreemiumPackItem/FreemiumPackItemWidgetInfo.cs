using System;
using System.Collections.Generic;
using Common.Currency;
using Configurations.Progress;
using UnityEngine;
using UI.Screens.ChoosePack.PackLevelItem.Base;
using UI.Screens.ChoosePack.PackLevelItem.Base.PackClickAction;

namespace UI.Screens.ChoosePack.PackLevelItem.FreemiumPackItem
{
    public class FreemiumPackItemWidgetInfo : BasePackItemWidgetInfo
    {
        public FreemiumPackItemWidgetInfo(string packName, GameObject packImagePrefab, int packId, PackStatus packStatus,
            Action<IPackClickAction> onPackClicked, List<ICurrency> currencyToUnlock, int indexInList = 0,
            Func<bool> getEntranceAnimationsAlreadyTriggered = null)
            : base(packName, packImagePrefab, packId, packStatus, onPackClicked, currencyToUnlock, indexInList, getEntranceAnimationsAlreadyTriggered)
        {
        }
    }
}
