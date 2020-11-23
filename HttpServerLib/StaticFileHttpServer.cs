using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Vlix
{
    public class HTTPStreamResult
    {
        public HTTPStreamResult(HttpStatusCode httpStatusCode, string contentType, MemoryStream memoryStream, string errorMsg)
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
    public class StaticFileHTTPServer
    {
        public CancellationTokenSource cancellationTokenSource;
        public StaticFileHTTPServer(string path, bool enableCache = true, int onlyCacheItemsLessThenMB = 10, int maximumCacheSizeInMB = 500)
        {
            this.Path = path;
            this.EnableCache = enableCache;
            this.OnlyCacheItemsLessThenMB = onlyCacheItemsLessThenMB;
            this.MaximumCacheSizeInMB = maximumCacheSizeInMB;
            cancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    if (cancellationTokenSource.IsCancellationRequested) break;
                    await Task.Delay(1000);
                    if (cancellationTokenSource.IsCancellationRequested) break;
                    await Task.Delay(1000);
                    if (cancellationTokenSource.IsCancellationRequested) break;
                    if (this.EnableCache)
                    {
                        double totalCacheInKB = 0;
                        foreach (var cache in this.cacheFiles.Values) totalCacheInKB += cache.ContentLengthInKB;
                        if (totalCacheInKB > this.MaximumCacheSizeInMB * 1024)
                        {
                            var cacheFileToRemoveInOrder = this.cacheFiles.Values.OrderBy(h => h.RequestCount).ThenBy(h => h.LastAccessTimeUTC).Select(h => h.FileToRead).ToList();
                            foreach (string key in cacheFileToRemoveInOrder)
                            {
                                if (this.cacheFiles.Count == 0) break;
                                if (this.MaximumCacheSizeInMB <= 0) break;
                                if (this.cacheFiles.TryRemove(key, out HTTPCache hTTPCacheRemoved))
                                {
                                    totalCacheInKB -= hTTPCacheRemoved.ContentLengthInKB;
                                    if (totalCacheInKB < this.MaximumCacheSizeInMB) break;
                                }
                            }
                        }
                    }
                }
            });
        }
        public string Path { get; private set; }
        public bool EnableCache { get; set; } = true;
        public int OnlyCacheItemsLessThenMB { get; set; }
        public int MaximumCacheSizeInMB { get; set; }



        public void Shutdown() { cancellationTokenSource.Cancel(); }

        private ConcurrentDictionary<string, HTTPCache> cacheFiles = new ConcurrentDictionary<string, HTTPCache>();
        class HTTPCache
        {
            public HTTPCache(string fileToRead, MemoryStream memoryStream, DateTime lastModifiedTimeUTC, double contentLengthInKB) 
            {
                this.FileToRead = fileToRead;
                this.MemoryStream = memoryStream; 
                this.LastModifiedTimeUTC = lastModifiedTimeUTC;
                this.LastAccessTimeUTC = DateTime.UtcNow;
                this.RequestCount = 1;
                this.ContentLengthInKB = contentLengthInKB;
            }
            public string FileToRead { get; internal set; }
            public DateTime LastModifiedTimeUTC { get; set; }
            public DateTime LastAccessTimeUTC { get; set; }
            public int RequestCount { get; set; } = 0;
            public MemoryStream MemoryStream { get; set; }

            public double ContentLengthInKB { get; set; }

        }

        private readonly IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
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

        public readonly string[] DefaultDocuments = { "index.html", "index.htm", "default.html", "default.htm" };
        public async Task<HttpStreamResult> ProcessRequest(string callerIP, string absolutePath)
        {
            MemoryStream outputMemoryStream = null;            
            try
            {
                if (!this.TryParseAbsolutePath(absolutePath, out string fileToRead, out string fileToReadDir, out string ErrorMsg))
                {
                    throw new ServerWebFaultException(ErrorMsg, HttpStatusCode.NotFound);
                }

                //Find a FileToRead if a directory is requested. Find standard files "index.html", "index.htm", "default.html", "default.htm"
                bool fileExists = false;
                DateTime fileLastModifiedTimeUTC = DateTime.MinValue;
                if (string.IsNullOrEmpty(fileToRead))
                {
                    foreach (string indexFile in DefaultDocuments)
                    {
                        if (File.Exists(fileToReadDir + "\\" + indexFile))
                        {
                            fileToRead = fileToReadDir + "\\" + indexFile;
                            fileExists = true;
                            break;
                        }
                    }                    
                }

                try
                {
                    if (!fileExists) fileExists = File.Exists(fileToRead);
                    if (!fileExists) throw new ServerWebFaultException("File '" + fileToRead + "' does not exist!", HttpStatusCode.NotFound);
                    if (fileExists) fileLastModifiedTimeUTC = File.GetLastWriteTimeUtc(fileToRead).ToUniversalTime();


                    outputMemoryStream = new MemoryStream();
                    HTTPCache httpCache = null;
                    if (this.EnableCache)
                    {
                        if (this.cacheFiles.TryGetValue(fileToRead, out httpCache) && httpCache.LastModifiedTimeUTC == fileLastModifiedTimeUTC)
                        {
                            httpCache.RequestCount++;
                            httpCache.LastAccessTimeUTC = DateTime.UtcNow;
                            httpCache.MemoryStream.Position = 0;
                            await httpCache.MemoryStream.CopyToAsync(outputMemoryStream);
                            //outputMemoryStream = httpCache.MemoryStream;
                        }
                        else
                        {
                            using (Stream input = new FileStream(fileToRead, FileMode.Open))
                            {
                                byte[] buffer = new byte[4096]; //byte[] buffer = new byte[1024 * 32]; //4096 faster than 32K
                                int nbytes; MemoryStream ms = new MemoryStream();
                                while ((nbytes = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    if (cancellationTokenSource.IsCancellationRequested) throw new ServerWebFaultException("Request was cancelled by server while reading file '" + fileToRead + "'", HttpStatusCode.InternalServerError);
                                    await ms.WriteAsync(buffer, 0, nbytes);
                                }
                                httpCache = new HTTPCache(fileToRead, ms, fileLastModifiedTimeUTC, outputMemoryStream.Length / 1024);
                                if (httpCache.ContentLengthInKB <= (this.OnlyCacheItemsLessThenMB * 1024)) this.cacheFiles.TryAdd(fileToRead, httpCache);
                                ms.Position = 0;
                                await ms.CopyToAsync(outputMemoryStream);
                            }
                        }
                    }
                    else
                    {
                        using (Stream input = new FileStream(fileToRead, FileMode.Open))
                        {
                            byte[] buffer = new byte[4096];
                            int nbytes;
                            while ((nbytes = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                if (cancellationTokenSource.IsCancellationRequested) throw new ServerWebFaultException("Request was cancelled by server while reading file '" + fileToRead + "'", HttpStatusCode.InternalServerError);
                                await outputMemoryStream.WriteAsync(buffer, 0, nbytes);
                            }
                        }
                    }
                    string contentType = _mimeTypeMappings.TryGetValue(System.IO.Path.GetExtension(fileToRead), out string mime) ? mime : "application/octet-stream";
                    lock (Server.APICallsLock) Server.APICalls.Add(new ServerAPICall(DateTime.UtcNow));
                    //outputMemoryStream.Position = 0;
                    //await outputMemoryStream.FlushAsync();                    
                    return new HttpStreamResult(HttpStatusCode.OK, contentType, outputMemoryStream, null);
                }
                catch (Exception ex)
                {
                    return new HttpStreamResult(HttpStatusCode.InternalServerError, null, null, "Generic Error wile processing file '" + fileToRead + "'\r\n" + ex.ToString());
                }
            }
            catch (Exception ex2)
            {
                return new HttpStreamResult(HttpStatusCode.InternalServerError, null, null, "Generic Error: " + ex2.ToString());
            }
            //finally
            //{
            //    stream?.Close();
            //}
        }

        private bool TryParseAbsolutePath(string absolutePath, out string fileToRead, out string fileToReadDir, out string errorMsg)
        {
            errorMsg = null;
            absolutePath = absolutePath.TrimEnd();
            if (Regex.IsMatch(absolutePath, "^\\s+/")) absolutePath = absolutePath.TrimStart();
            if (!absolutePath.StartsWith("/")) absolutePath = "/" + absolutePath;
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
}
