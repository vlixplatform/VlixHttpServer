using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vlix.HttpServer;
namespace Vlix
{
    public class WebServer
    {
        public WebServer()
        {

        }
        public HttpServer.HttpServer HttpServer { get; set; } = null;
        public HttpServer.HttpServer ConfigServer { get; set; } = null;
        public static ConcurrentQueue<LogStruct> LogsCache = new ConcurrentQueue<LogStruct>();
        public WebServerConfig WebServerConfig = null;
        public Task<bool> StartAsync()
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    if (LogsCache.Count > 100) for (int n = 0; n < LogsCache.Count - 100; n++) LogsCache.TryDequeue(out _);
                    await Task.Delay(15000).ConfigureAwait(false);
                }
            });
            WebServerConfig = new WebServerConfig();
            WebServerConfig.LoadConfigFile("webserver.json"); 
            this.HttpServer = new HttpServer.HttpServer((new CancellationTokenSource()).Token, WebServerConfig);
            this.HttpServer.OnErrorLog = (log) => { Log.Error(log); LogsCache.Enqueue(new LogStruct(log, LogLevel.Error)); };
            this.HttpServer.OnInfoLog = (log) => { Log.Information(log); LogsCache.Enqueue(new LogStruct(log, LogLevel.Info)); };
            this.HttpServer.OnWarningLog = (log) => { Log.Warning(log); LogsCache.Enqueue(new LogStruct(log, LogLevel.Warning)); };
            this.HttpServer.StartAsync().ConfigureAwait(false);



            HttpServerConfig configServerConfig = new HttpServerConfig() //Default Value if file does not exist
            {
                EnableHTTP = true, //Http must alays be enables
                HTTPPort = WebServerConfig.ConfigUtility.HTTPPort,
                AllowLocalhostConnectionsOnlyForHttp = WebServerConfig.ConfigUtility.AllowLocalhostConnectionsOnlyForHttp,
                EnableHTTPS = WebServerConfig.ConfigUtility.EnableHTTPS,
                HTTPSPort = WebServerConfig.ConfigUtility.HTTPSPort,
                SSLCertificateStoreName = WebServerConfig.ConfigUtility.SSLCertificateStoreName,
                SSLCertificateSubjectName = WebServerConfig.ConfigUtility.SSLCertificateSubjectName,
                EnableCache=false
            };
            this.ConfigServer = new HttpServer.HttpServer((new CancellationTokenSource()).Token, configServerConfig);
            this.ConfigServer.WebAPIs = new List<WebAPIAction>();
            this.ConfigServer.WebAPIs.Add(new WebAPIAction(
                "/config/getsslcerts", async (req) =>
                {
                    List<SSLCertVM> certs = null;
                    if (!CheckAuthentication(req)) { await req.RespondWithText("You are unauthorized to access this page!", HttpStatusCode.Unauthorized); return; }
                    StoreName storeName = StoreName.My; StoreLocation storeLocation = StoreLocation.LocalMachine;
                    try
                    {
                        if (req.Query.TryGetValue("storename", out string storeNameStr)) storeName = (StoreName)Enum.Parse(typeof(StoreName), storeNameStr);//(StoreName)Convert.ToInt64(storeNameStr);
                        if (req.Query.TryGetValue("storelocation", out string storeLocationStr)) storeLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), storeLocationStr); // (StoreLocation)Convert.ToInt64(storeNameStr);
                        X509Store store = new X509Store(storeName, storeLocation);
                        store.Open(OpenFlags.ReadOnly);
                        certs = new List<SSLCertVM>();                      
                        foreach (X509Certificate2 certificate in store.Certificates) if (certificate.HasPrivateKey) certs.Add(new SSLCertVM(certificate, storeName, null));
                    }
                    catch
                    {
                        certs = null;
                    }
                    await req.RespondWithJson(certs).ConfigureAwait(false);
                }, "GET"));
            this.ConfigServer.WebAPIs.Add(new WebAPIAction(
                "/config/load", async (req) =>
                {
                    if (!CheckAuthentication(req)) { await req.RespondWithText("You are unauthorized to access this page!", HttpStatusCode.Unauthorized); return; }
                    await req.RespondWithJson(WebServerConfig).ConfigureAwait(false);
                },"GET"));
            this.ConfigServer.WebAPIs.Add(new WebAPIAction(
                "/config/save", async (req) => 
                {
                    if (!CheckAuthentication(req)) { await req.RespondWithText("You are unauthorized to access this page!", HttpStatusCode.Unauthorized); return; }
                    this.WebServerConfig = JsonConvert.DeserializeObject<WebServerConfig>(req.RequestBody);
                    this.WebServerConfig.SaveConfigFile("webserver.json");
                    await req.RespondEmpty().ConfigureAwait(false);
                },"PUT"));
            this.ConfigServer.WebAPIs.Add(new WebAPIAction(
                "/config/getlogs/", async (req) =>
                {
                    if (!CheckAuthentication(req)) { await req.RespondWithText("You are unauthorized to access this page!", HttpStatusCode.Unauthorized); return; }
                    long lastlogreadtickutc = 0;
                    List<LogStruct> LogList = null;
                    try
                    {
                        if (req.Query.TryGetValue("lastread", out string tickStr)) lastlogreadtickutc = Convert.ToInt64(tickStr);
                        IEnumerable<LogStruct> Temp = WebServer.LogsCache.Where(L => L.TimeStampInUTC.Ticks > lastlogreadtickutc).Take(1000);
                        LogList = Temp.ToList();
                    }
                    catch
                    {
                        LogList = null;
                    }
                    await req.RespondWithJson(LogList).ConfigureAwait(false);
                }, "GET"));
            this.ConfigServer.StartAsync().ConfigureAwait(false);


            return Task.FromResult(true);
        }

        public bool CheckAuthentication(WebAPIActionInput webAPIActionInput)
        {
            Dictionary<string, string> headers = webAPIActionInput.Headers;
            if (headers.TryGetValue("Authorization", out string bA))
            {
                string[] bA2 = bA.Split(' ');
                if (bA2.Length != 2) return false;
                if (string.Equals(bA2[0],"Basic",StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] bA3 = bA2[1].ToStringFromBase64().Split(':');
                    if (bA3.Length != 2) return false;
                    string UN = bA3[0];
                    string PW = bA3[1];
                    if (string.Equals(UN,this.WebServerConfig.ConfigUtility.ConfigUsername,StringComparison.InvariantCultureIgnoreCase) 
                        && (this.WebServerConfig.ConfigUtility.ConfigPasswordHash==null || PW.ToSha256()== this.WebServerConfig.ConfigUtility.ConfigPasswordHash)) return true;
                }
            }
            return false;
        }
    }
}
