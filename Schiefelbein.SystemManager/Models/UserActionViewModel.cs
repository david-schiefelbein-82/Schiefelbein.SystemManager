using Schiefelbein.SystemManager.Configuration;

namespace Schiefelbein.SystemManager.Models
{
    public class UserActionViewModel
    {
        public string? ErrorMessage { get; set; }

        public string? Page { get; set; }

        public string? InfoMessage { get; set; }

        public WebServerLoginType LoginType { get; set; }

        public string OidcLoginText { get; set; }

        public string AdLoginText { get; set; }

        public UserActionViewModel(string? errorMessage, string? page, string? infoMessage, WebServerLoginType loginType, string oidcLoginText, string adLoginText)
        {
            ErrorMessage = errorMessage;
            Page = page;
            InfoMessage = infoMessage;
            LoginType = loginType;
            OidcLoginText = oidcLoginText;
            AdLoginText = adLoginText;
        }
    }
}