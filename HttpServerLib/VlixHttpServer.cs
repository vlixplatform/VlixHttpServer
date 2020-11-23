using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Vlix
{
    public class VlixHttpServer
    {
        public int HttpPort { get; internal set; }
        public int HttpsPort { get; internal set; }
        public bool EnableCache { get; internal set; }
        public int OnlyCacheItemsLessThenMB { get; internal set; }
        public int MaximumCacheSizeInMB { get; internal set; }
        public CancellationToken CancellationToken { get; internal set; }
        public string Path { get; private set; }
        public string[] DefaultDocuments = { "index.html", "index.htm", "default.html", "default.htm" };
        private CacheFiles cacheFiles = new CacheFiles();

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
        public Action<Exception> OnError { get; set; }
        public Action<string> OnWarningLog { get; set; }
        public Action<string> OnInfoLog { get; set; }
        class CacheFiles: ConcurrentDictionary<string, HttpCache>, IDisposable
        {
            Thread clearCache;
            public CacheFiles()
            {
                (clearCache = new Thread(async () =>
                {
                    while (true)
                    {
                        List<string> keysToRemove = new List<string>();
                        foreach (var cache in this) if (DateTime.Now > cache.Value.ExpiryTimeUTC) keysToRemove.Add(cache.Key);
                        foreach (string key in keysToRemove) this.TryRemove(key, out _);
                        await Task.Delay(3000);
                    }
                })).Start(); 
            }

            public void Dispose()
            {
                clearCache.Abort();
                clearCache = null;
                this.Clear();
            }
        }        
        class HttpCache
        {
            public HttpCache(MemoryStream memoryStream,DateTime lastModifiedTimeUTC, double contentLengthInMB) 
            { 
                this.MemoryStream = memoryStream;
                this.LastModifiedTimeUTC = lastModifiedTimeUTC;
                this.ExpiryTimeUTC = DateTime.UtcNow.AddDays(1);
                this.ContentLengthInMB = contentLengthInMB;  
            }

            public DateTime LastModifiedTimeUTC { get; set; }
            public DateTime ExpiryTimeUTC { get; private set; }
            public double ContentLengthInMB { get; set; }
            public MemoryStream MemoryStream { get; set; }
            
        }


        public bool TryParseAbsolutePath(string absolutePath, out string fileToRead, out string fileToReadDir, out string errorMsg)
        {
            errorMsg = null;
            absolutePath = absolutePath.TrimEnd();
            if (Regex.IsMatch(absolutePath, "^\\s+/")) absolutePath = absolutePath.TrimStart();
            if (!absolutePath.StartsWith("/")) absolutePath =  "/" + absolutePath;
            if (absolutePath.Contains(".."))
            {
                errorMsg = "ILLEGAL CHARACTER '..'. Returning NOT FOUND.";
                fileToRead = null;
                fileToReadDir = null;
                return false;
            }
            string lastURLPortion = absolutePath.Split('/').Last();
            var sss = lastURLPortion.Contains(".");
            if (!lastURLPortion.Contains("."))
            {
                fileToRead = "";
                string Temp = absolutePath.Replace('/', '\\');
                fileToReadDir = this.Path + System.IO.Path.GetDirectoryName(Temp + "\\");
                if (fileToReadDir.EndsWith("\\")) fileToReadDir = fileToReadDir.Substring(0, fileToReadDir.Length - 1);
            }
            else
            {
                string Temp = absolutePath.Replace('/', '\\');
                fileToReadDir = this.Path + System.IO.Path.GetDirectoryName(Temp);
                if (fileToReadDir.EndsWith("\\")) fileToReadDir = fileToReadDir.Substring(0, fileToReadDir.Length - 1);
                fileToRead = fileToReadDir + "\\" + System.IO.Path.GetFileName(Temp);                                
            }
            return true;
        }

        public VlixHttpServer(CancellationToken cancellationToken, string path, int httpPort = 80, int httpsPort = 443, bool enableCache = true, int onlyCacheItemsLessThenMB=10, int maximumCacheSizeInMB = 500)
        {
            if (path.Length < 1) throw new Exception("path cannot be empty!");
            if (path.Substring(path.Length-1,1)=="\\") path = path.Substring(0, path.Length - 1);
            this.Path = path;
            this.HttpPort = httpPort;
            this.CancellationToken = cancellationToken;
            this.HttpsPort = httpsPort;
            this.EnableCache = enableCache;
            this.OnlyCacheItemsLessThenMB = onlyCacheItemsLessThenMB;
            this.MaximumCacheSizeInMB = maximumCacheSizeInMB;
            _serverThread = new Thread(() => {                
                (_listener = new HttpListener()).Prefixes.Add("http://*:" + this.HttpPort.ToString() + "/");
                _listener.Prefixes.Add("https://*:" + this.HttpsPort.ToString() + "/");
                _listener.Start();
                this.OnInfoLog?.Invoke("Listening to port " + this.HttpPort + "(Http) and " + this.HttpsPort + "(Https), Directory = '" + this.Path + "'");
                
                while (true)
                {
                    HttpListenerContext newContext = _listener.GetContext(); //The thread stops here waiting for content to come
                    Task.Run(async () => 
                    {
                        await Process(newContext);
                    });
                }
            });
        }


        public class HttpStreamResult
        {
            public HttpStreamResult(HttpStatusCode httpStatusCode, string contentType, MemoryStream memoryStream, string errorMsg)
            {
                this.HttpStatusCode = httpStatusCode;
                this.ContentType = contentType;
                this.MemoryStream = memoryStream;
                this.ErrorMsg = errorMsg;
            }
            public MemoryStream MemoryStream { get; set; } = null;
            public string ContentType { get; set; } = null;

            public string ErrorMsg { get; set; } = null;
            public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.OK;
        }
        public async Task Process(HttpListenerContext context)
        {
            try
            {
                
                if (context == null) return;
                string callerIP = context.Request.RemoteEndPoint.ToString();
                string absolutePath = context.Request.Url.AbsolutePath; //this gives=> "/afolder/afile.html", "/",  "/afolder" 

                if (!this.TryParseAbsolutePath(absolutePath, out string fileToRead, out string fileToReadDir, out string ErrorMsg))
                {
                    this.OnWarningLog?.Invoke(callerIP + " requested '" + absolutePath + "'. " + ErrorMsg);
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound; return;// Task.CompletedTask;
                }


                //Find a FileToRead if a directory is requested. Find standard files "index.html", "index.htm", "default.html", "default.htm"
                bool fileExists = false;
                if (string.IsNullOrEmpty(fileToRead))
                {
                    bool found = false;
                    foreach (string indexFile in DefaultDocuments)
                    {
                        if (File.Exists(fileToReadDir + "\\" + indexFile))
                        {
                            fileExists = true;
                            fileToRead = fileToReadDir + "\\" + indexFile;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        this.OnWarningLog?.Invoke(callerIP + " requested '" + absolutePath + "'. File to read '" + fileToRead + "' does not exist. Returning NOT FOUND");
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound; return;
                    }
                }
                this.OnInfoLog?.Invoke(callerIP + " requested '" + absolutePath + "'. File to read => '" + fileToRead + "'");
                try
                {
                    if (string.IsNullOrEmpty(fileToRead)) { context.Response.StatusCode = (int)HttpStatusCode.NotFound; return; };
                    if (!fileExists) fileExists = File.Exists(fileToRead);
                    if (!fileExists) { context.Response.StatusCode = (int)HttpStatusCode.NotFound; return; }
                    DateTime fileLastModifiedTimeUTC = DateTime.MinValue;
                    if (fileExists) fileLastModifiedTimeUTC = File.GetLastWriteTimeUtc(fileToRead).ToUniversalTime();
                    if (this.EnableCache)
                    {
                        if (!this.cacheFiles.TryGetValue(fileToRead, out HttpCache httpCache) || httpCache.LastModifiedTimeUTC != fileLastModifiedTimeUTC)
                        {
                            var ms = new MemoryStream();
                            using (Stream input = new FileStream(fileToRead, FileMode.Open))
                            {
                                byte[] buffer = new byte[4096];
                                int nbytes;
                                while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    if (this.CancellationToken.IsCancellationRequested)
                                    {
                                        this.OnWarningLog?.Invoke(callerIP + " requested '" + absolutePath + "' was cancelled by server. File to read:  '" + fileToRead + "'.");
                                        return;
                                    }
                                    await ms.WriteAsync(buffer, 0, nbytes);
                                }
                                httpCache = new HttpCache(ms, DateTime.UtcNow, ms.Length / 1024 / 1024);
                                if (httpCache.ContentLengthInMB <= this.OnlyCacheItemsLessThenMB) this.cacheFiles.TryAdd(fileToRead, httpCache);
                            }
                        }
                        //context.Response.ContentLength64 = httpCache.MemoryStream.Length;
                        httpCache.MemoryStream.Position = 0;
                        httpCache.MemoryStream.CopyTo(context.Response.OutputStream);
                    }
                    else
                    {
                        using (Stream input = new FileStream(fileToRead, FileMode.Open))
                        {
                           // context.Response.ContentLength64 = input.Length;
                            byte[] buffer = new byte[4096];
                            int nbytes;
                            while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                if (this.CancellationToken.IsCancellationRequested) return;
                                context.Response.OutputStream.Write(buffer, 0, nbytes);
                            }
                        }
                    }
                    context.Response.ContentLength64 = context.Response.OutputStream.Length;
                    context.Response.ContentType = _mimeTypeMappings.TryGetValue(System.IO.Path.GetExtension(fileToRead), out string mime) ? mime : "application/octet-stream";
                    context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(fileToRead).ToString("r"));
                    context.Response.OutputStream.Flush();
                    context.Response.StatusCode = (int)HttpStatusCode.OK;   
                }
                catch {  context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; }

            }
            catch (Exception Ex) { this.OnError?.Invoke(Ex); }
            finally 
            { 
                context?.Response?.OutputStream?.Close();
            }
            return;
        }



        public void Start() { this.OnInfoLog?.Invoke("Starting Vlix Http Server..."); _serverThread.Start(); this.OnInfoLog?.Invoke("Vlix Http Server Started!"); }
        public void Stop() { this.OnInfoLog?.Invoke("Stopping Vlix Http Server..."); _serverThread.Abort(); _listener.Stop(); cacheFiles.Dispose(); this.OnInfoLog?.Invoke("Vlix Http Server Stopped!"); }

    }





}