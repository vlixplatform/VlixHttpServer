using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Vlix.HttpServer
{

    public class StaticFileProcessor
    {
        public StaticFileProcessorConfig Config { get; set; }
            
        public CancellationTokenSource cancellationTokenSource;
        public StaticFileProcessor(StaticFileProcessorConfig StaticFileProcessorConfig)
        {
            cancellationTokenSource = new CancellationTokenSource();
            this.Config = StaticFileProcessorConfig;
            Task.Run(async () =>
            {
                while (true)
                {
                    for (int i = 0; i < 3; i++) { await Task.Delay(1000); if (cancellationTokenSource.IsCancellationRequested) break; }
                    if (this.Config.EnableCache)
                    {
                        if (this.CacheFiles.TotalCacheInKB > this.Config.MaximumCacheSizeInMB * 1024)
                        {
                            var cacheFileToRemoveInOrder = this.CacheFiles.Values.OrderBy(h => h.LastAccessTimeUTC).ThenBy(h => h.RequestCount).Select(h => h.FileToRead).ToList();
                            foreach (string key in cacheFileToRemoveInOrder)
                            {
                                if (this.CacheFiles.Count == 0) break;
                                if (this.Config.MaximumCacheSizeInMB <= 0) break;
                                if (this.CacheFiles.TryRemove(key, out HTTPCache hTTPCacheRemoved))
                                {
                                    if (this.CacheFiles.TotalCacheInKB < this.Config.MaximumCacheSizeInMB * 1024) break;
                                }
                            }
                        }
                    }
                }
            });
        }
        public void Shutdown() { cancellationTokenSource.Cancel();  }

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
        public async Task<HTTPStreamResult> ProcessRequestAsync(string callerIP, string absolutePath, string wWWDirectory)
        {
            string fileToRead = null;
            MemoryStream outputMemoryStream = null;
            try
            {
                if (!this.TryParseAbsolutePath(wWWDirectory,absolutePath, out fileToRead, out string fileToReadDir, out string ErrorMsg))
                {
                    return new HTTPStreamResult(false, fileToRead, HttpStatusCode.NotFound, null, null, ErrorMsg, false);
                }

                //Find a FileToRead if a directory is requested. Find standard files "index.html", "index.htm", "default.html", "default.htm"
                bool fileExists = false;
                DateTime fileLastModifiedTimeUTC = DateTime.MinValue;
                if (string.IsNullOrEmpty(fileToRead))
                {
                    foreach (string indexFile in DefaultDocuments)
                    {
                        string temp = Path.Combine(fileToReadDir, indexFile);
                        if (File.Exists(temp))
                        {
                            fileToRead = temp;
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
                        if (fileToRead.Contains("favicon")) return new HTTPStreamResult(false, fileToRead, HttpStatusCode.NotFound, null, null, "File '" + fileToRead + "' does not exist!", true);
                        else return new HTTPStreamResult(false, fileToRead, HttpStatusCode.NotFound, null, null, "File '" + fileToRead + "' does not exist!", false);
                    }
                    if (fileExists) fileLastModifiedTimeUTC = File.GetLastWriteTimeUtc(fileToRead).ToUniversalTime();


                    outputMemoryStream = new MemoryStream();
                    HTTPCache httpCache = null;
                    bool obtainedFromCache = false;
                    if (this.Config.EnableCache)
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
                                        return new HTTPStreamResult(false, fileToRead, HttpStatusCode.InternalServerError, null, null, "Request was cancelled by server while reading file '" + fileToRead + "'", false);
                                    await ms.WriteAsync(buffer, 0, nbytes);
                                }
                                ms.Position = 0;
                                await ms.CopyToAsync(outputMemoryStream);
                                httpCache = new HTTPCache(fileToRead, ms, fileLastModifiedTimeUTC, outputMemoryStream.Length / 1024);
                                if (httpCache.ContentLengthInKB <= (this.Config.OnlyCacheItemsLessThenMB * 1024)) this.CacheFiles.TryAdd(fileToRead, httpCache);
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
                                if (cancellationTokenSource.IsCancellationRequested) return new HTTPStreamResult(false, fileToRead, HttpStatusCode.InternalServerError, null, null, "Request was cancelled by server while reading file '" + fileToRead + "'", false);
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
                    return new HTTPStreamResult(false, fileToRead, HttpStatusCode.InternalServerError, null, null, "Generic Error wile processing file '" + fileToRead + "'\r\n" + ex.ToString(), false);
                }


            }
            catch (Exception ex2)
            {
                return new HTTPStreamResult(false, fileToRead, HttpStatusCode.InternalServerError, null, null, "Generic Error: " + ex2.ToString(), false);
            }
        }

        public bool TryParseAbsolutePath(string wWWDirectory,string absolutePath, out string fileToRead, out string fileToReadDir, out string errorMsg)
        {
            errorMsg = null;
            absolutePath = absolutePath.TrimEnd();
            absolutePath = Regex.Replace(absolutePath, @"\s*(?=/)", "");
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
            var lastPortionContainsDot = lastURLPortion.Contains(".");
            if (!lastPortionContainsDot) //handles "/afolder"
            {
                fileToRead = "";
                string Temp = absolutePath;
                //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
                Temp = absolutePath.Replace('/', Path.DirectorySeparatorChar);
                fileToReadDir = wWWDirectory + System.IO.Path.GetDirectoryName(Temp + Path.DirectorySeparatorChar);                
                if (fileToReadDir.EndsWith(Path.DirectorySeparatorChar)) fileToReadDir = fileToReadDir.Substring(0, fileToReadDir.Length - 1);
            }
            else
            {
                string Temp = absolutePath;
                //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
                Temp = absolutePath.Replace('/', Path.DirectorySeparatorChar);
                fileToReadDir = wWWDirectory + System.IO.Path.GetDirectoryName(Temp);
                if (fileToReadDir.EndsWith(Path.DirectorySeparatorChar)) fileToReadDir = fileToReadDir.Substring(0, fileToReadDir.Length - 1);
                fileToRead = Path.Combine(fileToReadDir,Path.GetFileName(Temp));
            }
            return true;
        }
    }


}