using System;
using System.Collections.Generic;
using Common.Currency;
using Components.UI;
using Controllers;
using RSG;
using UI.Screens.ChoosePack.Widgets;
using UnityEngine;
using Utilities;
using Utilities.Disposable;
using Utilities.StateMachine;

namespace UI.Screens.ChoosePack.NoCurrencySequence
{
    public class VisualizeNotEnoughCurrencyContext : IStateContext
    {
        public VisualizeNotEnoughCurrencyContext(CurrencyDisplayWidget currencyDisplayWidget, AdsButtonWidget adsButtonWidget, List<ICurrency> desiredCurrency, Func<IDisposeProvider, IPromise<MediatorFlowInfo>> showMessagePopup)
        {
            CurrencyDisplayWidget = currencyDisplayWidget;
            AdsButtonWidget = adsButtonWidget;
            DesiredCurrency = desiredCurrency ?? new List<ICurrency>();
            ShowMessagePopup = showMessagePopup;
        }

        public CurrencyDisplayWidget CurrencyDisplayWidget { get; }
        public AdsButtonWidget AdsButtonWidget { get; }
        public List<ICurrency> DesiredCurrency { get; }
        public Func<IDisposeProvider, IPromise<MediatorFlowInfo>> ShowMessagePopup { get; }
    }
}