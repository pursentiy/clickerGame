using Components.UI;
using UnityEngine;

namespace UI.Screens.ChoosePack.Widgets
{
    public class PacksWidget : MonoBehaviour
    {
        [SerializeField] private FreemiumPackInitializerWidget _freemiumPackInitializerWidget;
        [SerializeField] private DefaultPackInitializerWidget _defaultPackInitializerWidget;

        public void Initialize(CurrencyDisplayWidget currencyDisplayWidget, AdsButtonWidget adsButtonWidget)
        {
            _freemiumPackInitializerWidget?.Initialize(currencyDisplayWidget, adsButtonWidget);
            _defaultPackInitializerWidget?.Initialize(currencyDisplayWidget, adsButtonWidget);
        }
        
        public void UpdatePacksState()
        {
            _freemiumPackInitializerWidget?.UpdatePacksState();
            _defaultPackInitializerWidget?.UpdatePacksState();
        }
    }
}