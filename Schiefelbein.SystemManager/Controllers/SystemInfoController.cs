using Microsoft.AspNetCore.Mvc;
using Schiefelbein.SystemManager.Configuration;
using Schiefelbein.SystemManager.Errors;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Schiefelbein.SystemManager.Controllers
{
    [Route("system-info")]
    public class SystemInfoController : Controller
    {
        private readonly ILogger<SystemInfoController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigManager _configManager;

        public SystemInfoController(ILogger<SystemInfoController> logger, IHttpClientFactory httpClientFactory, IConfigManager configManager)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configManager = configManager;
        }

        [ServiceManagerExceptionFilter]
        public async Task<JsonDocument> IndexAsync(string serverName, CancellationToken cancellationToken)
        {
            var config = _configManager.Servers.Servers.FirstOrDefault(x => string.Equals(serverName, x.Name, StringComparison.CurrentCultureIgnoreCase))
                ?? throw new ServiceManagerException("Cannot find server " + serverName, System.Net.HttpStatusCode.NotFound);

            var uri = config.SystemInfoUrl;
            try
            {
                using var client = _httpClientFactory.CreateClient();
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.SetAuthHeader(config);
                using var result = await client.SendAsync(request, cancellationToken);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    var errorMsg = await result.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Error getting SystemInfo - {url} - {statusCodeNum} {statusCode} : {errorMsg}", uri, (int)result.StatusCode, result.StatusCode, errorMsg);
                    string msg = string.Format("Error getting SystemInfo for server {0} - {1} {2} : {3}", serverName, (int)result.StatusCode, result.StatusCode, errorMsg);
                    throw new ServiceManagerException(msg, result.StatusCode);
                }

                var json = await result.Content.ReadAsStringAsync(cancellationToken);
                var doc = JsonSerializer.Deserialize<JsonDocument>(json);
                if (doc == null)
                {
                    throw new ServiceManagerException("Unable to get ServerInfo (invalid JSON)", System.Net.HttpStatusCode.InternalServerError);
                }

                return doc;
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
    }
}
