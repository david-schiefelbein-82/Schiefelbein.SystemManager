using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.VisualStudio;
using Schiefelbein.SystemManager.Configuration;
using Schiefelbein.SystemManager.Data;
using Schiefelbein.SystemManager.Errors;
using Schiefelbein.SystemManager.Managers;
using Schiefelbein.SystemManager.Models;
using Schiefelbein.Utilities.SystemManager.Models;

namespace Schiefelbein.SystemManager.Controllers
{
    [Authorize(Roles = SystemManagerRoles.Admin)]
    public class ServicesController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IServiceManager _serviceManager;
        private readonly IConfigManager _configManager;

        public ServicesController(ILogger<HomeController> logger, IConfigManager configManager, IServiceManager serviceManager)
        {
            _logger = logger;
            _serviceManager = serviceManager;
            _configManager = configManager;
            _logger.LogDebug("Created {name}", nameof(ServicesController));
        }

        [AllowAnonymous]
        public async Task<IActionResult> IndexAsync(string? serverName, CancellationToken cancellationToken = default)
        {
            ServicesViewModel vm;

            var serverList = (from x in _configManager.Servers.Servers select x.Name).ToArray();
            serverName = this.LoadServerName(serverName, serverList);
            this.SaveServerName(serverName);

            try
            {
                User.AssertServiceController();
            }
            catch (ServiceManagerUnauthorisedException ex)
            {
                return RedirectToAction("Index", "Home", new { error = ex.Message, page = "services" });
            }

            try
            {
                vm = await _serviceManager.GetServicesAsync(serverName, cancellationToken);
            }
            catch (Exception ex)
            {
                vm = new ServicesViewModel()
                {
                    Error = ex.Message,
                    IsSuccess = false,
                };
            }

            vm.ServerName = serverName;
            vm.Servers = serverList;
            return View(vm);
        }

        [HttpGet("[controller]/refresh")]
        public async Task<ServicesViewModel> RefreshAsync(string serverName, CancellationToken cancellationToken = default)
        {
            var serverList = (from x in _configManager.Servers.Servers select x.Name).ToArray();
            serverName = this.LoadServerName(serverName, serverList);

            _logger.LogInformation("Refresh {serverName}", serverName);
            var vm = await _serviceManager.GetServicesAsync(serverName, cancellationToken);
            return vm;
        }

        [HttpPost("[controller]/start")]
        [ServiceManagerExceptionFilter]
        public async Task<ServiceOperationResult> StartAsync(string? serverName, string serviceName, CancellationToken cancellationToken = default)
        {
            var serverList = (from x in _configManager.Servers.Servers select x.Name).ToArray();
            serverName = this.LoadServerName(serverName, serverList);

            _logger.LogInformation("StartService(serverName: {serverName}, serviceName: {serviceName})", serverName, serviceName);
            return await _serviceManager.StartAsync(serverName, serviceName, cancellationToken);
        }

        [HttpPost("[controller]/stop")]
        [ServiceManagerExceptionFilter]
        public async Task<ServiceOperationResult> StopAsync(string? serverName, string serviceName, CancellationToken cancellationToken = default)
        {
            var serverList = (from x in _configManager.Servers.Servers select x.Name).ToArray();
            serverName = this.LoadServerName(serverName, serverList);

            _logger.LogInformation("StopService(serverName: {serverName}, serviceName: {serviceName})", serverName, serviceName);
            return await _serviceManager.StopAsync(serverName, serviceName, cancellationToken);
        }

        [HttpPost("[controller]/favourite")]
        [ServiceManagerExceptionFilter]
        public async Task<ServiceOperationResult> FavouriteAsync(string? serverName, string serviceName, bool isFavourite, CancellationToken cancellationToken = default)
        {
            var serverList = (from x in _configManager.Servers.Servers select x.Name).ToArray();
            serverName = this.LoadServerName(serverName, serverList);

            _logger.LogInformation("Favourite(serverName: {serverName}, serviceName: {serviceName}, isFavourite: {isFavourite})", serverName, serviceName, isFavourite);
            return await _serviceManager.SetFavouriteAsync(serverName, serviceName, isFavourite, cancellationToken);
        }

        [HttpPost("[controller]/wait-for-status")]
        [ServiceManagerExceptionFilter]
        public async Task<WaitForServiceStatusResult> WaitForStatusAsync(string serverName, string serviceName, ServiceStatus[] serviceStatusus, int timeout, CancellationToken cancellationToken = default)
        {
            var serverList = (from x in _configManager.Servers.Servers select x.Name).ToArray();
            serverName = this.LoadServerName(serverName, serverList);

            _logger.LogInformation("WaitForStatus(serverName: {serverName}, serviceName: {serviceName}, serviceStatusus: {serviceStatusus}, timeout: {timeout})", serverName, serviceName, string.Join(", ", serviceStatusus), timeout);
            return await _serviceManager.WaitForStatusAsync(serverName, serviceName, serviceStatusus, timeout, cancellationToken);
        }
    }
}
