using Schiefelbein.Utilities.SystemManager.Models;

namespace Schiefelbein.SystemManager.Managers
{
    public class ServiceManager(ILogger<ServiceManager> logger, ISystemManagerSelector systemManagerSelector) : IServiceManager
    {
        private readonly ILogger<ServiceManager> _logger = logger;
        private readonly ISystemManagerSelector _systemManagerSelector = systemManagerSelector;
        
        public async Task<ServicesViewModel> GetServicesAsync(string serverName, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("GetServices {serverName}", serverName);
            var client = _systemManagerSelector.GetSystemManagerClient(serverName);
            return await client.GetServicesAsync(cancellationToken);
        }

        public async Task<ServiceOperationResult> StartAsync(string serverName, string serviceName, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("StartService {serverName}, serviceName: {serviceName}", serverName, serviceName);
            var client = _systemManagerSelector.GetSystemManagerClient(serverName);
            return await client.StartServiceAsync(serviceName, cancellationToken);
        }

        public async Task<ServiceOperationResult> StopAsync(string serverName, string serviceName, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("StopService {serverName}, serviceName: {serviceName}", serverName, serviceName);
            var client = _systemManagerSelector.GetSystemManagerClient(serverName);
            return await client.StopServiceAsync(serviceName, cancellationToken);
        }

        public async Task<ServiceOperationResult> SetFavouriteAsync(string serverName, string serviceName, bool isFavourite, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("SetFavourite {serverName}, serviceName: {serviceName}, isFavourite: {isFavourite}", serverName, serviceName, isFavourite);
            var client = _systemManagerSelector.GetSystemManagerClient(serverName);
            return await client.SetFavouriteServiceAsync(serviceName, isFavourite, cancellationToken);
        }

        public async Task<WaitForServiceStatusResult> WaitForStatusAsync(string serverName, string serviceName, ServiceStatus[] serviceStatuses, int timeout, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("WaitForStatus {serverName}, serviceName: {serviceName}, serviceStatuses: {serviceStatuses}, timeout: {timeout}", serverName, serviceName, string.Join(',', serviceStatuses), timeout);
            var client = _systemManagerSelector.GetSystemManagerClient(serverName);
            return await client.WaitForServiceStatusAsync(serviceName, serviceStatuses, timeout, cancellationToken);
        }
    }
}
