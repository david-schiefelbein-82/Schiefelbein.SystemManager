using System.Net;

namespace Schiefelbein.SystemManager.Errors
{
    public class ServiceManagerUnauthorisedException : ServiceManagerException
    {
        public ServiceManagerUnauthorisedException(string? message) : base(message, HttpStatusCode.InternalServerError)
        {
        }

        public ServiceManagerUnauthorisedException(string? message, Exception? innerException) : base(message, HttpStatusCode.Unauthorized, innerException)
        {
        }
    }
}
