using Schiefelbein.SystemManager.Configuration;
using Schiefelbein.SystemManager.Controllers;
using Schiefelbein.SystemManager.Errors;
using Schiefelbein.Utilities.SystemManager.Client;

namespace Schiefelbein.SystemManager.Managers
{
    public interface ISystemManagerSelector
    {
        SystemManagerClient GetSystemManagerClient(string serverName);
    }
}
