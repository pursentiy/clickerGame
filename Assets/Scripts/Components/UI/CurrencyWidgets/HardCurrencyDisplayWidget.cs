using System;
using Common.Currency;
using Components.UI.Base;

namespace Components.UI.CurrencyWidgets
{
    public class HardCurrencyDisplayWidget : SingleCurrencyDisplayWidgetBase
    {
        public override Type CurrencyType => typeof(HardCurrency);
    }
}