using Components.UI;
using UI.Screens.ChoosePack.Widgets.PacksInitializer;
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

        public void PlayEntranceAnimations()
        {
            _freemiumPackInitializerWidget?.RequestItemsEntranceAnimation();
            _defaultPackInitializerWidget?.RequestItemsEntranceAnimation();
        }

        public void PlayExitAnimations()
        {
            _freemiumPackInitializerWidget?.PlayExitAnimations();
            _defaultPackInitializerWidget?.PlayExitAnimations();
        }
    }
}