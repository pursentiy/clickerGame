using System;
using System.Collections.Generic;
using Common.Currency;
using Components.UI.Base;
using Extensions;
using UnityEngine;
using Installers;
using RSG;
using Services;
using Services.Player;
using Utilities.Disposable;
using Zenject;

namespace Components.UI
{
    public class CurrencyDisplayWidget : InjectableMonoBehaviour
    {
        [Inject] private readonly PlayerCurrencyManager _playerCurrencyManager;
        
        [Header("Currency Display Mappings")]
        [SerializeField] private List<SingleCurrencyDisplayWidgetBase> _currencyDisplayWidgets = new();

        private Dictionary<Type, SingleCurrencyDisplayWidgetBase> _displayMap;

        public void Start()
        {
            BuildDisplayMap();
            _playerCurrencyManager.CurrencyChangedSignal.MapListener(OnCurrencyChanged).DisposeWith(this);
        }

        private void BuildDisplayMap()
        {
            _displayMap = new Dictionary<Type, SingleCurrencyDisplayWidgetBase>();
            foreach (var widgetBase in _currencyDisplayWidgets)
            {
                if (widgetBase != null)
                {
                    _displayMap.TryAdd(widgetBase.CurrencyType, widgetBase);
                }
            }
        }

        private void OnCurrencyChanged(ICurrency newValue, CurrencyChangeMode mode)
        {
            if (mode != CurrencyChangeMode.Instant)
                return;
            
            if (TryGetDisplayWidget(newValue.GetType(), out var widget))
            {
                widget.SetCurrency(newValue, false);
            }
        }

        public void SetCurrency(ICurrency currency, bool withAnimation = false)
        {
            if (TryGetDisplayWidget(currency.GetType(), out var widget))
            {
                widget.SetCurrency(currency, withAnimation);
            }
        }

        public void SetCurrencyValue(Type currencyType, long value, bool withAnimation = false)
        {
            if (TryGetDisplayWidget(currencyType, out var widget))
            {
                widget.SetValue(value, withAnimation);
            }
        }

        public IPromise Bump(Type currencyType)
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

        public bool TryGetDisplayWidget(Type currencyType, out SingleCurrencyDisplayWidgetBase widgetBase)
        {
            if (_displayMap == null)
                BuildDisplayMap();

            widgetBase = null;
            if (_displayMap == null)
                return false;
            
            return _displayMap.TryGetValue(currencyType, out widgetBase);
        }

        public SingleCurrencyDisplayWidgetBase GetDisplayWidget(Type currencyType)
        {
            TryGetDisplayWidget(currencyType, out var widget);
            return widget;
        }

        public RectTransform GetAnimationTarget(Type currencyType)
        {
            if (TryGetDisplayWidget(currencyType, out var widget))
            {
                return widget.AnimationTarget;
            }
            return null;
        }

        public Vector3 GetAnimationTarget(ICurrency currency)
        {
            var rectTransform = GetAnimationTarget(currency.GetType());

            if (rectTransform == null)
            {
                LoggerService.LogWarning(this, $"Target for {currency.GetType().Name} not found at {nameof(GetAnimationTarget)}");
                return Vector3.zero;
            }
            
            return rectTransform.position;
        }
    }
}