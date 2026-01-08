using Common.Rewards;
using RSG;
using Services.ContentDeliveryService;
using UnityEngine;
using Zenject;

namespace Services.FlyingRewardsAnimation
{
    public class CurrencyLibraryService
    {
        [Inject] private CurrencyLibrary _currencyLibrary;
        [Inject] private readonly AddressableContentDeliveryService _contentDeliveryService;
        
        public IPromise<IDisposableContent<GameObject>> InstantiateAsync2DRewardAnimation(Transform parent)
        {
            return _contentDeliveryService.InstantiateAsync(_currencyLibrary.RewardAnimationItem, parent);
        }
        
        public Sprite GetMainIcon(string currencyName)
        {
            var currencyData = GetCurrencyData(currencyName);
            
            if (currencyData.MainIcon == null)
            {
                LoggerService.LogError($"CurrencyLibrary: MainIcon of {currencyName} not found. Default MainIcon was used");
                return _currencyLibrary.DefaultCurrencyData.MainIcon;
            }

            return currencyData.MainIcon;
        }
        
        public CurrencyInfoData GetCurrencyData(string currencyName)
        {
            if (!TryGetCurrencyData(currencyName, out var data))
                LoggerService.LogError($"CurrencyLibrary: {nameof(CurrencyInfoData)} of {currencyName} not found. Default {nameof(CurrencyInfoData)} was used");      
            
            return data;
        }
        
        private bool TryGetCurrencyData(string currencyName, out CurrencyInfoData data)
        {
            if (_currencyLibrary.HasCurrencyData(currencyName))
            {
                data = _currencyLibrary.CurrencyData[currencyName];
                return true;
            }
            
            data = _currencyLibrary.DefaultCurrencyData;
            return false;
        }
    }
}