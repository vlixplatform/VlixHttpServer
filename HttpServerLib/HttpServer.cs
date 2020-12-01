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
    public class HttpServer: StaticFileProcessor
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

        public HttpServer(string wWWDirectory, int httpPort = 80, int httpsPort = 443, bool enableCache = true, int onlyCacheItemsLessThenMB = 10, int maximumCacheSizeInMB = 500,
            bool allowLocalhostConnectionsOnly = false) : base(new StaticFileProcessorConfig() { WWWDirectory = wWWDirectory, EnableCache = enableCache, MaximumCacheSizeInMB = maximumCacheSizeInMB, OnlyCacheItemsLessThenMB = onlyCacheItemsLessThenMB })
        {
            CommonConstructor((new CancellationTokenSource()).Token, new HttpServerConfig()
            {
                WWWDirectory = wWWDirectory,
                EnableHTTP = true,
                HTTPPort = httpPort,
                EnableHTTPS = true,
                HTTPSPort = httpsPort,
                EnableCache = enableCache,
                OnlyCacheItemsLessThenMB =  onlyCacheItemsLessThenMB,
                MaximumCacheSizeInMB = maximumCacheSizeInMB,
                AllowLocalhostConnectionsOnly = allowLocalhostConnectionsOnly
            });
        }

        public HttpServer(CancellationToken cancellationToken, string wWWDirectory, int httpPort = 80, int httpsPort = 443, bool enableCache = true, int onlyCacheItemsLessThenMB = 10, 
            int maximumCacheSizeInMB = 500, bool allowLocalhostConnectionsOnly = false) : base(new StaticFileProcessorConfig() { WWWDirectory = wWWDirectory, EnableCache = enableCache, OnlyCacheItemsLessThenMB = onlyCacheItemsLessThenMB, MaximumCacheSizeInMB = maximumCacheSizeInMB })
        {
            CommonConstructor(cancellationToken, new HttpServerConfig()
            {
                WWWDirectory = wWWDirectory,
                EnableHTTP = true,
                HTTPPort = httpPort,
                EnableHTTPS = true,
                HTTPSPort = httpsPort,
                EnableCache = enableCache,
                OnlyCacheItemsLessThenMB = onlyCacheItemsLessThenMB,
                MaximumCacheSizeInMB = maximumCacheSizeInMB,
                AllowLocalhostConnectionsOnly = allowLocalhostConnectionsOnly
            });            
        }
        public HttpServer(CancellationToken cancellationToken, HttpServerConfig httpServerConfig) : 
            base(httpServerConfig)
        {
            CommonConstructor(cancellationToken, httpServerConfig);
        }

        public HttpServer(HttpServerConfig httpServerConfig) : base(httpServerConfig)
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
     
     
        public async Task StartAsync()
        {
            this.OnInfoLog?.Invoke("Starting Vlix HTTP Server...");
            await Services.TryBindSSLCertToPort(443, this.Config.SSLCertificateSubjectName, this.Config.SSLCertificateStoreName, (log) => this.OnInfoLog?.Invoke(log), (log) => this.OnErrorLog?.Invoke(log)); //Vlix Platform

            _listener = new HttpListener();
            
            if (this.Config.EnableHTTP) _listener.Prefixes.Add("http://*:" + this.Config.HTTPPort.ToString() + "/");
            if (this.Config.EnableHTTPS) _listener.Prefixes.Add("https://*:" + this.Config.HTTPSPort.ToString() + "/");
            if (!this.Config.EnableHTTP && !this.Config.EnableHTTPS)
            {
                this.OnInfoLog?.Invoke("Both HTTP (Port " + this.Config.HTTPPort + ") and HTTPS (Port " + this.Config.HTTPSPort + ") is disabled");
                return;
            }
            if (this.Config.EnableHTTP && !this.Config.EnableHTTPS) this.OnInfoLog?.Invoke("Listening to port " + this.Config.HTTPPort + "(HTTP), Directory = '" + this.Config.WWWDirectory + "'");
            if (!this.Config.EnableHTTP && this.Config.EnableHTTPS) this.OnInfoLog?.Invoke("Listening to port " + this.Config.HTTPSPort + "(HTTPS), Directory = '" + this.Config.WWWDirectory + "'");
            if (this.Config.EnableHTTP && this.Config.EnableHTTPS) this.OnInfoLog?.Invoke("Listening to port " + this.Config.HTTPPort + "(HTTP) and " + this.Config.HTTPSPort + "(HTTPS), Directory = '" + this.Config.WWWDirectory + "'");
            _listener.Start();
            _listener.BeginGetContext(OnContext, null); //The thread stops here waiting for content to come
            this.OnInfoLog?.Invoke("Vlix HTTP Server Started!");
        }

        private async void OnContext(IAsyncResult result)
        {
            if (!_listener.IsListening) return;
            HttpListenerContext context = _listener.EndGetContext(result);
            _listener.BeginGetContext(OnContext, null);

            try
            {
                string callerIP = context.Request.RemoteEndPoint.Address.ToString();
                string schemeStr = context.Request.Url.Scheme;
                string host = context.Request.Url.Host;
                int port = context.Request.Url.Port;
                string absolutePath = context.Request.Url.AbsolutePath;
                string absoluteURL = context.Request.Url.AbsoluteUri;
                if (string.IsNullOrWhiteSpace(absolutePath)) absolutePath = "/";
                if (this.Config.AllowLocalhostConnectionsOnly)
                {
                    if (!context.Request.IsLocal)
                    {
                        string msg = callerIP + " requested '" + absoluteURL + "' > Blocked as caller is not local ('AllowLocalhostConnectionsOnly' is set to 'true')";
                        this.OnWarningLog?.Invoke(msg);
                        await SendErrorResponsePage(context, msg, HttpStatusCode.Unauthorized).ConfigureAwait(false);
                        return;
                    }
                }

                //Process Rules
                if (this.Config.Rules != null)
                {
                    int ruleNum = 0;
                    foreach (Rule rule in this.Config.Rules)
                    {
                        ruleNum++;
                        Scheme scheme = Scheme.http;
                        if (string.Equals(schemeStr, "https", StringComparison.OrdinalIgnoreCase)) scheme = Scheme.https;
                        if (rule.IsMatch(scheme, host, port, absolutePath))
                        {
                            ProcessResult processResult = await rule.ResponseAction.ProcessAsync(callerIP,scheme,host, port,absolutePath, context, this);
                            if (processResult.Log)
                            {
                                string msg = null; if (!string.IsNullOrWhiteSpace(processResult.Message)) msg = " > " + processResult.Message;
                                if (processResult.LogLevel == LogLevel.Info) this.OnInfoLog?.Invoke(callerIP + " requested '" + absoluteURL + "' > Rule #" + ruleNum + msg);                                    
                                if (processResult.LogLevel == LogLevel.Warning) this.OnWarningLog?.Invoke(callerIP + " requested '" + absoluteURL + msg);
                                if (processResult.LogLevel == LogLevel.Error) this.OnErrorLog?.Invoke(callerIP + " requested '" + absoluteURL + "' > Rule #" + ruleNum + msg);                                
                            }
                            if (processResult.SendErrorResponsePage) { await this.SendErrorResponsePage(context, processResult.Message, processResult.SendErrorResponsePage_HttpStatusCode); return; }
                            if (!processResult.ContinueNextRule)
                            {                                
                                context.Response?.Close();
                                return;
                            }
                        }
                    }
                    
                }

                //Process Request
                HTTPStreamResult httpStreamResult = await this.ProcessRequestAsync(callerIP, absolutePath, this.Config.WWWDirectory).ConfigureAwait(false);
                this.OnHTTPStreamResult?.Invoke(httpStreamResult);
                if (httpStreamResult.HttpStatusCode == HttpStatusCode.OK)
                {
                    await this.SendToOutputAsync(httpStreamResult, context);
                    this.OnInfoLog?.Invoke(callerIP + " requested '" + absoluteURL + "' > Served '" + httpStreamResult.FileToRead + "'");
                }
                else
                {
                    if (httpStreamResult.HttpStatusCode == HttpStatusCode.NotFound)
                    {
                        if (!httpStreamResult.FaviconError) this.OnWarningLog?.Invoke(callerIP + " requested '" + absoluteURL + "' > '" + httpStreamResult.FileToRead + "' does not exist. Returning NOT FOUND");
                    }
                    if (httpStreamResult.HttpStatusCode == HttpStatusCode.InternalServerError)
                    {
                        this.OnErrorLog?.Invoke(httpStreamResult.ErrorMsg);
                    }
                    await SendErrorResponsePage(context, httpStreamResult.ErrorMsg, httpStreamResult.HttpStatusCode);
                }
            }
            catch (Exception ex)
            {
                await SendErrorResponsePage(context, ex.ToString(), HttpStatusCode.InternalServerError);
                this.OnErrorLog?.Invoke(ex.ToString());
            }
        }

        internal async Task SendToOutputAsync(HTTPStreamResult httpStreamResult, HttpListenerContext context)
        {
            context.Response.ContentLength64 = httpStreamResult.MemoryStream.Length;
            context.Response.ContentType = _mimeTypeMappings.TryGetValue(System.IO.Path.GetExtension(httpStreamResult.FileToRead), out string mime) ? mime : "application/octet-stream";
            context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
            if (!String.IsNullOrEmpty(httpStreamResult?.FileToRead)) context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(httpStreamResult.FileToRead).ToString("r"));
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await httpStreamResult.MemoryStream.CopyToAsync(context.Response.OutputStream);
        }

        internal async Task SendErrorResponsePage(HttpListenerContext context, string errorMsg, HttpStatusCode httpStatusCode)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(errorMsg);
                context.Response.StatusCode = (int)httpStatusCode;
                context.Response.ContentType = "text/html";
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.ContentLength64 = data.LongLength;
                await context.Response.OutputStream.WriteAsync(data, 0, data.Length);
            }
            finally
            {
                context.Response?.Close();
            }
        }

       
        public void Stop() { this.OnInfoLog?.Invoke("Stopping Vlix HTTP Server...");  this.Shutdown(); _listener?.Stop(); this.OnInfoLog?.Invoke("Vlix HTTP Server Stopped!"); }

    }





}