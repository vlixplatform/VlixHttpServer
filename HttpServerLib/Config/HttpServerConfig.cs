using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace Vlix.HttpServer
{
    public class HttpServerConfig: HttpServerConfigBase
    {
        [JsonProperty(Order = 20)]
        public string LogDirectory { get; set; } = Path.Combine("[ProgramDataDirectory]", "Vlix", "HTTPServer", "Logs");
        [JsonProperty(Order = 30)]
        public List<Rule> Rules { get; set; } = new List<Rule>();

    }
    public class HttpServerConfigBase : StaticFileProcessorConfig
    {
        [JsonProperty(Order = 1)]
        public bool EnableHTTP { get; set; } = true;
        [JsonProperty(Order = 2)]
        public int HTTPPort { get; set; } = 80;
        [JsonProperty(Order = 3)]
        public bool EnableHTTPS { get; set; } = false;
        [JsonProperty(Order = 4)]
        public int HTTPSPort { get; set; } = 443;
        [JsonProperty(Order = 5)]
        public string SSLCertificateSubjectName { get; set; } = null;
        [JsonProperty(Order = 10)]
        [JsonConverter(typeof(StringEnumConverter))]
        public StoreName SSLCertificateStoreName { get; set; } = StoreName.My;
        [JsonProperty(Order = 11)]
        public bool AllowLocalhostConnectionsOnlyForHttp { get; set; } = false;

    } 
}