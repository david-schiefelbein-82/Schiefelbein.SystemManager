using Schiefelbein.SystemManager.Configuration;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Schiefelbein.SystemManager.Models
{
    public class WaitForServiceStatusResult
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public bool Success { get; set; }

        public string Error { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceStatus Status { get; set; }

        public WaitForServiceStatusResult(string id, string name, ServiceStatus status, bool success, string error)
        {
            Id = id;
            Name = name;
            Status = status;
            Success = success;
            Error = error;
        }

        public override string ToString()
        {
            return string.Format("{{ id: {0}, name: {1}, success: {2}, error: {3}, status: {4} }}", Name, Success, Error, Status);
        }
    }
}
