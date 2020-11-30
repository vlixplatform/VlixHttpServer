using Newtonsoft.Json;
using System.IO;

namespace Vlix.HttpServer
{
    public class StaticFileProcessorConfig
    {
        [JsonProperty(Order = 6)]
        public string WWWDirectory { get; set; } = Path.Combine("[ProgramDataDirectory]", "Vlix", "HTTPServer", "www");
        [JsonProperty(Order = 11)]
        public bool EnableCache { get; set; } = true;
        [JsonProperty(Order = 12)]
        public int OnlyCacheItemsLessThenMB { get; set; } = 10;
        [JsonProperty(Order = 13)]
        public int MaximumCacheSizeInMB { get; set; } = 250;
    }


}