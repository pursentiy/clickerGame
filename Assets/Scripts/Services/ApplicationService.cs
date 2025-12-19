using System.Collections.Generic;
using System.Linq;
using Services;
using Services.Base;

namespace Handlers
{
    public class ApplicationService
    {
        private readonly List<IDisposableService> _disposableServices = new List<IDisposableService>();
        
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
    }
}