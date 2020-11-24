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
                        if (this.CacheFiles.TotalCacheInKB > this.MaximumCacheSizeInMB * 1024)
                        {
                            var cacheFileToRemoveInOrder = this.CacheFiles.Values.OrderBy(h => h.LastAccessTimeUTC).ThenBy(h => h.RequestCount).Select(h => h.FileToRead).ToList();
                            foreach (string key in cacheFileToRemoveInOrder)
                            {
                                if (this.CacheFiles.Count == 0) break;
                                if (this.MaximumCacheSizeInMB <= 0) break;
                                if (this.CacheFiles.TryRemove(key, out HTTPCache hTTPCacheRemoved))
                                {
                                    if (this.CacheFiles.TotalCacheInKB < this.MaximumCacheSizeInMB * 1024) break;
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

        

        public CacheFiles CacheFiles = new CacheFiles();
        
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
        public async Task<HTTPStreamResult> ProcessRequest(string callerIP, string absolutePath)
        {
            string fileToRead = null;
            MemoryStream outputMemoryStream = null;
            try
            {
                if (!this.TryParseAbsolutePath(absolutePath, out fileToRead, out string fileToReadDir, out string ErrorMsg))
                {
                    return new HTTPStreamResult(false,fileToRead, HttpStatusCode.NotFound, null, null, ErrorMsg,false);
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
                    if (!fileExists)
                    {
                        if (fileToRead.Contains("favicon")) return new HTTPStreamResult(false,fileToRead, HttpStatusCode.NotFound, null, null, "File '" + fileToRead + "' does not exist!", true);
                        else return new HTTPStreamResult(false,fileToRead, HttpStatusCode.NotFound, null, null, "File '" + fileToRead + "' does not exist!",false);
                    }
                    if (fileExists) fileLastModifiedTimeUTC = File.GetLastWriteTimeUtc(fileToRead).ToUniversalTime();


                    outputMemoryStream = new MemoryStream();
                    HTTPCache httpCache = null;
                    bool obtainedFromCache = false;
                    if (this.EnableCache)
                    {
                        if (this.CacheFiles.TryGetValue(fileToRead, out httpCache) && httpCache.LastModifiedTimeUTC == fileLastModifiedTimeUTC)
                        {
                            httpCache.RequestCount++;
                            httpCache.LastAccessTimeUTC = DateTime.UtcNow;
                            httpCache.MemoryStream.Position = 0;
                            await httpCache.MemoryStream.CopyToAsync(outputMemoryStream);
                            obtainedFromCache = true;
                        }
                        else
                        {
                            using (Stream input = new FileStream(fileToRead, FileMode.Open))
                            {
                                byte[] buffer = new byte[4096]; //byte[] buffer = new byte[1024 * 32]; //4096 faster than 32K
                                int nbytes; MemoryStream ms = new MemoryStream();
                                while ((nbytes = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    if (cancellationTokenSource.IsCancellationRequested) 
                                        return new HTTPStreamResult(false, fileToRead, HttpStatusCode.InternalServerError, null, null, "Request was cancelled by server while reading file '" + fileToRead + "'",false);
                                    await ms.WriteAsync(buffer, 0, nbytes);
                                }
                                ms.Position = 0;
                                await ms.CopyToAsync(outputMemoryStream);
                                httpCache = new HTTPCache(fileToRead, ms, fileLastModifiedTimeUTC, outputMemoryStream.Length / 1024);
                                if (httpCache.ContentLengthInKB <= (this.OnlyCacheItemsLessThenMB * 1024)) this.CacheFiles.TryAdd(fileToRead, httpCache);
                                obtainedFromCache = false;
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
                                if (cancellationTokenSource.IsCancellationRequested) return new HTTPStreamResult(false, fileToRead, HttpStatusCode.InternalServerError, null, null, "Request was cancelled by server while reading file '" + fileToRead + "'",false);
                                await outputMemoryStream.WriteAsync(buffer, 0, nbytes);
                            }
                            obtainedFromCache = false;
                        }
                    }
                    string contentType = _mimeTypeMappings.TryGetValue(System.IO.Path.GetExtension(fileToRead), out string mime) ? mime : "application/octet-stream";
                    outputMemoryStream.Position = 0;
                    return new HTTPStreamResult(obtainedFromCache, fileToRead, HttpStatusCode.OK, contentType, outputMemoryStream, null, false);
                }
                catch (Exception ex)
                {
                    return new HTTPStreamResult(false,fileToRead, HttpStatusCode.InternalServerError, null, null, "Generic Error wile processing file '" + fileToRead + "'\r\n" + ex.ToString(),false);
                }
            }
            catch (Exception ex2)
            {
                return new HTTPStreamResult(false,fileToRead, HttpStatusCode.InternalServerError, null, null, "Generic Error: " + ex2.ToString(),false);
            }
        }

        public bool TryParseAbsolutePath(string absolutePath, out string fileToRead, out string fileToReadDir, out string errorMsg)
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
}
