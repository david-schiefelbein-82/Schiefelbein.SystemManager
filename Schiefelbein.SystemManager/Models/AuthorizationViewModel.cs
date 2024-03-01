using Schiefelbein.Common.Web.Configuration;
using Schiefelbein.SystemManager.Configuration;
using Schiefelbein.SystemManager.Data;

namespace Schiefelbein.SystemManager.Models
{    public class AuthEntityViewModelWithError : AuthEntityViewModel
    {
        public string? ErrorMessage { get; set; }

        public AuthEntityViewModelWithError()
        {
        }

        public AuthEntityViewModelWithError(AuthEntityConfig config)
        {
            CopyFrom(config);
        }
    }

    public class AuthEntityViewModel
    {
        public AuthEntityViewModel()
        {
            Id = string.Empty;
            Label = string.Empty;
        }

        public string Id { get; set; }

        public string Label { get; set; }

        public AuthEntityType Type { get; set; }

        public bool ServerMonitor { get; set; }

        public bool ServiceController { get; set; }

        public bool Admin { get; set; }

        public void CopyFrom(AuthEntityConfig config)
        {
            Id = config.Id;
            Label = config.Label;
            Type = config.Type;
            ServerMonitor = config.Roles.Any(r => string.Equals(r, SystemManagerRoles.ServerMonitor, StringComparison.CurrentCultureIgnoreCase));
            ServiceController = config.Roles.Any(r => string.Equals(r, SystemManagerRoles.ServiceController, StringComparison.CurrentCultureIgnoreCase));
            Admin = config.Roles.Any(r => string.Equals(r, SystemManagerRoles.Admin, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
