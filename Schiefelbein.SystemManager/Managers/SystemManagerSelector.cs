using Schiefelbein.SystemManager.Configuration;
using Schiefelbein.SystemManager.Controllers;
using Schiefelbein.SystemManager.Errors;
using Schiefelbein.Utilities.SystemManager.Client;

namespace Schiefelbein.SystemManager.Managers
{
    public class SystemManagerSelector(
        ILogger<SystemManagerClient> logger,
        IHttpClientFactory httpClientFactory,
        IConfigManager configManager) : ISystemManagerSelector
    {
        private readonly ILogger<SystemManagerClient> _logger = logger;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IConfigManager _configManager = configManager;
        private readonly Dictionary<string, SystemManagerClient> _clients = [];

        public SystemManagerClient GetSystemManagerClient(string serverName)
        {
            lock (this)
            {
                if (_clients.TryGetValue(serverName, out var client))
                    return client;

                var config = _configManager.Servers.Servers.FirstOrDefault(x => string.Equals(serverName, x.Name, StringComparison.CurrentCultureIgnoreCase))
                    ?? throw new ServiceManagerException("Cannot find server " + serverName, System.Net.HttpStatusCode.NotFound);

                _logger.LogDebug("Creating SystemManagerClient for \\{serverName}", serverName);
                client = new SystemManagerClient(_logger, _httpClientFactory, config);
                _clients[serverName] = client;
                return client;
            }
        }
    }
}
