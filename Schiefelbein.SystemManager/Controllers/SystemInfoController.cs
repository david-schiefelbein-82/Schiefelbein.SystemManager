using Microsoft.AspNetCore.Mvc;
using Schiefelbein.SystemManager.Configuration;
using Schiefelbein.SystemManager.Errors;
using Schiefelbein.SystemManager.Managers;
using Schiefelbein.Utilities.SystemManager.Client;
using Schiefelbein.Utilities.SystemManager.Models;
using System.Configuration;

namespace Schiefelbein.SystemManager.Controllers
{
    [Route("system-info")]
    public class SystemInfoController(ILogger<SystemInfoController> logger, ISystemManagerSelector systemManagerSelector, IConfigManager configManager) : Controller
    {
        private readonly ILogger<SystemInfoController> _logger = logger;
        private readonly ISystemManagerSelector _systemManagerSelector = systemManagerSelector;
        private readonly IConfigManager _configManager = configManager;

        [ServiceManagerExceptionFilter]
        public async Task<SystemInfoModel> IndexAsync(string serverName, CancellationToken cancellationToken)
        {
            var client = _systemManagerSelector.GetSystemManagerClient(serverName);
            try
            {
                var result = await client.GetSystemInfoAsync(cancellationToken);

                UpdateConfig(serverName, result);

                return result;
            }
            catch (ServiceManagerException ex)
            {
                _logger.LogError(ex, "Index");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Index");
                throw;
            }
        }

        private void UpdateConfig(string serverName, SystemInfoModel result)
        {
            var config = _configManager.Servers.Servers.FirstOrDefault(x => string.Equals(x.Name, serverName, StringComparison.InvariantCultureIgnoreCase));
            bool modified = false;
            if (config != null)
            {
                var resultCpuCores = result.Cpu.Cores.Count;
                if (result.Cpu.Cores.Count > 0 && config.CpuCores != resultCpuCores)
                {
                    var configuredCpuCores = config.CpuCores;
                    config.CpuCores = result.Cpu.Cores.Count;
                    modified = true;
                    _logger.LogInformation("Changed Server {serverName} from {old} -> {new} cpuCores", serverName, configuredCpuCores, resultCpuCores);
                    }

                var names = string.Join(',', (from x in result.Disk select x.Name));
                var disks = string.Join(',', config.Disks);
                if (!string.Equals(names, disks))
                {
                    config.Disks = [.. (from x in result.Disk select x.Name)];
                    _logger.LogInformation("Changed Disks {serverName} from {old} -> {new} disks", serverName, disks, names);
                        modified = true;
                }

                if (modified)
                {
                    _logger.LogInformation("Saving Servers config");
                    _configManager.Servers.Save();
                }
            }
        }
    }
}
