using Schiefelbein.SystemManager.Configuration;
using Schiefelbein.SystemManager.Controllers;
using Schiefelbein.SystemManager.Errors;
using Schiefelbein.SystemManager.Models;
using System.Text;
using System.Web;

namespace Schiefelbein.SystemManager.Managers
{
    public class ServiceManager : IServiceManager
    {
        private readonly ILogger<ServiceManager> _logger;
        private readonly IConfigManager _configManager;
        private readonly IHttpClientFactory _httpClientFactory;

        public ServiceManager(ILogger<ServiceManager> logger, IConfigManager configManager, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configManager = configManager;
            _httpClientFactory = httpClientFactory;
        }

        private ServerInfoConfig GetConfig(string serverName)
        {
            var configItem = _configManager.Servers.Servers.FirstOrDefault(x => string.Equals(x.Name, serverName, StringComparison.CurrentCultureIgnoreCase));
            if (configItem == null)
            {
                throw new ServiceManagerException("unknown server " + serverName, System.Net.HttpStatusCode.NotFound);
            }

            return configItem;
        }
        
        public async Task<ServicesViewModel> GetServicesAsync(string serverName, CancellationToken cancellationToken = default)
        {
            var config = GetConfig(serverName);

            using var client = _httpClientFactory.CreateClient();
            var url = config.ServicesUrl;
            using var request = new HttpRequestMessage(HttpMethod.Get, config.ServicesUrl);
            request.SetAuthHeader(config);
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Error Retrieving Services - GET {url} - {statusCodeNum} {StatusCode} : {ErrorMessage}", url, (int)response.StatusCode, response.StatusCode, errorMsg);
                string msg = string.Format("Error Retrieving Services on {0} - {1} {2} : {3}", serverName, (int)response.StatusCode, response.StatusCode, errorMsg);
                throw new ServiceManagerException(msg, response.StatusCode);
            }

            var model = await response.Content.ReadFromJsonAsync<ServicesViewModel>() ??
                    throw new ServiceManagerException("Unable to get services", System.Net.HttpStatusCode.InternalServerError);

            _logger.LogInformation("GetServices - \\{serverName} : {result} items", serverName, model.Services.Count);
            return model;
        }

        public async Task<ServiceOperationResult> StartAsync(string serverName, string serviceName, CancellationToken cancellationToken = default)
        {
            var config = GetConfig(serverName);
            using var client = _httpClientFactory.CreateClient();
            var url = config.ServicesUrl + "/start";
            var reqContent = "serviceName=" + HttpUtility.UrlEncode(serviceName);
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(reqContent,
                                            Encoding.UTF8,
                                            "application/x-www-form-urlencoded")
            };
            request.SetAuthHeader(config);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Error Starting Service - POST {url} {request} - {statusCodeNum} {statusCode} : {errorMsg}", url, reqContent, (int)response.StatusCode, response.StatusCode, errorMsg);
                string msg = string.Format("Error Starting Service {0} on {1} - {2} {3} : {4}", serviceName, serverName, (int)response.StatusCode, response.StatusCode, errorMsg);
                throw new ServiceManagerException(msg, response.StatusCode);
            }

            var model = await response.Content.ReadFromJsonAsync<ServiceOperationResult>() ??
                    throw new ServiceManagerException("Unable to start service", System.Net.HttpStatusCode.InternalServerError);

            _logger.LogInformation("Start - \\{serverName} service:{serviceName}: {result}", serverName, serviceName, model);
            return model;
        }

        public async Task<ServiceOperationResult> StopAsync(string serverName, string serviceName, CancellationToken cancellationToken = default)
        {
            var config = GetConfig(serverName);
            using var client = _httpClientFactory.CreateClient();
            var url = config.ServicesUrl + "/stop";
            var reqContent = "serviceName=" + HttpUtility.UrlEncode(serviceName);
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(reqContent,
                                            Encoding.UTF8,
                                            "application/x-www-form-urlencoded")
            };
            request.SetAuthHeader(config);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Error Stopping Service - POST {url} {request} - {statusCodeNum} {statusCode} : {errorMsg}", url, reqContent, (int)response.StatusCode, response.StatusCode, errorMsg);
                string msg = string.Format("Error Stopping Service {0} on {1} - {2} {3} : {4}", serviceName, serverName, (int)response.StatusCode, response.StatusCode, errorMsg);
                throw new ServiceManagerException(msg, response.StatusCode);
            }

            var model = await response.Content.ReadFromJsonAsync<ServiceOperationResult>() ??
                    throw new ServiceManagerException("Unable to stop service", System.Net.HttpStatusCode.InternalServerError);

            _logger.LogInformation("Stop - \\{serverName} service:{serviceName}: {result}", serverName, serviceName, model);
            return model;
        }

        public async Task<ServiceOperationResult> SetFavouriteAsync(string serverName, string serviceName, bool isFavourite, CancellationToken cancellationToken = default)
        {
            var config = GetConfig(serverName);
            using var client = _httpClientFactory.CreateClient();
            var url = config.ServicesUrl + "/favourite";
            var reqContent = "serviceName=" + HttpUtility.UrlEncode(serviceName) + "&isFavourite=" + isFavourite;
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(reqContent,
                                            Encoding.UTF8,
                                            "application/x-www-form-urlencoded")
            };
            request.SetAuthHeader(config);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Error SetFavourite Service - POST {url} {request} - {statusCodeNum} {statusCode} : {errorMsg}", url, reqContent, (int)response.StatusCode, response.StatusCode, errorMsg);
                string msg = string.Format("Error Setting Favourite Service {0} on {1} - {2} {3} : {4}", serviceName, serverName, (int)response.StatusCode, response.StatusCode, errorMsg);
                throw new ServiceManagerException(msg, response.StatusCode);
            }

            var model = await response.Content.ReadFromJsonAsync<ServiceOperationResult>() ??
                    throw new ServiceManagerException("Unable to set service favourite", System.Net.HttpStatusCode.InternalServerError);

            _logger.LogInformation("SetFavourite - \\{serverName} service:{serviceName}: {result}", serverName, serviceName, model);
            return model;
        }

        public async Task<WaitForServiceStatusResult> WaitForStatusAsync(string serverName, string serviceName, ServiceStatus[] serviceStatuses, int timeout, CancellationToken cancellationToken = default)
        {
            var config = GetConfig(serverName);
            using var client = _httpClientFactory.CreateClient();
            var url = config.ServicesUrl + "/wait-for-status";
            var requestContent = new StringBuilder("serviceName=" + HttpUtility.UrlEncode(serviceName));
            foreach (var serviceStatus in serviceStatuses)
            {
                requestContent.Append("&serviceStatuses[]=").Append(HttpUtility.UrlEncode(serviceStatus.ToString()));
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(requestContent.ToString(),
                                            Encoding.UTF8,
                                            "application/x-www-form-urlencoded")
            };
            request.SetAuthHeader(config);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Error WaitForStatus Service - POST {url} {request} - {statusCodeNum} {statusCode} : {errorMsg}", url, requestContent.ToString(), (int)response.StatusCode, response.StatusCode, errorMsg);
                string msg = string.Format("Error Waiting For Service Status {0} on {1} - {2} {3} : {4}", serviceName, serverName, (int)response.StatusCode, response.StatusCode, errorMsg);
                throw new ServiceManagerException(msg, response.StatusCode);
            }

            var model = await response.Content.ReadFromJsonAsync<WaitForServiceStatusResult>() ??
                    throw new ServiceManagerException("Unable to wait-for-status", System.Net.HttpStatusCode.InternalServerError);

            _logger.LogInformation("WaitForStatus - \\{serverName} service:{serviceName}: {result}", serverName, serviceName, model);
            return model;
        }
    }
}
