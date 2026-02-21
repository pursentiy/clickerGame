using System;
using Common.Currency;
using Components.UI.Base;

namespace Components.UI.CurrencyWidgets
{
    public class StarsDisplayWidget : SingleCurrencyDisplayWidgetBase
    {
        public override Type CurrencyType => typeof(Stars);
    }
}