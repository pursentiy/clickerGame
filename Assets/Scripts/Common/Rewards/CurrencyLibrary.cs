using Extensions;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Common.Rewards
{
    [CreateAssetMenu(fileName = "CurrencyLibrary", menuName = "ScriptableObjects/CurrencyLibrary")]
    public sealed class CurrencyLibrary : ScriptableObject
    {
        [SerializeField] private CurrencyDataByString _currencyData;
        [SerializeField] private AssetReference _rewardAnimationItem;
        
        public AssetReference RewardAnimationItem => _rewardAnimationItem;
        public CurrencyDataByString CurrencyData => _currencyData;
        public CurrencyInfoData DefaultCurrencyData => _currencyData.GetValueOrFirst("Default");
        public bool HasCurrencyData(string currencyName) => _currencyData.ContainsKey(currencyName);
    }
}