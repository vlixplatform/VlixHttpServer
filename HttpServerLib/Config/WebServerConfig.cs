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
    public class ConfigUtiltyConfig
    {
        [JsonProperty(Order = 51)]
        public int HTTPPort { get; set; } = 33171;
        [JsonProperty(Order = 52)]
        public bool EnableHTTPS { get; set; } = false;
        [JsonProperty(Order = 53)]
        public int HTTPSPort { get; set; } = 33170;
        [JsonProperty(Order = 54)]
        public string SSLCertificateSubjectName { get; set; } = null;
        [JsonProperty(Order = 55)]
        [JsonConverter(typeof(StringEnumConverter))]
        public StoreName SSLCertificateStoreName { get; set; } = StoreName.My;
        [JsonProperty(Order = 56)]
        public bool AllowLocalhostConnectionsOnlyForHttp { get; set; } = true;
        [JsonProperty(Order = 57)]
        public string ConfigUsername { get; set; } = "Administrator";
        [JsonProperty(Order = 58)]
        public string ConfigPasswordHash { get; set; } = null;
        
        
    }

    public class WebServerConfig : HttpServerConfig
    {
        [JsonProperty(Order = 40)]
        public ConfigUtiltyConfig ConfigUtility { get; set; } = new ConfigUtiltyConfig();
        public void SaveConfigFile(string configFileName)
        {
            string appDirectory = ConfigurationManager.AppSettings.Get("AppDirectory");
            if (appDirectory == null) appDirectory = Environment.CurrentDirectory;
            else appDirectory = appDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            Directory.CreateDirectory(appDirectory);
            string configFilePath = Path.Combine(appDirectory, configFileName);
            string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            File.WriteAllText(configFilePath, jsonString);
        }

        public void LoadConfigFile(string configFileName)
        {
            WebServerConfig config = new WebServerConfig();
            string appDirectory = ConfigurationManager.AppSettings.Get("AppDirectory");
            if (appDirectory == null) appDirectory = Environment.CurrentDirectory;
            else appDirectory = appDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            Directory.CreateDirectory(appDirectory);
            string configFilePath = Path.Combine(appDirectory, configFileName);
            if (File.Exists(configFilePath))
            {
                string configJSONStr = File.ReadAllText(configFilePath);
                config = JsonConvert.DeserializeObject<WebServerConfig>(configJSONStr, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
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
            this.AllowLocalhostConnectionsOnlyForHttp = config.AllowLocalhostConnectionsOnlyForHttp;
            this.EnableCache = config.EnableCache;
            this.ConfigUtility.AllowLocalhostConnectionsOnlyForHttp = config.ConfigUtility.AllowLocalhostConnectionsOnlyForHttp;
            this.ConfigUtility.ConfigUsername = config.ConfigUtility.ConfigUsername;
            this.ConfigUtility.ConfigPasswordHash = config.ConfigUtility.ConfigPasswordHash;
            this.ConfigUtility.EnableHTTPS = config.ConfigUtility.EnableHTTPS;
            this.ConfigUtility.HTTPPort = config.ConfigUtility.HTTPPort;
            this.ConfigUtility.HTTPSPort = config.ConfigUtility.HTTPSPort;
            this.ConfigUtility.SSLCertificateStoreName = config.ConfigUtility.SSLCertificateStoreName;
            this.ConfigUtility.SSLCertificateSubjectName = config.ConfigUtility.SSLCertificateSubjectName;            
            this.Rules.Clear();
            foreach (var r in config.Rules) this.Rules.Add(r);
        }
    }
}