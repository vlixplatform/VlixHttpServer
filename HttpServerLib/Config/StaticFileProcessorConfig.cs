using Newtonsoft.Json;
using System;
using System.IO;

namespace Vlix.HttpServer
{
    public class StaticFileProcessorConfig
    {
        private string _WWWDirectory = Path.Combine("[ProgramDataDirectory]", "Vlix", "HTTPServer", "www");

        private string _WWWDirectoryParsed = null;
        public string WWWDirectoryParsed() { if (_WWWDirectoryParsed == null) _WWWDirectoryParsed = this.WWWDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)); return _WWWDirectoryParsed; }        
        [JsonProperty(Order = 6)]
        public string WWWDirectory { get { return _WWWDirectory; } set { _WWWDirectory = value; _WWWDirectoryParsed = null; } }
        [JsonProperty(Order = 11)]
        public bool EnableCache { get; set; } = true;
        [JsonProperty(Order = 12)]
        public int OnlyCacheItemsLessThenMB { get; set; } = 10;
        [JsonProperty(Order = 13)]
        public int MaximumCacheSizeInMB { get; set; } = 250;
    }


}