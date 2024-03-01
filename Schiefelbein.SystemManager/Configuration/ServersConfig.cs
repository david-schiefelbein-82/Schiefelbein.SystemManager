using Microsoft.AspNetCore.Hosting.Server;
using Schiefelbein.SystemManager.Errors;
using Schiefelbein.SystemManager.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Schiefelbein.SystemManager.Configuration
{
    public class ServerInfoConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("systeminfo-url")]
        public string SystemInfoUrl { get; set; }

        [JsonPropertyName("services-url")]
        public string ServicesUrl { get; set; }

        [JsonPropertyName("cpu-cores")]
        public int CpuCores { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        public ServerInfoConfig()
        {
            Name = string.Empty;
            SystemInfoUrl = string.Empty;
            ServicesUrl = string.Empty;
            CpuCores = 1;
            User = string.Empty;
            Password = string.Empty;
        }

        public override string ToString()
        {
            return string.Format("{{ name: {0}, systeminfo-url: {1}, services-url: {2} }}", Name, SystemInfoUrl, ServicesUrl);
        }
    }

    public class ServersConfig
    {
        [JsonPropertyName("servers")]
        public ServerInfoConfig[] Servers { get; set; }

        public ServersConfig()
        {
            Servers = Array.Empty<ServerInfoConfig>();
        }

        public static ServersConfig Load()
        {
            using var fileStream = new FileStream("Config\\servers.json", FileMode.Open, FileAccess.Read, FileShare.Read);
            var config = JsonSerializer.Deserialize<ServersConfig>(fileStream, ConfigManager.SerializationOptions) ??
                throw new ServiceManagerConfigurationException("unable to load config file");
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
