using System.Net;

namespace Schiefelbein.SystemManager.Errors
{
    public class ServiceManagerConfigurationException : ServiceManagerException
    {
        public ServiceManagerConfigurationException(string? message) : base(message, System.Net.HttpStatusCode.InternalServerError)
        {
        }

        public ServiceManagerConfigurationException(string? message, Exception? innerException) : base(message, HttpStatusCode.InternalServerError, innerException)
        {
        }
    }
}
