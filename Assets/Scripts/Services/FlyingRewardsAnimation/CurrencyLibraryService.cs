using Common.Currency;
using UnityEngine;

namespace Services.FlyingRewardsAnimation
{
    public class CurrencyLibraryService
    {
        public Sprite GetMainIcon(ICurrency currency, ICurrencyDescriptor descriptorData = null)
        {
            var currencyName = _rewardInfoProvider.GetPrefabName(currency);
            
            return GetSkinMainIcon(currencyName, _skinCurrencyService.GetCurrencySkin(skinCurrency, descriptorData));
        }
        
        public Sprite GetSkinMainIcon(string currencyName, string currencySkin)
        {
            if (currencyName == currencySkin)
            {
                return GetStandardMainIcon(currencyName);
            }
            var currencyData = GetCurrencyData(currencyName, _currentSkin);

            var hasSkinIcon = currencyData.SkinCurrencyInfoData.IsNullOrEmpty() ||
                              !currencyData.SkinCurrencyInfoData.ContainsKey(currencySkin) ||
                              currencyData.SkinCurrencyInfoData[currencySkin].MainIcon == null;
            if (hasSkinIcon)
            {
                ProLogger.LogError($"CurrencyLibrary: MainIcon of {currencyName} is either null or not found for skin currency {currencySkin}. Standard was used");
                return GetStandardMainIcon(currencyName);
            }
            
            return currencyData.SkinCurrencyInfoData[currencySkin].MainIcon;
        }
        
        public Sprite GetStandardMainIcon(string currencyName)
        {
            var currencyData = GetCurrencyData(currencyName);
            
            if (currencyData.StandardCurrencyInfoData.MainIcon == null)
            {
                LoggerService.LogError($"CurrencyLibrary: MainIcon of {currencyName} not found. Default MainIcon was used");
                return _currencyLibrary.DefaultCurrencyData.StandardCurrencyInfoData.MainIcon;
            }

            return currencyData.StandardCurrencyInfoData.MainIcon;
        }
        
        public CurrencyData GetCurrencyData(string currencyName, string skin)
        {
            if (!TryGetCurrencyData(currencyName, skin, out var data))
                _silentErrorHandler.HandleProdSilentError($"CurrencyLibrary: CurrencyData of {currencyName} not found. Default CurrencyData was used");      
            return data;
        }
        
        private bool TryGetCurrencyData(string currencyName, string skin, out CurrencyData data)
        {
            if (currencyName == "Coins" && skin != DefaultSkin)
            {
                if (CoinsSkinDatas.TryGetValue(skin, out var skinData))
                {
                    if (skinData != null)
                    {
                        data = skinData.CurrencyData;
                        return true;
                    }
                }
                
                data = CoinsSkinData.CurrencyData;
                return true;
            }
            
            if (_currencyLibrary.HasCurrencyData(currencyName))
            {
                data = _currencyLibrary.CurrencyDatas[currencyName];
                return true;
            }
            
            data = _currencyLibrary.DefaultCurrencyData;
            return false;
        }
    }
}