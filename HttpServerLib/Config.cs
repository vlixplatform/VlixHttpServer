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
                FromAnyPort = false,
                FromPort = 80,
                URLWildCardMatch = "*"
            };

            this.To = new RedirectTo()
            {
                ToSamePort = false,
                ToPort = 443,
                EnableURLRewrite = false,
                RewriteURLAs = null
            };
        }
    }


    public class RedirectFrom
    {
        public bool FromAnyPort { get; set; } = false;
        public int? FromPort { get; set; } = null;
        public string URLWildCardMatch { get; set; } = "*";
    }

    public class RedirectTo
    {
        public bool ToSamePort { get; set; } = false;
        public int? ToPort { get; set; } = null;
        public bool EnableURLRewrite { get; set; } = false;
        public string RewriteURLAs { get; set; } = null;
    }
}