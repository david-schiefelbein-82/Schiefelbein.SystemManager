using System.Text.Json.Serialization;
using System.Text.Json;
using Schiefelbein.SystemManager.Models;
using System;
using Schiefelbein.Common.Web.Configuration;
using Microsoft.CodeAnalysis.RulesetToEditorconfig;

namespace Schiefelbein.SystemManager.Configuration
{
    public class ConfigManager : IConfigManager
    {
        private static readonly JsonSerializerOptions _toStringOptions = new()
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        };

        public static JsonSerializerOptions SerializationOptions => new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter() },
        };

        [JsonPropertyName("servers")]
        public ServersConfig Servers { get; private set; }

        [JsonPropertyName("web-server")]
        public WebServerConfig WebServer { get; private set; }

        [JsonPropertyName("authorisation")]
        public IAuthorizationConfig Authorization { get; private set; }

        public ConfigManager()
        {
            Servers = ServersConfig.Load();
            WebServer = WebServerConfig.Load();
            Authorization = AuthorizationConfig.Load();
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, _toStringOptions);
        }
    }
}
