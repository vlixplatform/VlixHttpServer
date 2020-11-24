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
    public class VlixHttpServer: StaticFileHTTPServer
    {
        public bool EnableHTTP { get; internal set; }
        public int HTTPPort { get; internal set; }
        public bool EnableHTTPS { get; internal set; }
        public int HTTPSPort { get; internal set; }
        public bool AllowLocalhostConnectionsOnly { get; set; } = false;
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

        private Thread _serverThread;
        private HttpListener _listener;
        public Action<string> OnErrorLog { get; set; }
        public Action<string> OnWarningLog { get; set; }
        public Action<string> OnInfoLog { get; set; }
        public Action<HTTPStreamResult> OnHTTPStreamResult { get; set; }

        public VlixHttpServer(string path, int httpPort = 80, int httpsPort = 443, bool enableCache = true, int onlyCacheItemsLessThenMB = 10, int maximumCacheSizeInMB = 500,
            bool allowLocalhostConnectionsOnly = false) : base(path, enableCache, onlyCacheItemsLessThenMB, maximumCacheSizeInMB)
        {
            CommonConstructor((new CancellationTokenSource()).Token, path, true, httpPort, true, httpsPort, enableCache, onlyCacheItemsLessThenMB, maximumCacheSizeInMB, allowLocalhostConnectionsOnly);
        }

        public VlixHttpServer(CancellationToken cancellationToken, string path, int httpPort = 80, int httpsPort = 443, bool enableCache = true, int onlyCacheItemsLessThenMB = 10, 
            int maximumCacheSizeInMB = 500, bool allowLocalhostConnectionsOnly = false) : base(path, enableCache, onlyCacheItemsLessThenMB, maximumCacheSizeInMB)
        {
            CommonConstructor(cancellationToken, path, true, httpPort, true, httpsPort, enableCache, onlyCacheItemsLessThenMB, maximumCacheSizeInMB, allowLocalhostConnectionsOnly);
        }
        public VlixHttpServer(CancellationToken cancellationToken, HttpServerConfig httpServerConfig) : base(httpServerConfig.WWWDirectory, httpServerConfig.EnableCache, httpServerConfig.OnlyCacheItemsLessThenMB, httpServerConfig.MaximumCacheSizeInMB)
        {
            CommonConstructor(cancellationToken, httpServerConfig.WWWDirectory, httpServerConfig.EnableHTTP, httpServerConfig.HTTPPort, httpServerConfig.EnableHTTPS, httpServerConfig.HTTPSPort, httpServerConfig.EnableCache, httpServerConfig.OnlyCacheItemsLessThenMB, httpServerConfig.MaximumCacheSizeInMB, httpServerConfig.AllowLocalhostConnectionsOnly);
        }

        public VlixHttpServer(HttpServerConfig httpServerConfig) : base(httpServerConfig.WWWDirectory, httpServerConfig.EnableCache, httpServerConfig.OnlyCacheItemsLessThenMB, httpServerConfig.MaximumCacheSizeInMB)
        {
            CommonConstructor((new CancellationTokenSource()).Token, httpServerConfig.WWWDirectory, httpServerConfig.EnableHTTP, httpServerConfig.HTTPPort, httpServerConfig.EnableHTTPS, httpServerConfig.HTTPSPort, httpServerConfig.EnableCache, httpServerConfig.OnlyCacheItemsLessThenMB, httpServerConfig.MaximumCacheSizeInMB, httpServerConfig.AllowLocalhostConnectionsOnly);
        }

        private void CommonConstructor(CancellationToken cancellationToken, string path, bool EnableHTTP = true, int httpPort = 80, bool EnableHTTPS = true, int httpsPort = 443, bool enableCache = true, int onlyCacheItemsLessThenMB=10, 
            int maximumCacheSizeInMB = 500, bool allowLocalhostConnectionsOnly = false)
        {
            if (path.Length < 1) throw new Exception("path cannot be empty!");
            if (path.Substring(path.Length-1,1)=="\\") path = path.Substring(0, path.Length - 1);
            this.CancellationToken = cancellationToken;
            this.EnableHTTP = EnableHTTP;
            this.HTTPPort = httpPort;
            this.EnableHTTPS = EnableHTTPS;
            this.HTTPSPort = httpsPort;
            this.AllowLocalhostConnectionsOnly = allowLocalhostConnectionsOnly;
            _serverThread = new Thread(async () => {
                _listener = new HttpListener();
                if (this.EnableHTTP) _listener.Prefixes.Add("http://*:" + this.HTTPPort.ToString() + "/");
                if (this.EnableHTTPS) _listener.Prefixes.Add("https://*:" + this.HTTPSPort.ToString() + "/");
                _listener.Start();
                this.OnInfoLog?.Invoke("Listening to port " + this.HTTPPort + "(HTTP) and " + this.HTTPSPort + "(HTTPS), Directory = '" + this.WWWDirectory + "'");
                
                while (true)
                {
                    if (!this.EnableHTTP && !this.EnableHTTPS)
                    {
                        this.OnInfoLog?.Invoke("Both HTTP (Port " + this.HTTPPort + ") and HTTPS (Port " + this.HTTPSPort + ") is disabled");
                        await Task.Delay(3000);
                        continue;
                    }
                    HttpListenerContext context = _listener.GetContext(); //The thread stops here waiting for content to come
                    _ = Task.Run(async () => 
                    {
                        try
                        {
                            string callerIP = context.Request.RemoteEndPoint.ToString();
                            string absolutePath = context.Request.Url.AbsolutePath;
                            if (this.AllowLocalhostConnectionsOnly)
                            {
                                if (!context.Request.IsLocal)
                                {
                                    string msg = callerIP + " requested '" + absolutePath + "'. Blocked as caller is not local ('AllowLocalhostConnectionsOnly' is set to 'true')";
                                    this.OnWarningLog?.Invoke(msg);
                                    SendErrorResponse(context, msg, HttpStatusCode.Unauthorized);
                                    return;
                                }
                            }

                            HTTPStreamResult httpStreamResult = await this.ProcessRequest(callerIP, absolutePath);
                            this.OnHTTPStreamResult?.Invoke(httpStreamResult);
                            if (httpStreamResult.HttpStatusCode == HttpStatusCode.OK)
                            {                                
                                context.Response.ContentLength64 = httpStreamResult.MemoryStream.Length;
                                context.Response.ContentType = _mimeTypeMappings.TryGetValue(System.IO.Path.GetExtension(httpStreamResult.FileToRead), out string mime) ? mime : "application/octet-stream";
                                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                                if (!String.IsNullOrEmpty(httpStreamResult?.FileToRead)) context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(httpStreamResult.FileToRead).ToString("r"));
                                context.Response.StatusCode = (int)HttpStatusCode.OK;                                
                                await httpStreamResult.MemoryStream.CopyToAsync(context.Response.OutputStream);
                                this.OnInfoLog?.Invoke(callerIP + " requested '" + absolutePath + "'. File to read '" + httpStreamResult.FileToRead + "'");
                            }
                            else
                            {
                                if (httpStreamResult.HttpStatusCode == HttpStatusCode.NotFound)
                                {
                                    if (!httpStreamResult.FaviconError) this.OnWarningLog?.Invoke(callerIP + " requested '" + absolutePath + "'. File to read '" + httpStreamResult.FileToRead + "' does not exist. Returning NOT FOUND");
                                }
                                if (httpStreamResult.HttpStatusCode == HttpStatusCode.InternalServerError)
                                {
                                    this.OnErrorLog?.Invoke(httpStreamResult.ErrorMsg);
                                }                                
                                SendErrorResponse(context, httpStreamResult.ErrorMsg, httpStreamResult.HttpStatusCode);
                            }
                        }
                        catch (Exception ex)
                        {
                            SendErrorResponse(context, ex.ToString(), HttpStatusCode.InternalServerError);
                            this.OnErrorLog?.Invoke(ex.ToString());
                        }
                    });
                }
            });
        }

        private async void SendErrorResponse(HttpListenerContext context, string errorMsg, HttpStatusCode httpStatusCode)
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

        public void Start() { this.OnInfoLog?.Invoke("Starting Vlix HTTP Server..."); _serverThread.Start(); this.OnInfoLog?.Invoke("Vlix HTTP Server Started!"); }
        public void Stop() { this.OnInfoLog?.Invoke("Stopping Vlix HTTP Server...");  this.Shutdown(); _listener?.Stop(); _serverThread?.Abort(); this.OnInfoLog?.Invoke("Vlix HTTP Server Stopped!"); }

    }





}