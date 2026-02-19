using System;
using UnityEngine;
using UI.Screens.ChoosePack.PackLevelItem.Base;

namespace UI.Screens.ChoosePack.PackLevelItem.FreemiumPackItem
{
    public class FreemiumPackItemWidgetInfo : BasePackItemWidgetInfo
    {
        public FreemiumPackItemWidgetInfo(string packName, GameObject packImagePrefab, int packId, bool isUnlocked,
            Action onClickAction, Action onLockedClickAction, int starsRequired)
            : base(packName, packImagePrefab, packId, isUnlocked, onClickAction, onLockedClickAction, starsRequired)
        {
        }
    }
}
