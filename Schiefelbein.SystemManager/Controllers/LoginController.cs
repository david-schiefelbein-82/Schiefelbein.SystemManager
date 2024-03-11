using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schiefelbein.Common.Web;
using Schiefelbein.Common.Web.ActiveDirectory;
using Schiefelbein.Common.Web.Oidc;
using Schiefelbein.SystemManager.Configuration;
using Schiefelbein.SystemManager.Data;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Schiefelbein.SystemManager.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IOidcClient _oidcClient;
        private readonly IConfigManager _configManager;
        private readonly IJwtSecurityTokenProvider _jwtSecurityTokenProvider;
        private readonly IActiveDirectoryUserAuthenticator _activeDirectoryUserAuthenticator;

        public static string PageToController(string? page)
        {
            if (string.Equals(page, "Services", StringComparison.CurrentCultureIgnoreCase))
                return "Services";

            if (string.Equals(page, "Admin", StringComparison.CurrentCultureIgnoreCase))
                return "Admin";

            return "ServerInfo";
        }

        public LoginController(ILogger<LoginController> logger, IOidcClient oidcClient, IConfigManager configManager, IJwtSecurityTokenProvider jwtSecurityTokenProvider, IActiveDirectoryUserAuthenticator activeDirectoryUserAuthenticator)
        {
            _logger = logger;
            _oidcClient = oidcClient;
            _configManager = configManager;
            _jwtSecurityTokenProvider = jwtSecurityTokenProvider;
            _activeDirectoryUserAuthenticator = activeDirectoryUserAuthenticator;
        }

        public IActionResult Index(string? page)
        {
            return View();
        }

        [HttpPost("[controller]/signin-ad")]
        public ActionResult SigninAd(string? page, string username, string password)
        {
            if (_configManager.WebServer.LoginMethod != WebServerLoginType.ActiveDirectory &&
                _configManager.WebServer.LoginMethod != WebServerLoginType.OidcAndActiveDirectory)
                return RedirectToAction("Index", "Home", new { error = "Login with crentials not supported", page });

            try
            {
                var user = _activeDirectoryUserAuthenticator.Login(username, password);
                _logger.LogInformation("Login-AD success, {user}", user);
                CreateAuthenticationToken(user);

                return RedirectToAction("Index", PageToController(page));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login-AD user: {user}", username);
                return RedirectToAction("Index", "Home", new { error = ex.Message, page });
            }
        }

        [HttpGet("[controller]/signout")]
        public ActionResult Signout()
        {
            this.ClearAuthenticationToken();
            return RedirectToAction("Index", "Home");
        }

        private static string? PageFromState(string? state)
        {
            if (state == null)
                return null;

            var parts = state.Split('&');
            foreach (var part in parts)
            {
                if (part.StartsWith("page=", StringComparison.CurrentCultureIgnoreCase))
                {
                    return part.Substring("page=".Length);
                }
            }

            return null;
        }

#if false
        // NOTE: to get this to work, the jwt would likely need to contain a few critical
        //       bits of information (everything returned by the OIDC lookup or AD lookup)
        //       The JWT would also have to be signed with the same signing key
        //  SECURITY NOTE: I have commented out this method for fear it could be a security vulnerability.
        //                 theoretically the security is in the signing of the jwt, and if that is valid
        //                 we can trust the user has logged in, but need to think about it.
        //                 We also need to think about how the client would send the token... 
        //                 is it safe for the browser to have access to the token? (probably is safe)
        [HttpGet("signin-jwt")]
        public async Task<ActionResult> SigninJwt(
            [FromQuery(Name = "page")] string? page,
            [FromQuery(Name = "jwt")] string? jwt)
        {
            if (jwt != null)
            {
                // first read the token
                var token = _jwtSecurityTokenProvider.ReadAndValidateToken(jwt);

                // TODO: extract name and type (OIDC or AD), and groups from the token claims

                // TODO: now rebuild the token - this is nescessary because this site config contains roles
            }

            return RedirectToAction("Index", PageToController(page));
        }
#endif


        [HttpGet("signin-oidc")]
        public async Task<ActionResult> SigninOidcGet(
            [FromQuery(Name = "error")] string? error,
            [FromQuery(Name = "error_description")] string? errorDescription,
            [FromQuery(Name = "state")] string? state,
            [FromQuery(Name = "code")] string? code,
            [FromQuery(Name = "id_token")] string? idToken,
            [FromQuery(Name = "access_token")] string? accessToken)
        {
            string? page = PageFromState(state);
            _logger.LogInformation("signin-oidc callback (GET) {sid} state: {state}", HttpContext.Session.Id, state);

            if (_configManager.WebServer.LoginMethod != WebServerLoginType.OIDC &&
                _configManager.WebServer.LoginMethod != WebServerLoginType.OidcAndActiveDirectory)
                return RedirectToAction("Index", "Home", new { page, error = "Login with OIDC not supported" });

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("callback {error}: {errorDescription}", error, errorDescription);
                return RedirectToAction("Index", "Home", new { page, error = string.Format("{0}: {1}", error, errorDescription) });
            }

            if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(idToken) && string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("no id_token, token or code supplied to callback {request}", Print(Request));
                return RedirectToAction("Index", "Home", new { page, error = string.Format("no id_token, token or code supplied to callback") });
            }

            try
            {
                await Login(state, code, idToken, accessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET callback {sid}", HttpContext.Session.Id);
                return RedirectToAction("Index", "Home", new { page, error = ex.Message });
            }

            return RedirectToAction("Index", PageToController(page));
        }

        [HttpPost("signin-oidc")]
        public async Task<ActionResult> SigninOidcPost(IFormCollection formCollection)
        {
            string? error = formCollection["error"];
            string? errorDescription = formCollection["error_description"];
            string? state = formCollection["state"];
            string? code = formCollection["code"];
            string? idToken = formCollection["id_token"];
            string? accessToken = formCollection["access_token"];

            string? page = PageFromState(state);
            _logger.LogInformation("signin-oidc callback (POST) {sid} state: {state}", HttpContext.Session.Id, state);

            if (_configManager.WebServer.LoginMethod != WebServerLoginType.OIDC &&
                _configManager.WebServer.LoginMethod != WebServerLoginType.OidcAndActiveDirectory)
                return RedirectToAction("Index", "Home", new { error = "Login with OIDC not supported" });

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("POST callback {error}: {errorDescription}", error, errorDescription);
                return RedirectToAction("Index", "Home", new { page, error = string.Format("{0}: {1}", error, errorDescription) });
            }

            if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(idToken) && string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Index", "Home", new { page, error = string.Format("no id_token, token or code supplied to callback") });
            }

            try
            {
                await Login(state, code, idToken, accessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST /callback/ {sid}", HttpContext.Session.Id);
                return RedirectToAction("Index", "Home", new { page, error = ex.Message });
            }

            return RedirectToAction("Index", PageToController(page));
        }

        private static string Print(HttpRequest request)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} {1} {2}", request.Method, request.Path, request.QueryString);
            foreach (var header in request.Headers)
            {
                sb.AppendFormat("{0}: {1}", header.Key, header.Value);
            }

            return sb.ToString();
        }

        public void CreateAuthenticationToken(OidcJwtToken idToken)
        {
            var token = _jwtSecurityTokenProvider.CreateAuthenticationToken(idToken);
            _logger.LogInformation("OIDC User Login {claims}", PrintClaims(token));
            HttpContext.Session.SetString("jwtToken", _jwtSecurityTokenProvider.WriteToken(token));
        }

        public void CreateAuthenticationToken(ActiveDirectoryUser user)
        {
            var token = _jwtSecurityTokenProvider.CreateAuthenticationToken(user);
            _logger.LogInformation("AD User Login {claims}", PrintClaims(token));
            HttpContext.Session.SetString("jwtToken", _jwtSecurityTokenProvider.WriteToken(token));
        }

        public void ClearAuthenticationToken()
        {
            HttpContext.Session.Remove("jwtToken");
        }

        /// <summary>
        /// Login with either a code or id_token
        /// </summary>
        /// <param name="state">I use the state to encode the sessionId</param>
        /// <param name="code">the code returned by the authentication attempt</param>
        /// <param name="idToken">the id_token returned by the authentication attempt</param>
        /// <param name="accessToken">the access_token returned by the authentication attempt</param>
        /// <returns>Once the user is logged in.  Throws an exeption otherwise</returns>
        private async Task Login(string? state, string? code, string? idToken, string? accessToken)
        {
            if (state == null)
                return;

            var sessionId = state ?? "unknown";

            _logger.LogDebug("Login postback for session {sid}", sessionId);

            if (code != null)
            {
                await LoginWithCode(_oidcClient, code);
            }
            else
            {
                await LoginWithIdToken(_oidcClient, idToken, accessToken);
            }
        }

        /// <summary>
        /// login with an id-token
        /// </summary>
        /// <param name="idTokenStr"></param>
        /// <param name="accessTokenStr"></param>
        private async Task LoginWithIdToken(IOidcClient client, string? idTokenStr, string? accessTokenStr)
        {
            OidcJwtToken? idToken = null;
            OidcJwtToken? accessToken = null;

            if (idTokenStr != null)
                _ = OidcJwtToken.TryParse(idTokenStr, out idToken);

            if (accessTokenStr != null)
                _ = OidcJwtToken.TryParse(accessTokenStr, out accessToken);

            if (idToken != null)
            {
                // validation throws an exception if it fails
                await client.ValidateJwt(idToken);
                _logger.LogInformation("id_token is valid (signature has been verified)");

                CreateAuthenticationToken(idToken);
            }
        }

        private async Task LoginWithCode(IOidcClient client, string code)
        {
            // if we have a code, then we call a rest api to get the tokens from it
            try
            {
                var codeToken = await client.GetToken(code);
                _ = OidcJwtToken.TryParse(codeToken.IdTokenBase64 ?? string.Empty, out OidcJwtToken? idToken);

                if (idToken != null)
                {
                    // validation throws an exception if it fails
                    await client.ValidateJwt(idToken);
                    _logger.LogInformation("id_token is valid (signature has been verified)");

                    CreateAuthenticationToken(idToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginWithCode");
                throw;
            }
        }

        private string PrintClaims(JwtSecurityToken token)
        {
            return string.Join(", ", from x
                                     in token.Claims
                                     select x.Type + " = " + x.Value);
        }
    }
}
