// #define TESTING
using Schiefelbein.SystemManager.Errors;
using System.Security.Claims;

namespace Schiefelbein.SystemManager.Data
{
    public static class SystemManagerRoles
    {
        public const string ServerMonitor = "server-monitor";
        public const string ServiceController = "service-controller";
        public const string Admin = "admin";

        /// <summary>
        /// asserts that the current user has permission to monitor servers
        /// </summary>
        /// <exception cref="ServiceManagerUnauthorisedException">if the user does not have permission</exception>
        public static void AssertServerMonitor(this ClaimsPrincipal user)
        {
#if TESTING
            return;
#else
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                throw new ServiceManagerUnauthorisedException("user is not logged in");
            }

            if (user.IsInRole(ServerMonitor))
                return;

            throw new ServiceManagerUnauthorisedException("user does not have permission to access server-info");
#endif
        }

        /// <summary>
        /// asserts that the current user has permission start and stop services
        /// </summary>
        /// <exception cref="ServiceManagerUnauthorisedException">if the user does not have permission</exception>
        public static void AssertServiceController(this ClaimsPrincipal user)
        {
#if TESTING
            return;
#else
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                throw new ServiceManagerUnauthorisedException("user is not logged in");
            }

            if (user.IsInRole(ServiceController))
                return;

            throw new ServiceManagerUnauthorisedException("user does not have permission to act as a service controller");
#endif
        }

        /// <summary>
        /// asserts that the current user has admin permissions
        /// </summary>
        /// <exception cref="ServiceManagerUnauthorisedException">if the user does not have permission</exception>
        public static void AssertAdmin(this ClaimsPrincipal user)
        {
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                throw new ServiceManagerUnauthorisedException("user is not logged in");
            }

            if (user.IsInRole(SystemManagerRoles.Admin))
                return;

            throw new ServiceManagerUnauthorisedException("user does not have admin permissions");
        }

    }
}
