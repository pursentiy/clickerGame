using System;
using System.Collections.Generic;
using Common.Currency;
using Extensions;
using UnityEngine;
using Installers;
using RSG;
using Services.Player;
using Utilities.Disposable;
using Zenject;

namespace Components.UI
{
    [Serializable]
    public class CurrencyDisplayEntry
    {
        public CurrencyType CurrencyType;
        public SingleCurrencyDisplayWidget DisplayWidget;
    }

    public class CurrencyDisplayWidget : InjectableMonoBehaviour
    {
        [Inject] private readonly PlayerCurrencyManager _playerCurrencyManager;
        
        [Header("Currency Display Mappings")]
        [SerializeField] private List<CurrencyDisplayEntry> _currencyDisplays = new();
        
        [Header("Default Currency (for backward compatibility)")]
        [SerializeField] private CurrencyType _defaultCurrencyType = CurrencyType.Stars;

        private Dictionary<CurrencyType, SingleCurrencyDisplayWidget> _displayMap;

        public RectTransform AnimationTarget => GetAnimationTarget(_defaultCurrencyType);

        public void Start()
        {
            BuildDisplayMap();
            _playerCurrencyManager.CurrencyChangedSignal.MapListener(OnCurrencyChanged).DisposeWith(this);
        }

        private void BuildDisplayMap()
        {
            _displayMap = new Dictionary<CurrencyType, SingleCurrencyDisplayWidget>();
            foreach (var entry in _currencyDisplays)
            {
                if (entry.DisplayWidget != null && !_displayMap.ContainsKey(entry.CurrencyType))
                {
                    _displayMap[entry.CurrencyType] = entry.DisplayWidget;
                }
            }
        }

        private void OnCurrencyChanged(ICurrency newValue, CurrencyChangeMode mode)
        {
            if (mode != CurrencyChangeMode.Instant)
                return;

            var currencyType = CurrencyParser.GetCurrencyType(newValue);
            if (TryGetDisplayWidget(currencyType, out var widget))
            {
                widget.SetCurrency(newValue, false);
            }
        }

        public void SetCurrency(ICurrency currency, bool withAnimation = false)
        {
            if (currency == null) return;
            
            var currencyType = CurrencyParser.GetCurrencyType(currency);
            if (TryGetDisplayWidget(currencyType, out var widget))
            {
                widget.SetCurrency(currency, withAnimation);
            }
        }

        // public void SetCurrency(long value, bool withAnimation = false)
        // {
        //     SetCurrencyValue(_defaultCurrencyType, value, withAnimation);
        // }

        public void SetCurrencyValue(CurrencyType currencyType, long value, bool withAnimation = false)
        {
            if (TryGetDisplayWidget(currencyType, out var widget))
            {
                widget.SetValue(value, withAnimation);
            }
        }

        public IPromise Bump()
        {
            return Bump(_defaultCurrencyType);
        }

        public IPromise Bump(CurrencyType currencyType)
        {
            if (TryGetDisplayWidget(currencyType, out var widget))
            {
                return widget.Bump();
            }
            return Promise.Resolved();
        }

        public IPromise BumpAll()
        {
            var promises = new List<IPromise>();
            foreach (var widget in _displayMap.Values)
            {
                promises.Add(widget.Bump());
            }
            return Promise.All(promises);
        }

        public bool TryGetDisplayWidget(CurrencyType currencyType, out SingleCurrencyDisplayWidget widget)
        {
            if (_displayMap == null)
                BuildDisplayMap();
            
            return _displayMap.TryGetValue(currencyType, out widget);
        }

        public SingleCurrencyDisplayWidget GetDisplayWidget(CurrencyType currencyType)
        {
            TryGetDisplayWidget(currencyType, out var widget);
            return widget;
        }

        public RectTransform GetAnimationTarget(CurrencyType currencyType)
        {
            if (TryGetDisplayWidget(currencyType, out var widget))
            {
                return widget.AnimationTarget;
            }
            return null;
        }

        public RectTransform GetAnimationTarget(ICurrency currency)
        {
            var currencyType = CurrencyParser.GetCurrencyType(currency);
            return GetAnimationTarget(currencyType);
        }
    }
}