using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace Vlix.HttpServer
{
    public class HttpServerConfig: HttpServerConfigBase
    {
        public HttpServerConfig() { }
        public HttpServerConfig(UtilityConfig utilityConfig)
        {
            this.EnableHTTP = true; //Http must alays be enables
            this.HTTPPort = utilityConfig.HTTPPort;
            this.AllowLocalhostConnectionsOnlyForHttp = utilityConfig.AllowLocalhostConnectionsOnlyForHttp;
            this.EnableHTTPS = utilityConfig.EnableHTTPS;
            this.HTTPSPort = utilityConfig.HTTPSPort;
            this.SSLCertificateStoreName = utilityConfig.SSLCertificateStoreName;
            this.SSLCertificateSubjectName = utilityConfig.SSLCertificateSubjectName;
            this.EnableCache = false;
            this.Rules = null;
        }
        public void Update(HttpServerConfig httpServerConfig)
        {
            this.LogDirectory = httpServerConfig.LogDirectory;
            this.Rules.Clear();
            foreach (var r in httpServerConfig.Rules) this.Rules.Add(r);
            this.EnableHTTP = httpServerConfig.EnableHTTP;
            this.HTTPPort = httpServerConfig.HTTPPort;
            this.EnableHTTPS = httpServerConfig.EnableHTTPS;
            this.HTTPSPort = httpServerConfig.HTTPSPort;
            this.EnableCache = httpServerConfig.EnableCache;
            this.MaximumCacheSizeInMB = httpServerConfig.MaximumCacheSizeInMB;
            this.OnlyCacheItemsLessThenMB = httpServerConfig.OnlyCacheItemsLessThenMB;
            this.WWWDirectory = httpServerConfig.WWWDirectory;
            this.LogDirectory = httpServerConfig.LogDirectory;
            this.SSLCertificateSubjectName = httpServerConfig.SSLCertificateSubjectName;
            this.SSLCertificateStoreName = httpServerConfig.SSLCertificateStoreName;
            this.AllowLocalhostConnectionsOnlyForHttp = httpServerConfig.AllowLocalhostConnectionsOnlyForHttp;
        }

        private string _LogDirectoryParsed = null;
        [DebuggerStepThrough]
        public string LogDirectoryParsed() { if (_LogDirectoryParsed == null) _LogDirectoryParsed = this.LogDirectory?.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)); ; return _LogDirectoryParsed; }
        private string _LogDirectory = Path.Combine("[ProgramDataDirectory]", "Vlix", "HTTPServer", "Logs");
        [JsonProperty(Order = 20)]
        public string LogDirectory { get { return _LogDirectory; } set { _LogDirectory = value; _LogDirectoryParsed = null; } }
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