using System.Collections.Generic;
using Common.Currency;
using Components.UI;
using UI.Screens.ChoosePack.Widgets;
using UnityEngine;
using Utilities.StateMachine;

namespace UI.Screens.ChoosePack.NoCurrencySequence
{
    public class VisualizeNotEnoughCurrencyContext : IStateContext
    {
        public VisualizeNotEnoughCurrencyContext(CurrencyDisplayWidget currencyDisplayWidget, AdsButtonWidget adsButtonWidget, List<ICurrency> desiredCurrency)
        {
            CurrencyDisplayWidget = currencyDisplayWidget;
            AdsButtonWidget = adsButtonWidget;
            DesiredCurrency = desiredCurrency ?? new List<ICurrency>();
        }

        public CurrencyDisplayWidget CurrencyDisplayWidget { get; }
        public AdsButtonWidget AdsButtonWidget { get; }
        public List<ICurrency> DesiredCurrency { get; }
    }
}