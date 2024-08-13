using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schiefelbein.SystemManager.Configuration;
using Schiefelbein.SystemManager.Data;
using Schiefelbein.SystemManager.Errors;
using Schiefelbein.SystemManager.Models;

namespace Schiefelbein.SystemManager.Controllers
{
    [Authorize(Roles = SystemManagerRoles.ServerMonitor)]
    public class ServerInfoController(
        ILogger<ServerInfoController> logger,
        WebServerConfig webServerConfig,
        IConfigManager configManager) : Controller
    {
        private readonly ILogger<ServerInfoController> _logger = logger;
        private readonly WebServerConfig _webServerConfig = webServerConfig;
        private readonly IConfigManager _configManager = configManager;

        [HttpGet("[controller]/")]
        [AllowAnonymous]
        public IActionResult Index(string? serverName)
        {
            try
            {
                User.AssertServerMonitor();
            }
            catch (ServiceManagerUnauthorisedException ex)
            {
                return RedirectToAction("Index", "Home", new { error = ex.Message, page = "ServerInfo" });
            }

            var serverList = (from x in _configManager.Servers.Servers select x.Name).ToArray();
            serverName = this.LoadServerName(serverName, serverList);
            this.SaveServerName(serverName);

            var vm = new ServerInfoViewModel(serverList, serverName, 16, [ "C:\\" ]);
            _logger.LogDebug("Index -- {sid} viewModel: {vm}", HttpContext.Session.Id, vm);

            var configItem = _configManager.Servers.Servers.FirstOrDefault(x => string.Equals(x.Name, serverName, StringComparison.CurrentCultureIgnoreCase)) ??
                throw new Exception("unknown server " + serverName);
            vm.CpuCores = configItem.CpuCores;
            vm.Disks = configItem.Disks;

            return View(vm);
        }
    }

}