using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.CodeAnalysis;
using Microsoft.IdentityModel.Protocols;
using Schiefelbein.SystemManager.Configuration;
using Schiefelbein.SystemManager.Data;
using Schiefelbein.SystemManager.Errors;
using Schiefelbein.SystemManager.Models;
using Schiefelbein.Common.Web.Oidc;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Schiefelbein.SystemManager.Controllers
{
    [AllowAnonymous]
    public partial class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOidcClient _oidcClient;
        private readonly IConfigManager _configManager;

        public HomeController(ILogger<HomeController> logger, IConfigManager configManager, IOidcClient oidcClient)
        {
            _logger = logger;
            _configManager = configManager;
            _oidcClient = oidcClient;

        }

        [HttpGet("/")]
        public IActionResult Index(string? error, string? page, string? info)
        {
            HttpContext.Session.SetString("FileManager-SID", HttpContext.Session.Id);
            _logger.LogInformation("/ {sid}", HttpContext.Session.Id);

            var sisterSites = (from x in
                              _configManager.WebServer.SisterSites
                               select new SisterSiteViewModel(x.Name, x.Url)).ToArray();

            var model = new UserActionViewModel(error, page, info, _configManager.WebServer.LoginMethod, 
                _configManager.WebServer.OidcLoginText, _configManager.WebServer.AdLoginText, sisterSites);
            return View(model);
        }

        public IActionResult Login(string? page)
        {
            string state = HttpContext.Session.Id;
            var url = _oidcClient.GetAuthenticationUrl(state);
            _logger.LogInformation("/Login {sid} redirecting to {url}", HttpContext.Session.Id, url);
            return new RedirectResult(url);
        }

        public IActionResult Error()
        {
            _logger.LogWarning("/Error {sid}", HttpContext.Session.Id);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}