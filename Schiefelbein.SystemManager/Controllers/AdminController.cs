using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Schiefelbein.Common.Web.Configuration;
using Schiefelbein.SystemManager.Configuration;
using Schiefelbein.SystemManager.Data;
using Schiefelbein.SystemManager.Errors;
using Schiefelbein.SystemManager.Models;
using System.ServiceProcess;

namespace Schiefelbein.SystemManager.Controllers
{
    [Authorize(Roles = SystemManagerRoles.Admin)]
    public class AdminController(ILogger<AdminController> logger, IConfigManager configManager) : Controller
    {
        private readonly ILogger<AdminController> _logger = logger;
        private readonly IConfigManager _configManager = configManager;

        [AllowAnonymous]
        public ActionResult Index()
        {
            try
            {
                User.AssertAdmin();
            }
            catch (ServiceManagerUnauthorisedException ex)
            {
                return RedirectToAction("Index", "Home", new { error = ex.Message });
            }

            var viewModel = from x
                            in _configManager.Authorization.UsersAndGroups
                            select new AuthEntityViewModelWithError(x);
            return View(viewModel);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string id, string label, AuthEntityType type,
            bool serverMonitor, bool serviceController, bool admin)
        {
            try
            {
                if (_configManager.Authorization.UsersAndGroups.FirstOrDefault(x => x.Id == id && x.Type == type) != null)
                {
                    throw new Exception("user already exists");
                }

                var newUser = new AuthEntityConfig()
                {
                    Id = id,
                    Label = label,
                    Type = type,
                    Roles = BuildRoles(serverMonitor, serviceController, admin)
                };

                _configManager.Authorization.UsersAndGroups.Add(newUser);
                _configManager.Authorization.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View(
                    new AuthEntityViewModelWithError()
                    {
                        ErrorMessage = ex.Message,
                        Id = id,
                        Label = label,
                        Type = type,
                        ServerMonitor = serverMonitor,
                        ServiceController = serviceController,
                        Admin = admin
                    });
            }
        }

        // GET
        public ActionResult Edit(string id, AuthEntityType type)
        {
            var viewModel = new AuthEntityViewModelWithError();
            var item = _configManager.Authorization.UsersAndGroups.FirstOrDefault(x => x.Id == id && x.Type == type);
            if (item == null)
            {
                viewModel.ErrorMessage = "Cannot find " + type + " with Id: " + id;
            }
            else
            {
                viewModel.CopyFrom(item);
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string origId, AuthEntityType origType, string id, string label, AuthEntityType type,
            bool serverMonitor, bool serviceController, bool admin)
        {
            try
            {
                var item = _configManager.Authorization.UsersAndGroups.FirstOrDefault(x => x.Id == origId && x.Type == origType) ?? throw new Exception("cannot edit item");
                
                if (!string.IsNullOrEmpty(id) && string.Equals(id, this.User.Identity?.Name, StringComparison.CurrentCultureIgnoreCase) && !admin)
                {
                    var model = new AuthEntityViewModelWithError() { ErrorMessage = "Cannot remove admin permissions from yourself" };
                    model.CopyFrom(item);
                    model.Id = origId;
                    model.Type = origType;
                    model.Label = label;
                    model.ServerMonitor = serverMonitor;
                    model.ServiceController = serviceController;
                    return View(model);
                }

                item.Id = id;
                item.Label = label;
                item.Type = type;
                item.Roles = BuildRoles(serverMonitor, serviceController, admin);
                _configManager.Authorization.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View(new AuthEntityViewModelWithError()
                {
                    ErrorMessage = ex.Message,
                });
            }
        }

        private static string[] BuildRoles(bool serverMonitor, bool serviceController, bool admin)
        {
            var roles = new List<string>();
            if (serverMonitor)
                roles.Add(SystemManagerRoles.ServerMonitor);

            if (serviceController)
                roles.Add(SystemManagerRoles.ServiceController);

            if (admin)
                roles.Add(SystemManagerRoles.Admin);

            return [.. roles];
        }

        public ActionResult Delete(string id, AuthEntityType type)
        {
            var viewModel = new AuthEntityViewModelWithError();
            var item = _configManager.Authorization.UsersAndGroups.FirstOrDefault(x => x.Id == id && x.Type == type);
            if (item == null)
            {
                viewModel.ErrorMessage = "Cannot find " + type + " with Id: " + id;
            }
            else
            {
                viewModel.CopyFrom(item);
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id, string label, AuthEntityType type, IFormCollection _)
        {
            try
            {
                if (!string.IsNullOrEmpty(id) && string.Equals(id, this.User.Identity?.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return View(new AuthEntityViewModelWithError() { ErrorMessage = "Cannot delete yourself", Id = id, Type = type, Label = label });
                }

                var removed = _configManager.Authorization.UsersAndGroups.RemoveAll(x => x.Id == id && x.Type == type);
                if (removed > 0)
                {
                    _configManager.Authorization.Save();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View(new AuthEntityViewModelWithError() { ErrorMessage = ex.Message });
            }
        }
    }
}
