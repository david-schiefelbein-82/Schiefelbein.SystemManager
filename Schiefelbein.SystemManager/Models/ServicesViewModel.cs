using Microsoft.AspNetCore.Hosting.Server;

namespace Schiefelbein.SystemManager.Models
{
    public class ServicesViewModel
    {
        public string[] Servers { get; set; }

        public string ServerName { get; set; }

        public List<ServiceViewModel> Services { get; set; }

        public bool IsSuccess { get; set; }

        public string Error { get; set; }

        public ServicesViewModel()
        {
            Servers = Array.Empty<string>();
            ServerName = string.Empty;
            Services = new List<ServiceViewModel>();
            Error = string.Empty;
            IsSuccess = false;
        }
    }
}
