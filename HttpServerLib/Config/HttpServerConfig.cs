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

        public void SaveConfigFile()
        {
            string appDirectory = ConfigurationManager.AppSettings.Get("AppDirectory");
            appDirectory = appDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

            Directory.CreateDirectory(appDirectory);
            string configFilePath = Path.Combine(appDirectory, "httpserver.json");
            string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            File.WriteAllText(configFilePath, jsonString);
        }

        public void LoadConfigFile()
        {
            HttpServerConfig config = new HttpServerConfig();
            string appDirectory = ConfigurationManager.AppSettings.Get("AppDirectory");
            appDirectory = appDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));


            Directory.CreateDirectory(appDirectory);
            string configFilePath = Path.Combine(appDirectory, "httpserver.json");
            if (File.Exists(configFilePath))
            {
                string configJSONStr = File.ReadAllText(configFilePath);
                config = JsonConvert.DeserializeObject<HttpServerConfig>(configJSONStr, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            }
            else
            {
                string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, jsonString);
            }
            config.LogDirectory = config.LogDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            config.WWWDirectory = config.WWWDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            this.EnableHTTP = config.EnableHTTP;
            this.HTTPPort = config.HTTPPort;
            this.EnableHTTPS = config.EnableHTTPS;
            this.HTTPSPort = config.HTTPSPort;
            this.SSLCertificateSubjectName = config.SSLCertificateSubjectName;
            this.SSLCertificateStoreName = config.SSLCertificateStoreName;
            this.LogDirectory = config.LogDirectory;
            this.MaximumCacheSizeInMB = config.MaximumCacheSizeInMB;
            this.OnlyCacheItemsLessThenMB = config.OnlyCacheItemsLessThenMB;
            this.WWWDirectory = config.WWWDirectory;
            this.AllowLocalhostConnectionsOnly = config.AllowLocalhostConnectionsOnly;
            this.EnableCache = config.EnableCache;
            this.Rules.Clear();
            foreach (var r in config.Rules) this.Rules.Add(r);
        }
    }
    public class HttpServerConfigBase : StaticFileProcessorConfig
    {
        [JsonProperty(Order = 1)]
        public bool EnableHTTP { get; set; } = true;
        [JsonProperty(Order = 2)]
        public int HTTPPort { get; set; } = 80;
        [JsonProperty(Order = 3)]
        public bool EnableHTTPS { get; set; } = true;
        [JsonProperty(Order = 4)]
        public int HTTPSPort { get; set; } = 443;
        [JsonProperty(Order = 5)]
        public string SSLCertificateSubjectName { get; set; } = null;
        [JsonProperty(Order = 10)]
        [JsonConverter(typeof(StringEnumConverter))]
        public StoreName SSLCertificateStoreName { get; set; } = StoreName.My;
        [JsonProperty(Order = 11)]
        public bool AllowLocalhostConnectionsOnly { get; set; } = false;
        
      
    }
}