using Components.UI;
using UI.Screens.ChoosePack.Widgets;

namespace UI.Screens.ChoosePack.PackLevelItem
{
    public class PackItemWidgetInfo
    {
        public PackItemWidgetInfo(CurrencyDisplayWidget currencyDisplayWidget, AdsButtonWidget adsButtonWidget)
        {
            CurrencyDisplayWidget = currencyDisplayWidget;
            AdsButtonWidget = adsButtonWidget;
        }

        public CurrencyDisplayWidget CurrencyDisplayWidget { get; }
        public AdsButtonWidget AdsButtonWidget { get; }
    }
}