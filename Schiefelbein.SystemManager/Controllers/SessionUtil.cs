using Microsoft.AspNetCore.Mvc;
using Schiefelbein.SystemManager.Configuration;
using System.Net.Http.Headers;

namespace Schiefelbein.SystemManager.Controllers
{
    public static class SessionUtil
    {
        private const string KEY_SERVER_NAME = "serverName";

        public static void SetAuthHeader(this HttpRequestMessage request, ServerInfoConfig config)
        {
            var auth = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(config.User + ":" + config.Password));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
        }

        public static string LoadServerName(this Controller controller, string? serverName, string[] serverList)
        {
            if (string.IsNullOrWhiteSpace(serverName))
            {
                var savedServerName = controller.HttpContext.Session.GetString(KEY_SERVER_NAME);

                if (!string.IsNullOrWhiteSpace(savedServerName))
                {
                    serverName = savedServerName;
                }
                else
                {
                    serverName = serverList[0];
                }
            }

            return serverName;
        }

        public static void SaveServerName(this Controller controller, string serverName)
        {
            controller.HttpContext.Session.SetString(KEY_SERVER_NAME, serverName);
        }
    }
}
