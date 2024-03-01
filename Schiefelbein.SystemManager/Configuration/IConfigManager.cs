using Schiefelbein.Common.Web.Configuration;

namespace Schiefelbein.SystemManager.Configuration
{
    public interface IConfigManager
    {
        ServersConfig Servers { get; }

        WebServerConfig WebServer { get; }

        IAuthorizationConfig Authorization { get; }
    }
}