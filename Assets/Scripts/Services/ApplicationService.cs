using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Installers;
using Services.Base;

namespace Services
{
    public class ApplicationService
    {
        private readonly List<IDisposableService> _disposableServices = new List<IDisposableService>();
        
        public bool IsInitialized { get; private set; }

        public void SetApplicationInitialized()
        {
            IsInitialized = true;
        }
        
        public void RegisterDisposableService<TService>(bool skipInitialize = false) where TService : IDisposableService
        {
            var service = ContainerHolder.CurrentContainer.Resolve<TService>();
            if (!skipInitialize)
            {
                service.InitializeService();
            }
            
            if (_disposableServices.Any(x => x == service as IDisposableService))
            {
                LoggerService.LogWarning($"An attempt to register service {service.GetType().Name} twice ");
                return;
            }

            _disposableServices.Add(service);
        }
        
        public void RegisterDisposableService<TService>(TService service, bool skipInitialize = false) where TService : IDisposableService
        {
            if (!skipInitialize)
            {
                service.InitializeService();
            }

            if (_disposableServices.Any(x => x == service as IDisposableService))
            {
                LoggerService.LogWarning($"An attempt to register service {service.GetType().Name} twice ");
                return;
            }
            
            _disposableServices.Add(service);
        }

        public void DisposeServices()
        {
            if (!_disposableServices.IsNullOrEmpty())
            {
                _disposableServices.Foreach(s =>
                {
                    try
                    {
                        s.DisposeService();
                    }
                    catch (Exception e)
                    {
                        LoggerService.LogWarning(this, $"Error disposing: {e}");
                    }
                });
                _disposableServices.Clear();   
            }

            IsInitialized = false;
        }
    }
}