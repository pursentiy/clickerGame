using System;
using Components.UI;
using UnityEngine;
using Utilities.StateMachine;

namespace UI.Screens.ChoosePack.Widgets.PacksInitializer.Sequences.UnlockFreemiumPackSequence
{
    public class UnlockFreemiumPackSequenceContext : IStateContext
    {
        public UnlockFreemiumPackSequenceContext(CurrencyDisplayWidget currencyDisplayWidget, RectTransform packTransform, Action updatePacksAction, int packId)
        {
            CurrencyDisplayWidget = currencyDisplayWidget;
            PackTransform = packTransform;
            UpdatePacksAction = updatePacksAction;
            PackId = packId;
        }

        public CurrencyDisplayWidget CurrencyDisplayWidget { get; }
        public RectTransform PackTransform { get; }
        public Action UpdatePacksAction { get; }
        public int PackId { get; }
    }
}