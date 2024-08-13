using Microsoft.AspNetCore.Hosting.Server;
using Schiefelbein.SystemManager.Errors;
using Schiefelbein.SystemManager.Models;
using Schiefelbein.Utilities.SystemManager.Client;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Schiefelbein.SystemManager.Configuration
{
    public class ServerInfoConfig : ISystemMonitorClientConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("systeminfo-url")]
        public string SystemInfoUrl { get; set; }

        [JsonPropertyName("services-url")]
        public string ServicesUrl { get; set; }

        [JsonPropertyName("cpu-cores")]
        public int CpuCores { get; set; }

        [JsonPropertyName("disks")]
        public string[] Disks { get; set; }

        [JsonPropertyName("user")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        public ServerInfoConfig()
        {
            Name = string.Empty;
            SystemInfoUrl = string.Empty;
            ServicesUrl = string.Empty;
            CpuCores = 1;
            Disks = ["C:\\"];
            Username = string.Empty;
            Password = string.Empty;
        }

        public override string ToString()
        {
            return string.Format("{{ name: {0}, systeminfo-url: {1}, services-url: {2} }}", Name, SystemInfoUrl, ServicesUrl);
        }
    }

    public class ServersConfig
    {
        private static readonly JsonSerializerOptions _options = new ()
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        };

        [JsonPropertyName("servers")]
        public ServerInfoConfig[] Servers { get; set; }

        public ServersConfig()
        {
            Servers = [];
        }

        public static ServersConfig Load()
        {
            using var fileStream = new FileStream("Config\\servers.json", FileMode.Open, FileAccess.Read, FileShare.Read);
            var config = JsonSerializer.Deserialize<ServersConfig>(fileStream, ConfigManager.SerializationOptions) ??
                throw new ServiceManagerConfigurationException("unable to load config file");
            return config;
        }

        public void Save()
        {
            using var fileStream = new FileStream("Config\\servers.json", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            JsonSerializer.Serialize(fileStream, this, ConfigManager.SerializationOptions);
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, _options);
        }
    }
}
