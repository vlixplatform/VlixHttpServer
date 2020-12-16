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
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using System.Web;
using System.Collections.Specialized;

namespace Vlix.HttpServer
{
    public class HttpServer: StaticFileProcessor
    {
        public HttpClient HttpClient = new HttpClient();
        public new HttpServerConfig Config { get; set; } = null;
        public CancellationToken CancellationToken { get; internal set; }
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
        public List<WebAPIAction> WebAPIs { get; set; } = new List<WebAPIAction>();
        public HttpServer(string wWWDirectory, int httpPort, int httpsPort = 443, string certSubjectName = null, StoreName certStoreName = StoreName.My, bool enableCache = true, int onlyCacheItemsLessThenMB = 10, int maximumCacheSizeInMB = 500,
            bool allowLocalhostConnectionsOnlyForHttp = false) : base(new StaticFileProcessorConfig() { WWWDirectory = wWWDirectory, EnableCache = enableCache, MaximumCacheSizeInMB = maximumCacheSizeInMB, OnlyCacheItemsLessThenMB = onlyCacheItemsLessThenMB })
        {
            CommonConstructor((new CancellationTokenSource()).Token, new HttpServerConfig()
            {
                WWWDirectory = wWWDirectory,
                EnableHTTP = true,
                HTTPPort = httpPort,
                EnableHTTPS = (string.IsNullOrWhiteSpace(certSubjectName)) ? false : true,
                HTTPSPort = httpsPort,
                EnableCache = enableCache,
                OnlyCacheItemsLessThenMB =  onlyCacheItemsLessThenMB,
                MaximumCacheSizeInMB = maximumCacheSizeInMB,
                AllowLocalhostConnectionsOnlyForHttp = allowLocalhostConnectionsOnlyForHttp,
                SSLCertificateSubjectName = certSubjectName,
                SSLCertificateStoreName = StoreName.My,
            });
        }
        public HttpServer(CancellationToken cancellationToken, string wWWDirectory, int httpPort = 80, int httpsPort = 443, string certSubjectName = null, StoreName certStoreName = StoreName.My, bool enableCache = true, int onlyCacheItemsLessThenMB = 10, 
            int maximumCacheSizeInMB = 500, bool allowLocalhostConnectionsOnlyForHttp = false) : base(new StaticFileProcessorConfig() { WWWDirectory = wWWDirectory, EnableCache = enableCache, OnlyCacheItemsLessThenMB = onlyCacheItemsLessThenMB, MaximumCacheSizeInMB = maximumCacheSizeInMB })
        {
            CommonConstructor(cancellationToken, new HttpServerConfig()
            {
                WWWDirectory = wWWDirectory,
                EnableHTTP = true,
                HTTPPort = httpPort,
                EnableHTTPS = (string.IsNullOrWhiteSpace(certSubjectName)) ? false : true,
                HTTPSPort = httpsPort,
                EnableCache = enableCache,
                OnlyCacheItemsLessThenMB = onlyCacheItemsLessThenMB,
                MaximumCacheSizeInMB = maximumCacheSizeInMB,
                AllowLocalhostConnectionsOnlyForHttp = allowLocalhostConnectionsOnlyForHttp,
                SSLCertificateSubjectName = certSubjectName,
                SSLCertificateStoreName = StoreName.My
            });            
        }        
        public HttpServer(CancellationToken cancellationToken, HttpServerConfig httpServerConfig) : base(httpServerConfig)
        {
            CommonConstructor(cancellationToken, httpServerConfig);
        }
        public HttpServer(HttpServerConfig httpServerConfig) : base(httpServerConfig)
        {
            CommonConstructor((new CancellationTokenSource()).Token, httpServerConfig);
        }
        public HttpServer(CancellationToken cancellationToken, UtilityConfig utilityConfig) : base(new StaticFileProcessorConfig() { WWWDirectory = null, EnableCache = false })
        {
            CommonConstructor(cancellationToken, new HttpServerConfig(utilityConfig));
        }
        public HttpServer(UtilityConfig utilityConfig) : base(new StaticFileProcessorConfig() { WWWDirectory = null, EnableCache = false })
        {
            CommonConstructor((new CancellationTokenSource()).Token, new HttpServerConfig(utilityConfig));
        }
        private void CommonConstructor(CancellationToken cancellationToken, HttpServerConfig httpServerConfig)
        {
            if (httpServerConfig.WWWDirectory.Length < 1) throw new Exception("path cannot be empty!");
            if (httpServerConfig.WWWDirectory.EndsWith(Path.DirectorySeparatorChar)) httpServerConfig.WWWDirectory = httpServerConfig.WWWDirectory.Substring(0, httpServerConfig.WWWDirectory.Length - 1);
            this.CancellationToken = cancellationToken;
            this.Config = httpServerConfig;
        }
        public async Task<bool> StartAsync()
        {
            this.OnInfoLog?.Invoke("Starting Vlix Web Server...");
            
            _listener = new HttpListener();
            
            if (this.Config.EnableHTTP) _listener.Prefixes.Add("http://*:" + this.Config.HTTPPort.ToString() + "/");
            if (this.Config.EnableHTTPS)
            {
                if (this.Config.EnableHTTP && (this.Config.HTTPPort == this.Config.HTTPSPort))
                {
                    this.OnErrorLog?.Invoke("Failed to start HTTPS Web Server (HTTPS Port cannot be the same as HTTP Port)!");
                    throw new Exception("Failed to start HTTPS Web Server (HTTPS Port cannot be the same as HTTP Port)!");
                }
                if (!await SSLCertificateServices.TryFindAndBindLatestSSLCertToPort(this.Config.HTTPSPort, this.Config.SSLCertificateSubjectName, this.Config.SSLCertificateStoreName, (log) => this.OnInfoLog?.Invoke(log), (log) => this.OnErrorLog?.Invoke(log)).ConfigureAwait(false))
                {
                    this.OnErrorLog?.Invoke("Failed to Start Web Server (Unable to bind SSL Cert to Port)!");
                    throw new Exception("Failed to Start Web Server (Unable to bind SSL Cert to Port)!");
                };
                _listener.Prefixes.Add("https://*:" + this.Config.HTTPSPort.ToString() + "/");
                _ = Task.Run(async () =>
                {
                    while (true)
                    {                        
                        await Task.Delay(60000 * 5).ConfigureAwait(false);
                        if (!await SSLCertificateServices.TryFindAndBindLatestSSLCertToPort(this.Config.HTTPSPort, this.Config.SSLCertificateSubjectName, this.Config.SSLCertificateStoreName, (log) => this.OnInfoLog?.Invoke(log), (log) => this.OnErrorLog?.Invoke(log)).ConfigureAwait(false))
                        {
                            this.OnErrorLog?.Invoke("Failed to Start Web Server (Unable to bind SSL Cert to Port)!");
                            throw new Exception("Failed to Start Web Server (Unable to bind SSL Cert to Port)!");
                        };
                    }
                });

            }
            if (!this.Config.EnableHTTP && !this.Config.EnableHTTPS)
            {
                this.OnInfoLog?.Invoke("Unable to start as both HTTP (Port " + this.Config.HTTPPort + ") and HTTPS (Port " + this.Config.HTTPSPort + ") is disabled");
                return false;
            }
            if (this.Config.EnableHTTP && !this.Config.EnableHTTPS) this.OnInfoLog?.Invoke("Listening to port " + this.Config.HTTPPort + "(HTTP), Directory = '" + this.Config.WWWDirectoryParsed() + "'");
            if (!this.Config.EnableHTTP && this.Config.EnableHTTPS) this.OnInfoLog?.Invoke("Listening to port " + this.Config.HTTPSPort + "(HTTPS), Directory = '" + this.Config.WWWDirectoryParsed() + "'");
            if (this.Config.EnableHTTP && this.Config.EnableHTTPS) this.OnInfoLog?.Invoke("Listening to port " + this.Config.HTTPPort + "(HTTP) and " + this.Config.HTTPSPort + "(HTTPS), Directory = '" + this.Config.WWWDirectoryParsed() + "'");

            _listener.Start();
            _listener.BeginGetContext(OnContext, null); //The thread stops here waiting for content to come

            this.OnInfoLog?.Invoke("Vlix HTTP Server Started!");
            return true;
        }

        private async void OnContext(IAsyncResult result)
        {
            if (!_listener.IsListening) return;
            HttpListenerContext context = null;

            try
            {
                context = _listener.EndGetContext(result);
                _listener.BeginGetContext(OnContext, null);

                string httpMethod = context.Request.HttpMethod;
                string callerIP = context.Request.RemoteEndPoint.Address.ToString();
                string schemeStr = context.Request.Url.Scheme;
                Scheme scheme; if (string.Equals(schemeStr, "https", StringComparison.OrdinalIgnoreCase)) scheme = Scheme.https; else scheme = Scheme.http;
                string host = context.Request.Url.Host;
                int port = context.Request.Url.Port;
                string pathAndQuery = context.Request.Url.PathAndQuery;
                string absolutePath = context.Request.Url.AbsolutePath;
                string absoluteURL = context.Request.Url.AbsoluteUri;
                if (string.IsNullOrWhiteSpace(absolutePath)) absolutePath = "/";
                if (scheme == Scheme.http && this.Config.AllowLocalhostConnectionsOnlyForHttp)
                {
                    if (!context.Request.IsLocal)
                    {
                        string msg = callerIP + " requested '" + absoluteURL + "' > Blocked as caller is not local ('AllowLocalhostConnectionsOnlyForHttp' is set to 'true')";
                        this.OnWarningLog?.Invoke(msg);
                        await SendErrorResponsePage(context, msg, HttpStatusCode.Unauthorized).ConfigureAwait(false);
                        return;
                    }
                }

                //Process Rules
                if (this.Config.Rules != null)
                {
                    foreach (Rule rule in this.Config.Rules)
                    {                        
                        if (rule.IsMatch(scheme, host, port, absolutePath))
                        {                            
                            ProcessRuleResult processRuleResult = await rule.ResponseAction.ProcessAsync(callerIP,scheme,host, port,absolutePath, pathAndQuery, context.Request.QueryString, context.Request.Headers, this).ConfigureAwait(false);
                            switch (processRuleResult.ActionType)
                            {
                                case ActionType.AlternativeWWWDirectory:
                                    if (processRuleResult.IsSuccess) await this.SendToOutputAsync(processRuleResult.HttpStreamResult, context).ConfigureAwait(false);
                                    break;
                                case ActionType.Redirect:
                                    context.Response.Redirect(processRuleResult.RedirectURL);
                                    break;
                                case ActionType.Deny:
                                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                    //context.Response?.Close();
                                    break;
                                case ActionType.ReverseProxy:
                                    if (processRuleResult.IsSuccess) await this.SendToOutputAsync(processRuleResult.HttpStreamResult, context).ConfigureAwait(false);
                                    break;
                            }
                            
                            string msg = null; if (!string.IsNullOrWhiteSpace(processRuleResult.Message)) msg = processRuleResult.Message;
                            if (processRuleResult.LogLevel == LogLevel.Info) this.OnInfoLog?.Invoke(callerIP + " requested '" + absoluteURL + "' > " + rule.Name + " > " + msg);                                    
                            else if (processRuleResult.LogLevel == LogLevel.Warning) this.OnWarningLog?.Invoke(callerIP + " requested '" + absoluteURL + "' > " + rule.Name + " > " + msg);
                            else if (processRuleResult.LogLevel == LogLevel.Error) this.OnErrorLog?.Invoke(callerIP + " requested '" + absoluteURL + "' > " + rule.Name + " > " + msg);
                            if (!processRuleResult.IsSuccess) { await this.SendErrorResponsePage(context, processRuleResult.Message, processRuleResult.SendErrorResponsePage_HttpStatusCode).ConfigureAwait(false); return; }
                            if (!processRuleResult.ContinueNextRule) { context.Response?.Close(); return; }
                        }
                    }
                }

                //Process Embedded Web API Paths                
                if (this.WebAPIs != null)
                {                                 
                    foreach (WebAPIAction action in this.WebAPIs)
                    {
                        if ((httpMethod==null || string.Equals(action.HttpMethod, httpMethod, StringComparison.InvariantCultureIgnoreCase)) && string.Equals(action.Path,absolutePath,StringComparison.InvariantCultureIgnoreCase))
                        {
                            string body = null;
                            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding)) body = await reader.ReadToEndAsync().ConfigureAwait(false);
                            string query = WebUtility.UrlDecode(context.Request.Url.Query);
                            WebAPIActionInput webAPIActionInput = new WebAPIActionInput(httpMethod, query, body, action, context, this.OnErrorLog);
                            var actionResult = action.Action?.Invoke(webAPIActionInput);
                            if (actionResult.Exception != null) throw actionResult.Exception;
                            return;
                        }
                    }
                }


                //Process Static Files
                HTTPStreamResult httpStreamResult = await this.ProcessRequestAsync(callerIP, absolutePath, this.Config.WWWDirectoryParsed()).ConfigureAwait(false);
                if (httpStreamResult.HttpStatusCode == HttpStatusCode.OK)
                {
                    await this.SendToOutputAsync(httpStreamResult, context).ConfigureAwait(false);
                    this.OnInfoLog?.Invoke(callerIP + " requested '" + absoluteURL + "' > Served '" + httpStreamResult.FileToRead + "'");
                }
                else
                {
                    if (httpStreamResult.HttpStatusCode == HttpStatusCode.NotFound && !httpStreamResult.FaviconError)
                        this.OnWarningLog?.Invoke(callerIP + " requested '" + absoluteURL + "' > '" + httpStreamResult.FileToRead + "' does not exist. Returning NOT FOUND");
                    if (httpStreamResult.HttpStatusCode == HttpStatusCode.InternalServerError) 
                        this.OnErrorLog?.Invoke(httpStreamResult.ErrorMsg);
                    await SendErrorResponsePage(context, httpStreamResult.ErrorMsg, httpStreamResult.HttpStatusCode).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                var exStr = ex.ToString();
                try
                {
                    if (context != null) await SendErrorResponsePage(context, exStr, HttpStatusCode.InternalServerError).ConfigureAwait(false);
                }
                catch { }
                this.OnErrorLog?.Invoke(exStr);
            }
        }

        internal async Task SendToOutputAsync(HTTPStreamResult httpStreamResult, HttpListenerContext context)
        {
            context.Response.ContentLength64 = httpStreamResult.Stream.Length;
            context.Response.ContentType = httpStreamResult.ContentType;
            context.Response.ContentEncoding = Encoding.UTF8;
            //context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
            //if (!String.IsNullOrEmpty(httpStreamResult?.FileToRead)) context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(httpStreamResult.FileToRead).ToString("r"));

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await httpStreamResult.Stream.CopyToAsync(context.Response.OutputStream).ConfigureAwait(false);
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
                await context.Response.OutputStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            }
            finally
            {
                context.Response?.Close();
            }
        }
        public void Stop() { this.OnInfoLog?.Invoke("Stopping Vlix HTTP Server...");  this.Shutdown(); this.HttpClient.Dispose(); _listener?.Stop(); this.OnInfoLog?.Invoke("Vlix HTTP Server Stopped!"); }
        public void AddLog(string log, LogLevel logLevel = LogLevel.Info)
        {
            switch (logLevel)
            {
                case LogLevel.Info:
                    this.OnInfoLog(log);
                    break;
                case LogLevel.Warning:
                    this.OnWarningLog(log);
                    break;
                case LogLevel.Error:
                    this.OnErrorLog(log);
                    break;
            }
            
        }
    }





}