using System.Text.Json.Serialization;
using System.Text;

namespace Schiefelbein.SystemManager.Models
{
#if false
    public enum ServiceStatus
    {
        Unknown = 0,
        Stopped = 1,
        StartPending,
        StopPending,
        Running,
        ContinuePending,
        PausePending,
        Paused
    }

    public enum ServiceStartMode
    {
        Unknown = -1,
        Boot = 0,
        System,
        Automatic,
        Manual,
        Disabled
    }
    public class ServiceViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceStartMode StartMode { get; set; }


        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceStatus Status { get; set; }

        public bool IsFavourite { get; set; }

        public ServiceViewModel()
        {
            Id = string.Empty;
            Name = string.Empty;
            DisplayName = string.Empty;
            Status = ServiceStatus.Unknown;
            IsFavourite = false;
            StartMode = ServiceStartMode.Manual;
        }

        public ServiceViewModel(string name, string displayName, ServiceStatus status, bool isFavourite)
        {
            Id = ToId(name);
            Name = name;
            DisplayName = displayName;
            Status = status;
            IsFavourite = isFavourite;
        }

        private static string FromId(string name)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < name.Length; ++i)
            {
                var ch = name[i];
                if (ch == '-')
                {
                    var end = name.IndexOf('-', ch + 1);
                    if (end > ch)
                    {
                        var code = name.Substring(ch + 1, end - i);
                        int charCode = Convert.ToInt32(code, 16);
                        sb.Append((char)charCode);
                        i = end + 1;
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        private static string ToId(string name)
        {
            var sb = new StringBuilder();
            foreach (var ch in name)
            {
                if (!char.IsLetterOrDigit(ch))
                {
                    int code = ch;
                    sb.Append(string.Format("-{0:x}-", code));
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }
    }
#endif
}