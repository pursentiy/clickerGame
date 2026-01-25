using System;
using Plugins.RotaryHeart;
using TMPro;
using UnityEngine;

namespace Common.Rewards
{
    [Serializable]
    public class CurrencyInfoData
    {
        public Sprite MainIcon;
        public TMP_SpriteAsset SpriteAsset;
    }
    
    [Serializable]
    public class CurrencyDataByString : UnityDictionary<string, CurrencyInfoData>
    {
        
    }
}