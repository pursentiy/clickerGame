using System;
using System.Collections.Generic;
using Common.Currency;
using UI.Screens.ChoosePack.PackLevelItem.Base.PackClickAction;
using UnityEngine;

namespace UI.Screens.ChoosePack.PackLevelItem.Base
{
    public abstract class BasePackItemWidgetInfo
    {
        private readonly Func<bool> _getEntranceAnimationsAlreadyTriggered;

        public string PackName { get; }
        public GameObject PackImagePrefab { get; }
        public int PackId { get; }
        public bool IsUnlocked { get; set; }
        /// <summary>Single click handler; receives <see cref="IPackClickAction"/> (e.g. popup anchor, extensible). Handler decides behavior from current state (e.g. GetPackStatus).</summary>
        public Action<IPackClickAction> OnClickAction { get; }
        public IReadOnlyList<ICurrency> CurrencyToUnlock { get; }
        public int IndexInList { get; }
        public bool EntranceAnimationRequested { get; set; }

        protected BasePackItemWidgetInfo(string packName, GameObject packImagePrefab, int packId, bool isUnlocked,
            Action<IPackClickAction> onPackClicked, List<ICurrency> currencyToUnlock, int indexInList = 0,
            Func<bool> getEntranceAnimationsAlreadyTriggered = null)
        {
            PackName = packName;
            PackImagePrefab = packImagePrefab;
            PackId = packId;
            IsUnlocked = isUnlocked;
            OnClickAction = onPackClicked;
            CurrencyToUnlock = currencyToUnlock ?? new List<ICurrency>();
            IndexInList = indexInList;
            _getEntranceAnimationsAlreadyTriggered = getEntranceAnimationsAlreadyTriggered;
        }

        public bool GetEntranceAnimationsAlreadyTriggered() => _getEntranceAnimationsAlreadyTriggered?.Invoke() ?? false;
    }
}
