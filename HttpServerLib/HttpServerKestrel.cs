using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Text;

namespace  Vlix.HttpServer
{
    public class HttpServerKestrel
    {
        public new HttpServerConfig Config { get; set; } = null;
        //public bool EnableHTTP { get; internal set; }
        //public int HTTPPort { get; internal set; }
        //public bool EnableHTTPS { get; internal set; }
        //public int HTTPSPort { get; internal set; }
        //public bool AllowLocalhostConnectionsOnly { get; set; } = false;
        public CancellationToken CancellationToken { get; internal set; }
        //public List<Rule> Rules { get; set; } = null;

        #region MimeTypings
        private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            {".mp4", "video/mp4"},
            {".asf", "video/x-ms-asf"},
            {".asx", "video/x-ms-asf"},
            {".avi", "video/x-msvideo"},
            {".bin", "application/octet-stream"},
            {".cco", "application/x-cocoa"},
            {".crt", "application/x-x509-ca-cert"},
            {".css", "text/css"},
            {".deb", "application/octet-stream"},
            {".der", "application/x-x509-ca-cert"},
            {".dll", "application/octet-stream"},
            {".dmg", "application/octet-stream"},
            {".ear", "application/java-archive"},
            {".eot", "application/octet-stream"},
            {".exe", "application/octet-stream"},
            {".flv", "video/x-flv"},
            {".gif", "image/gif"},
            {".hqx", "application/mac-binhex40"},
            {".htc", "text/x-component"},
            {".htm", "text/html"},
            {".html", "text/html"},
            {".ico", "image/x-icon"},
            {".img", "application/octet-stream"},
            {".iso", "application/octet-stream"},
            {".jar", "application/java-archive"},
            {".jardiff", "application/x-java-archive-diff"},
            {".jng", "image/x-jng"},
            {".jnlp", "application/x-java-jnlp-file"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".js", "application/x-javascript"},
            {".mml", "text/mathml"},
            {".mng", "video/x-mng"},
            {".mov", "video/quicktime"},
            {".mp3", "audio/mpeg"},
            {".mpeg", "video/mpeg"},
            {".mpg", "video/mpeg"},
            {".msi", "application/octet-stream"},
            {".msm", "application/octet-stream"},
            {".msp", "application/octet-stream"},
            {".pdb", "application/x-pilot"},
            {".pdf", "application/pdf"},
            {".pem", "application/x-x509-ca-cert"},
            {".pl", "application/x-perl"},
            {".pm", "application/x-perl"},
            {".png", "image/png"},
            {".prc", "application/x-pilot"},
            {".ra", "audio/x-realaudio"},
            {".rar", "application/x-rar-compressed"},
            {".rpm", "application/x-redhat-package-manager"},
            {".rss", "text/xml"},
            {".run", "application/x-makeself"},
            {".sea", "application/x-sea"},
            {".shtml", "text/html"},
            {".sit", "application/x-stuffit"},
            {".swf", "application/x-shockwave-flash"},
            {".tcl", "application/x-tcl"},
            {".tk", "application/x-tcl"},
            {".txt", "text/plain"},
            {".war", "application/java-archive"},
            {".wbmp", "image/vnd.wap.wbmp"},
            {".wmv", "video/x-ms-wmv"},
            {".xml", "text/xml"},
            {".xpi", "application/x-xpinstall"},
            {".zip", "application/zip"},
        };
    #endregion

        private HttpListener _listener = null;
        public Action<string> OnErrorLog { get; set; }
        public Action<string> OnWarningLog { get; set; }
        public Action<string> OnInfoLog { get; set; }
        public Action<HTTPStreamResult> OnHTTPStreamResult { get; set; }

        public HttpServerKestrel(CancellationToken cancellationToken, HttpServerConfig httpServerConfig)
        {
            CommonConstructor(cancellationToken, httpServerConfig);
        }

        public HttpServerKestrel(HttpServerConfig httpServerConfig)
        {
            CommonConstructor((new CancellationTokenSource()).Token, httpServerConfig);
        }

        private void CommonConstructor(CancellationToken cancellationToken, HttpServerConfig httpServerConfig)
        {
            if (httpServerConfig.WWWDirectory.Length < 1) throw new Exception("path cannot be empty!");
            if (httpServerConfig.WWWDirectory.EndsWith(Path.DirectorySeparatorChar)) httpServerConfig.WWWDirectory = httpServerConfig.WWWDirectory.Substring(0, httpServerConfig.WWWDirectory.Length - 1);
            this.CancellationToken = cancellationToken;
            this.Config = httpServerConfig;
        }
    }
}