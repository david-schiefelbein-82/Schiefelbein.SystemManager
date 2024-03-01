using System;
using System.DirectoryServices.ActiveDirectory;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Schiefelbein.SystemManager.Data;

namespace Schiefelbein.SystemManager.Models
{
    public class ServerInfoViewModel
    {
        public string[] Servers { get; set; }

        public string ServerName { get; set; }

        public int CpuCores { get; set; }


        public ServerInfoViewModel(string[] servers, string serverName, int cpuCores)
        {
            Servers = servers;
            ServerName = serverName;
            CpuCores = cpuCores;
        }

        public override string ToString()
        {
            return string.Format("{{ Servers: {0}, ServerName: {1} }}", string.Join(", ", Servers), ServerName);
        }
    }
}