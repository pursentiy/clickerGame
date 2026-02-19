using System;
using UnityEngine;
using UI.Screens.ChoosePack.PackLevelItem.Base;

namespace UI.Screens.ChoosePack.PackLevelItem.DefaultPackItem
{
    public class DefaultPackItemWidgetInfo : BasePackItemWidgetInfo
    {
        public DefaultPackItemWidgetInfo(string packName, GameObject packImagePrefab, int packId, bool isUnlocked,
            Action onClickAction, Action onLockedClickAction, int starsRequired)
            : base(packName, packImagePrefab, packId, isUnlocked, onClickAction, onLockedClickAction, starsRequired)
        {
        }
    }
}
