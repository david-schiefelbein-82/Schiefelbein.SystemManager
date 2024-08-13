using Microsoft.AspNetCore.Mvc;
using Schiefelbein.SystemManager.Models;
using Schiefelbein.Utilities.SystemManager.Models;

namespace Schiefelbein.SystemManager.Managers
{
    public interface IServiceManager
    {
        public Task<ServicesViewModel> GetServicesAsync(string serverName, CancellationToken cancellationToken = default);

        public Task<ServiceOperationResult> StartAsync(string serverName, string serviceName, CancellationToken cancellationToken = default);

        public Task<ServiceOperationResult> StopAsync(string serverName, string serviceName, CancellationToken cancellationToken = default);

        public Task<ServiceOperationResult> SetFavouriteAsync(string serverName, string serviceName, bool isFavourite, CancellationToken cancellationToken = default);

        public Task<WaitForServiceStatusResult> WaitForStatusAsync(string serverName, string serviceName, ServiceStatus[] serviceStatusus, int timeout, CancellationToken cancellationToken = default);
    }
}
