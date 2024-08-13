namespace Schiefelbein.SystemManager.Models
{
    public class ServerInfoViewModel(string[] servers, string serverName, int cpuCores, string[] disks)
    {
        public string[] Servers { get; set; } = servers;

        public string ServerName { get; set; } = serverName;

        public int CpuCores { get; set; } = cpuCores;
        public string[] Disks { get; set; } = disks;

        public override string ToString()
        {
            return string.Format("{{ Servers: {0}, ServerName: {1} }}", string.Join(", ", Servers), ServerName);
        }
    }
}