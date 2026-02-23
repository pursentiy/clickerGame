using System;
using Components.UI;
using UnityEngine;
using Utilities.StateMachine;

namespace UI.Screens.ChoosePack.Widgets.PacksInitializer.Sequences.UnlockFreemiumPackSequence
{
    public class UnlockFreemiumPackSequenceContext : IStateContext
    {
        public CurrencyDisplayWidget CurrencyDisplayWidget { get; }
        public RectTransform PackTransform { get; }
        public Action UpdatePacksAction { get; }
    }
}