using Components.UI;
using UI.Screens.ChoosePack.Widgets;
using UnityEngine;
using Utilities.StateMachine;

namespace UI.Screens.ChoosePack.NoCurrencySequence
{
    public class VisualizeNotEnoughCurrencyContext : IStateContext
    {
        public VisualizeNotEnoughCurrencyContext(CurrencyDisplayWidget currencyDisplayWidget, AdsButtonWidget adsButtonWidget)
        {
            CurrencyDisplayWidget = currencyDisplayWidget;
            AdsButtonWidget = adsButtonWidget;
        }

        public CurrencyDisplayWidget CurrencyDisplayWidget { get; }
        public AdsButtonWidget AdsButtonWidget { get; }
    }
}