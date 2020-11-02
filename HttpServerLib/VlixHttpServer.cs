﻿using System;
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
        public int Port { get; internal set; }
        public bool EnableCache { get; internal set; }
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
                        foreach (var cache in this) if (DateTime.Now > cache.Value.ExpiryTime) keysToRemove.Add(cache.Key);
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
            public HttpCache(MemoryStream memoryStream,DateTime expiryTime) { this.MemoryStream = memoryStream; this.ExpiryTime = expiryTime;  }
            public DateTime ExpiryTime { get; set; }
            public MemoryStream MemoryStream { get; set; }
            
        }


        public bool TryParseAbsolutePath(string absolutePath, out string fileToRead, out string fileToReadDir, out string errorMsg)
        {
            errorMsg = null;
            absolutePath = absolutePath.TrimEnd();
            if (Regex.IsMatch(absolutePath, "^\\s+/")) absolutePath = absolutePath.TrimStart();
            //if (!absolutePath.StartsWith("/")) absolutePath =  "/" + absolutePath;
            if (!absolutePath.StartsWith("/")) absolutePath =  "/" + absolutePath;
            if (absolutePath.Contains(".."))
            {
                errorMsg = "ILLEGAL CHARACTER '..'. Returning NOT FOUND.";
                fileToRead = null;
                fileToReadDir = null;
                return false;
            }
            string lastURLPortion = absolutePath.Split('/').Last(); //this gives=> "afile.html",          "",   "afolder"
            var sss = lastURLPortion.Contains(".");
            if (!lastURLPortion.Contains(".")) //handles "/afolder"
            {
                //absolutePath = absolutePath + "/"; // "/afolder" ==> "/afolder/"
                fileToRead = "";
                string Temp = absolutePath.Replace('/', '\\');
                //fileToReadDir = this.Path + absolutePath.Replace('/', '\\');
                fileToReadDir = this.Path + System.IO.Path.GetDirectoryName(Temp + "\\");
                if (fileToReadDir.EndsWith("\\")) fileToReadDir = fileToReadDir.Substring(0, fileToReadDir.Length - 1);
                //fileToReadDir = this.Path + absolutePath.Replace('/', '\\');
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

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public VlixHttpServer(string path, int port = 8080, bool enableCache = true)
        {
            if (path.Length < 1) throw new Exception("path cannot be empty!");
            if (path.Substring(path.Length-1,1)=="\\") path = path.Substring(0, path.Length - 1);
            this.Path = path;
            this.Port = port;
            this.EnableCache = enableCache;
            _serverThread = new Thread(() => {                
                (_listener = new HttpListener()).Prefixes.Add("http://*:" + this.Port.ToString() + "/");
                _listener.Start();
                this.OnInfoLog?.Invoke("Listening to port " + this.Port + ", Directory = '" + this.Path + "'");
                
                while (true)
                {
                    HttpListenerContext newContext = _listener.GetContext(); //The thread stops here waiting for content to come
                    Task.Run(() => Process(newContext, cancellationTokenSource.Token)); //.WithCancellation(cancellationTokenSource.Token);
                }
            });
            //_serverThread.SetApartmentState(ApartmentState.STA);
        }

        public void Process(HttpListenerContext context, CancellationToken cancellationToken)
        {
            try
            {
                bool fileExists = false; //this flag is to avoid checking file exists twice
                if (context == null) return;
                string callerIP = context.Request.RemoteEndPoint.ToString();
                string absolutePath = context.Request.Url.AbsolutePath; //this gives=> "/afolder/afile.html", "/",  "/afolder" 

                if (!this.TryParseAbsolutePath(absolutePath, out string fileToRead, out string fileToReadDir, out string ErrorMsg))
                {
                    this.OnWarningLog?.Invoke(callerIP + " requested '" + absolutePath + "'. " + ErrorMsg);
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound; return;// Task.CompletedTask;
                }

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
                        this.OnWarningLog?.Invoke(callerIP + " requested '" + absolutePath + "'. File to read => '" + fileToRead + "' does not exist. Returning NOT FOUND");
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound; return;// Task.CompletedTask;
                    }
                }
                this.OnInfoLog?.Invoke(callerIP + " requested '" + absolutePath + "'. File to read => '" + fileToRead + "'");
                if (string.IsNullOrEmpty(fileToRead)) { context.Response.StatusCode = (int)HttpStatusCode.NotFound; return; };// Task.CompletedTask; }
                if (!fileExists) fileExists = File.Exists(fileToRead);
                if (fileExists)
                {
                    try
                    {
                        if (this.EnableCache)
                        {
                            context.Response.ContentType = _mimeTypeMappings.TryGetValue(System.IO.Path.GetExtension(fileToRead), out string mime) ? mime : "application/octet-stream";
                            context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                            context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(fileToRead).ToString("r"));
                            if (!this.cacheFiles.TryGetValue(fileToRead, out HttpCache httpCache))
                            {
                                httpCache = new HttpCache(new MemoryStream(), DateTime.Now.AddMinutes(1));
                                using (Stream input = new FileStream(fileToRead, FileMode.Open))
                                {
                                    context.Response.ContentLength64 = input.Length;
                                    byte[] buffer = new byte[4096]; //byte[] buffer = new byte[1024 * 32]; //4096 faster than 32K
                                    int nbytes;
                                    while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        if (cancellationToken.IsCancellationRequested) return;
                                        httpCache.MemoryStream.Write(buffer, 0, nbytes);
                                    }
                                    this.cacheFiles.TryAdd(fileToRead, httpCache);
                                }
                            }
                            httpCache.MemoryStream.Position = 0;
                            httpCache.MemoryStream.CopyTo(context.Response.OutputStream);
                        }
                        else
                        {
                            using (Stream input = new FileStream(fileToRead, FileMode.Open))
                            {
                                string mime;
                                context.Response.ContentType = _mimeTypeMappings.TryGetValue(System.IO.Path.GetExtension(fileToRead), out mime) ? mime : "application/octet-stream";
                                context.Response.ContentLength64 = input.Length;
                                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                                context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(fileToRead).ToString("r"));
                                byte[] buffer = new byte[4096]; //byte[] buffer = new byte[1024 * 32]; //4096 faster than 32K
                                int nbytes;
                                while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    if (cancellationToken.IsCancellationRequested) return;
                                    context.Response.OutputStream.Write(buffer, 0, nbytes);
                                }
                            }
                        }
                        context.Response.OutputStream.Flush();
                        context.Response.StatusCode = (int)HttpStatusCode.OK;   
                    }
                    catch(Exception ex) {  context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; }
                }
                else context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            catch (Exception Ex) { this.OnError?.Invoke(Ex); }
            finally 
            { 
                context?.Response?.OutputStream?.Close();
            }
            return;// Task.CompletedTask;
        }



        public void Start() { this.OnInfoLog?.Invoke("Starting Vlix Http Server..."); _serverThread.Start(); this.OnInfoLog?.Invoke("Vlix Http Server Started!"); }
        public void Stop() { this.OnInfoLog?.Invoke("Stopping Vlix Http Server..."); cancellationTokenSource.Cancel(); _serverThread.Abort(); _listener.Stop(); cacheFiles.Dispose(); this.OnInfoLog?.Invoke("Vlix Http Server Stopped!"); }

    }





}