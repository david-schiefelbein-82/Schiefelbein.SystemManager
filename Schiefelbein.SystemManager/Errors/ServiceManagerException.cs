using Microsoft.AspNetCore.Http;
using System.Net;

namespace Schiefelbein.SystemManager.Errors
{
    public class ServiceManagerException : Exception
    {
        public ServiceManagerException(string? message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public ServiceManagerException(string? message, HttpStatusCode statusCode, Exception? innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
