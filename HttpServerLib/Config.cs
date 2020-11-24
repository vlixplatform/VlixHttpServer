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
        public RedirectFrom From { get; set; } = new RedirectFrom();
        public RedirectTo To { get; set; } = new RedirectTo();
    }

    public class HttpToHttpsRedirect : Redirect
    {
        public HttpToHttpsRedirect()
        {
            this.From = new RedirectFrom()
            {
                AnyHostName = true,
                HostNameWildCard = null,
                AnyPort = false,
                Port = 80,
                URLWildCard = "*"
            };

            this.To = new RedirectTo()
            {
                HostName = null,
                Port = 443,
                HTTPS = true,
                RewriteURLTo = null
            };
        }
    }


    public class RedirectFrom
    {
        public bool AnyHostName { get; set; } = true;
        public string HostNameWildCard { get; set; } = null;
        public bool AnyPort { get; set; } = true;
        public int? Port { get; set; } = null;

        public bool AnyURL { get; set; } = true;
        public string URLWildCard { get; set; } = null;

        Wildcard hostWildCard = null;
        public Wildcard GetHostWildCard()
        {
            if (hostWildCard==null) hostWildCard = new Wildcard(HostNameWildCard);
            return hostWildCard;
        }
        Wildcard uRLWilCard = null;
        public Wildcard GetURLWildCard()
        {
            if (uRLWilCard == null) uRLWilCard = new Wildcard(URLWildCard);
            return uRLWilCard;
        }


    }

    public class RedirectTo
    {
        public string HostName { get; set; } = null;
        public int? Port { get; set; } = null;
        public bool? HTTPS { get; set; } = null;
        public string RewriteURLTo { get; set; } = null;
    }
}