using System;
using Common.Currency;
using UnityEngine;

namespace UI.Screens.ChoosePack.PackLevelItem
{
    public class PackItemWidgetInfo
    {
        public PackItemWidgetInfo(string packName, GameObject packImagePrefab, int packId, bool isUnlocked,
            Action onClickAction, Action onLockedClickAction, ICurrency currencyToUnlock)
        {
            PackName = packName;
            PackImagePrefab = packImagePrefab;
            PackId = packId;
            IsUnlocked = isUnlocked;
            OnClickAction = onClickAction;
            OnLockedClickAction = onLockedClickAction;
            CurrencyToUnlock = currencyToUnlock;
        }

        public string PackName { get; }
        public GameObject PackImagePrefab { get; }
        public int PackId { get; }
        public bool IsUnlocked { get; set; }
        public Action OnClickAction { get; }
        public Action OnLockedClickAction { get; }
        public ICurrency CurrencyToUnlock { get; }
    }
}