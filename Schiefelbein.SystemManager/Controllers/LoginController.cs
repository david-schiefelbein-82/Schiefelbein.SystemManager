using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Schiefelbein.Common.Web;
using Schiefelbein.Common.Web.ActiveDirectory;
using Schiefelbein.Common.Web.AppLocal;
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
            _logger.LogDebug("Created LoginController");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("[controller]/signin-ad")]
        public async Task<ActionResult> SigninAdAsync(string? page, string username, string password, CancellationToken cancellationToken = default)
        {
            if (_configManager.WebServer.LoginMethod != WebServerLoginType.ActiveDirectory &&
                _configManager.WebServer.LoginMethod != WebServerLoginType.OidcAndActiveDirectory)
                return RedirectToAction("Index", "Home", new { error = "Login with crentials not supported", page });

            try
            {
                var user = _activeDirectoryUserAuthenticator.Login(username, password);
                _logger.LogInformation("Login-AD success, {user}", user);
                await CreateAuthenticationTokenAsync(user, cancellationToken);
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


        [HttpGet("signin-oidc")]
        public async Task<ActionResult> SigninOidcGetAsync(
            [FromQuery(Name = "error")] string? error,
            [FromQuery(Name = "error_description")] string? errorDescription,
            [FromQuery(Name = "state")] string? state,
            [FromQuery(Name = "code")] string? code,
            [FromQuery(Name = "id_token")] string? idToken,
            [FromQuery(Name = "access_token")] string? accessToken,
            CancellationToken cancellationToken = default)
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
                await LoginAsync(state, code, idToken, accessToken, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET callback {sid}", HttpContext.Session.Id);
                return RedirectToAction("Index", "Home", new { page, error = ex.Message });
            }

            return RedirectToAction("Index", PageToController(page));
        }

        [HttpPost("signin-oidc")]
        public async Task<ActionResult> SigninOidcPost(IFormCollection formCollection, CancellationToken cancellationToken = default)
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
                await LoginAsync(state, code, idToken, accessToken, cancellationToken);
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

        public async Task CreateAuthenticationTokenAsync(OidcJwtToken idToken, CancellationToken cancellationToken = default)
        {
            var token = _jwtSecurityTokenProvider.CreateAuthenticationToken(idToken);
            var validationResult = await _jwtSecurityTokenProvider.ReadAndValidateTokenAsync(token, cancellationToken);
            _logger.LogInformation("OIDC User Login {claims}", PrintClaims(validationResult));
            HttpContext.Session.SetString("jwtToken", token);
        }

        public async Task CreateAuthenticationTokenAsync(ActiveDirectoryUser adUser, CancellationToken cancellationToken = default)
        {
            var token = _jwtSecurityTokenProvider.CreateAuthenticationToken(adUser);
            var validationResult = await _jwtSecurityTokenProvider.ReadAndValidateTokenAsync(token, cancellationToken);
            _logger.LogInformation("AD User Login {claims}", PrintClaims(validationResult));
            HttpContext.Session.SetString("jwtToken", token);
        }
        public async Task CreateAuthenticationTokenAsync(IAppLocalUser localUser, CancellationToken cancellationToken = default)
        {
            var token = _jwtSecurityTokenProvider.CreateAuthenticationToken(localUser);
            var validationResult = await _jwtSecurityTokenProvider.ReadAndValidateTokenAsync(token, cancellationToken);
            _logger.LogInformation("Local User Login {claims}", PrintClaims(validationResult));
            HttpContext.Session.SetString("jwtToken", token);
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
        private async Task LoginAsync(string? state, string? code, string? idToken, string? accessToken, CancellationToken cancellationToken = default)
        {
            if (state == null)
                return;

            var sessionId = state ?? "unknown";

            _logger.LogDebug("Login postback for session {sid}", sessionId);

            if (code != null)
            {
                await LoginWithCodeAsync(_oidcClient, code, cancellationToken);
            }
            else
            {
                await LoginWithIdTokenAsync(_oidcClient, idToken, accessToken, cancellationToken);
            }
        }

        /// <summary>
        /// login with an id-token
        /// </summary>
        /// <param name="idTokenStr"></param>
        /// <param name="accessTokenStr"></param>
        private async Task LoginWithIdTokenAsync(IOidcClient client, string? idTokenStr, string? accessTokenStr, CancellationToken cancellationToken = default)
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
                await client.ValidateJwtAsync(idToken, cancellationToken);
                _logger.LogInformation("id_token is valid (signature has been verified)");

                await CreateAuthenticationTokenAsync(idToken, cancellationToken);
            }
        }

        private async Task LoginWithCodeAsync(IOidcClient client, string code, CancellationToken cancellationToken = default)
        {
            // if we have a code, then we call a rest api to get the tokens from it
            try
            {
                var codeToken = await client.GetTokenAsync(code, cancellationToken);
                _ = OidcJwtToken.TryParse(codeToken.IdTokenBase64 ?? string.Empty, out OidcJwtToken? idToken);

                if (idToken != null)
                {
                    // validation throws an exception if it fails
                    await client.ValidateJwtAsync(idToken, cancellationToken);
                    _logger.LogInformation("id_token is valid (signature has been verified)");

                    await CreateAuthenticationTokenAsync(idToken, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginWithCode");
                throw;
            }
        }

        private static string PrintClaims(TokenValidationResult validationResult)
        {
            var sb = new StringBuilder();
            int count = 0;
            foreach (var kvp in validationResult.Claims)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                if (count++ == 0)
                    sb.Append(", ");

                if (value is string valueStr)
                {
                    sb.AppendFormat("{0}: {1}", key, valueStr);
                }
                else if (value is IList<object> list)
                {
                    sb.AppendFormat("{0}: [{1}]", key, string.Join(',', list));
                }
            }

            return sb.ToString();
        }
    }
}
