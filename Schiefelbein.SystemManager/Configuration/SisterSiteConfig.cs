﻿using System.Text.Json.Serialization;

namespace Schiefelbein.SystemManager.Configuration
{
    public class SisterSiteConfig
    {

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        public SisterSiteConfig()
        {
            Name = string.Empty;
            Url = string.Empty;
        }
    }
}
