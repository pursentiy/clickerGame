using System;
using Plugins.RotaryHeart;
using UnityEngine;

namespace Common.Rewards
{
    [Serializable]
    public class CurrencyInfoData
    {
        public Sprite MainIcon;
    }
    
    [Serializable]
    public class CurrencyDataByString : UnityDictionary<string, CurrencyInfoData>
    {
        
    }
}