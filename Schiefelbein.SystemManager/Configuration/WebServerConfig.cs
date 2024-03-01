using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Schiefelbein.SystemManager.Errors;
using Schiefelbein.Common.Web.ActiveDirectory.Config;
using Schiefelbein.Common.Web.Oidc.Config;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Schiefelbein.Common.Web.Configuration;

namespace Schiefelbein.SystemManager.Configuration
{
    public enum WebServerLoginType
    {
        OIDC,
        ActiveDirectory,
        OidcAndActiveDirectory,
    }

    public class WebServerConfig
    {

        [JsonPropertyName("client-debugging")]
        public bool ClientDebugging { get; set; }

        [JsonPropertyName("use-https-redirection")]
        public bool UseHttpsRedirection { get; set; }

        [JsonPropertyName("session-idle-timeout")]
        public TimeSpan SessionIdleTimeout { get; set; }

        [JsonPropertyName("path-base")]
        public string? PathBase { get; set; }

        [JsonPropertyName("login-method")]
        public WebServerLoginType LoginMethod { get; set; }

        [JsonPropertyName("ad-login-text")]
        public string AdLoginText { get; set; }

        [JsonPropertyName("oidc-login-text")]
        public string OidcLoginText { get; set; }

        [JsonPropertyName("jwt-token-authentication")]
        public JwtTokenAuthenticationConfig JwtTokenAuthentication { get; set; }

        [JsonPropertyName("sister-sites")]
        public SisterSiteConfig[] SisterSites { get; set; }

        [JsonPropertyName("oidc")]
        public OidcConfig Oidc { get; set; }

        [JsonPropertyName("active-directory")]
        public ActiveDirectoryConfig ActiveDirectory { get; set; }

        public WebServerConfig()
        {
            UseHttpsRedirection = true;
            SessionIdleTimeout = TimeSpan.FromMinutes(60);
            OidcLoginText = "Login";
            AdLoginText = "Login";
            Oidc = new OidcConfig();
            SisterSites = Array.Empty<SisterSiteConfig>();
            ActiveDirectory = new ActiveDirectoryConfig();
            JwtTokenAuthentication = new JwtTokenAuthenticationConfig();
        }

        public static WebServerConfig Load()
        {
            using var fileStream = new FileStream("Config\\web-server.json", FileMode.Open, FileAccess.Read, FileShare.Read);
            var config = JsonSerializer.Deserialize<WebServerConfig>(fileStream, ConfigManager.SerializationOptions) ??
                throw new ServiceManagerConfigurationException("unable to load config file");

            if (string.IsNullOrWhiteSpace(config.JwtTokenAuthentication.TokenKey))
            {
                config.JwtTokenAuthentication.TokenKey = JwtTokenAuthenticationConfig.GenerateKey(256);
            }

            return config;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions()
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() },
            });
        }
    }
}
