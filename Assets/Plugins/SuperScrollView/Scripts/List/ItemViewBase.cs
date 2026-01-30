using System;
using System.ComponentModel;
using Extensions;
using Installers;
using JetBrains.Annotations;
using Platform.Common.Components.List;
using Plugins.SuperScrollView.Scripts.ListView;
using SuperScrollView;
using UnityEngine;
using Utilities;
using Utilities.Disposable;

namespace Plugins.SuperScrollView.Scripts.List
{
    public abstract class ItemViewBase : MonoBehaviour, IItemView, INotifyPropertyChanged, IDisposeProvider
    {
        public event Action<ListItemReleaseType> OnRelease;
        public event PropertyChangedEventHandler PropertyChanged;

        private static int _globalId;
        private int _localId;

        private bool _injected;

        protected virtual void Awake()
        {
            _localId = _globalId++;
            Inject();
        }

        public void Inject()
        {
            if (_injected)
            {
                return;
            }

            ContainerHolder.CurrentContainer.Inject(this);
            _injected = true;
        }
        
        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertiesChanged(params string[] properties)
        {
            foreach (var property in properties)
            {
                OnPropertyChanged(property);
            }
        }

        public virtual void Release(ListItemReleaseType type = ListItemReleaseType.Default)
        {
            OnRelease.SafeInvoke(type);
            DisposeService.HandledDispose(this);
            OnRelease = null;
        }

        protected virtual void OnDestroy()
        {
            Release(ListItemReleaseType.OnDestroy);
        }

        public DisposableCollection ChildDisposables { get; } = new DisposableCollection();
        public string GetDescription()
        {
            return $"{GetType().Name}:{_localId}";
        }
    }
}