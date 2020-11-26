using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Vlix.HttpServer
{
    public class HttpServerConfig
    {
        public bool EnableHTTP { get; set; } = true;
        public int HTTPPort { get; set; } = 80;
        public bool EnableHTTPS { get; set; } = true;
        public int HTTPSPort { get; set; } = 443;
        public string SSLThumbPrint { get; set; } = null;
        public bool AllowLocalhostConnectionsOnly { get; set; } = false;
        public string WWWDirectory { get; set; } = Path.Combine("[ProgramDataDirectory]", "Vlix", "HTTPServer", "www");
        public string LogDirectory { get; set; } = Path.Combine("[ProgramDataDirectory]", "Vlix", "HTTPServer", "Logs"); 
        public bool EnableCache { get; set; } = true;
        public int OnlyCacheItemsLessThenMB { get; set; } = 10;
        public int MaximumCacheSizeInMB { get; set; } = 250;
        public List<Redirect> Redirects { get; set; } = new List<Redirect>();

        public void LoadConfigFile()
        {
            HttpServerConfig config = new HttpServerConfig();
            //config.Redirects = new List<Redirect>()
            //{
            //    new Redirect() {From= new RedirectFrom() {AnyHostName= true, HostNameWildCard = "*"}, To = new RedirectTo() { Port=80 } },
            //    new HttpToHttpsRedirect()
            //};
            string appDirectory = ConfigurationManager.AppSettings.Get("AppDirectory");
            appDirectory = appDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));


            Directory.CreateDirectory(appDirectory);
            string configFilePath = Path.Combine(appDirectory, "httpserver.json");
            if (File.Exists(configFilePath))
            {
                string configJSONStr = File.ReadAllText(configFilePath);
                config = JsonConvert.DeserializeObject<HttpServerConfig>(configJSONStr);
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
            this.SSLThumbPrint = config.SSLThumbPrint;
            this.LogDirectory = config.LogDirectory;
            this.MaximumCacheSizeInMB = config.MaximumCacheSizeInMB;
            this.OnlyCacheItemsLessThenMB = config.OnlyCacheItemsLessThenMB;
            this.WWWDirectory = config.WWWDirectory;
            this.AllowLocalhostConnectionsOnly = config.AllowLocalhostConnectionsOnly;
            this.EnableCache = config.EnableCache;
            this.Redirects.Clear();
            foreach (var r in config.Redirects) this.Redirects.Add(r);
        }
    }

    public class Redirect
    {
        public bool Enable { get; set; } = false;
        public RedirectFrom From { get; set; } = new RedirectFrom();
        public RedirectTo To { get; set; } = new RedirectTo();

        public string Process(Scheme scheme, string host, int port, string path)
        {
            if (this.Enable)
            {
                bool portMatch = (this.From.AnyPort || (!this.From.AnyPort && (this.From.Port == port)));
                bool hostMatch = (this.From.AnyHostName || (!this.From.AnyHostName && (this.From.GetHostWildCard().IsMatch(host))));
                bool uRLMatch = (this.From.AnyPath || (!this.From.AnyHostName && (this.From.GetURLWildCard().IsMatch(path))));
                if (portMatch && hostMatch && uRLMatch)
                {
                    string redirectScheme = (this.To.Scheme ?? scheme).ToString();
                    string redirectHost = this.To.HostName ?? host;
                    int redirectPort = this.To.Port ?? port;
                    string redirectPath = this.To.Path ?? path;
                    string redirectURL = redirectScheme + "://" + redirectHost + ":" + redirectPort + redirectPath;
                    return redirectURL;
                }
                //System.Security.Cryptography.X509Certificates.StoreName sadsadsa;
            }
            return null;
        }
    }

    public class HttpToHttpsRedirect : Redirect
    {
        public HttpToHttpsRedirect()
        {
            this.Enable = true;
            this.From = new RedirectFrom()
            {
                AnyHostName = true,
                HostNameWildCard = null,
                AnyPort = false,
                Port = 80,
                PathWildCard = "*"
            };

            this.To = new RedirectTo()
            {
                Scheme = Scheme.https,
                HostName = null,
                Port = 443,
                Path = null
            };
        }
    }


    public class RedirectFrom
    {
        public bool AnyHostName { get; set; } = false;
        public string HostNameWildCard { get; set; } = null;
        public bool AnyPort { get; set; } = false;
        public int? Port { get; set; } = null;
        public bool AnyPath { get; set; } = false;
        public string PathWildCard { get; set; } = null;

        Wildcard hostWildCard = null;
        public Wildcard GetHostWildCard()
        {
            if (hostWildCard==null) hostWildCard = new Wildcard(HostNameWildCard);
            return hostWildCard;
        }
        Wildcard uRLWilCard = null;
        public Wildcard GetURLWildCard()
        {
            if (uRLWilCard == null) uRLWilCard = new Wildcard(PathWildCard);
            return uRLWilCard;
        }
    }

    public class RedirectTo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Scheme? Scheme { get; set; } = null;
        public string HostName { get; set; } = null;
        public int? Port { get; set; } = null;
        public string Path { get; set; } = null;
    }

    public enum Scheme { http, https }
}