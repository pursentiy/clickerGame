

using System;
using System.Collections.Generic;
using Common.Currency;
using Components.UI;
using Configurations.Progress;
using UnityEngine;
using Utilities.StateMachine;

namespace UI.Screens.ChoosePack.Widgets.PacksInitializer.Sequences.UnlockFreemiumPackSequence
{
    public class UnlockFreemiumPackSequenceContext : IStateContext
    {
        public UnlockFreemiumPackSequenceContext(List<ICurrency> packCost, int packId, CurrencyDisplayWidget currencyDisplayWidget, RectTransform packTransform, Action updatePacksAction, PackType packType, RectTransform visualizerFlightRewardsContainer)
        {
            PackCost = packCost;
            PackId = packId;
            CurrencyDisplayWidget = currencyDisplayWidget;
            PackTransform = packTransform;
            UpdatePacksAction = updatePacksAction;
            PackType = packType;
            VisualizerFlightRewardsContainer = visualizerFlightRewardsContainer;
        }

        public List<ICurrency> PackCost { get; }
        public int PackId { get; }
        public CurrencyDisplayWidget CurrencyDisplayWidget { get; }
        public RectTransform PackTransform { get; }
        public Action UpdatePacksAction { get; }
        public PackType  PackType { get; }
        public RectTransform VisualizerFlightRewardsContainer { get; }
    }
}