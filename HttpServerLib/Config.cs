using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Configuration;

namespace Vlix.HttpServer
{
    public class HttpServerConfig
    {
        public bool EnableHTTP { get; set; } = true;
        public int HTTPPort { get; set; } = 80;
        public bool EnableHTTPS { get; set; } = true;
        public int HTTPSPort { get; set; } = 443;
        public bool AllowLocalhostConnectionsOnly { get; set; } = false;
        public string WWWDirectory { get; set; } = @"[ProgramDataDirectory]\Vlix\HTTPServer\www";
        public string LogDirectory { get; set; } = @"[ProgramDataDirectory]\Vlix\HTTPServer\Logs";
        public bool EnableCache { get; set; } = true;
        public int OnlyCacheItemsLessThenMB { get; set; } = 10;
        public int MaximumCacheSizeInMB { get; set; } = 250;
        public List<Redirect> Redirects { get; set; } = new List<Redirect>();
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