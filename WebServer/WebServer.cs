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
        public async Task<bool> StartAsync()
        {
            try
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
                await this.HttpServer.StartAsync().ConfigureAwait(false);


                this.ConfigServer = new HttpServer.HttpServer((new CancellationTokenSource()).Token, new HttpServerConfig(WebServerConfig.UtilityConfig));
                this.ConfigServer.OnErrorLog = (log) => { Log.Error(log); LogsCache.Enqueue(new LogStruct(log, LogLevel.Error)); };
                this.ConfigServer.OnInfoLog = (log) => { Log.Information(log); LogsCache.Enqueue(new LogStruct(log, LogLevel.Info)); };
                this.ConfigServer.OnWarningLog = (log) => { Log.Warning(log); LogsCache.Enqueue(new LogStruct(log, LogLevel.Warning)); };
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
                            var x509Certificate2s = await SSLCertificateServices.GetSSLCertificates(storeName);
                            foreach (X509Certificate2 certificate in x509Certificate2s) if (certificate.HasPrivateKey) certs.Add(new SSLCertVM(certificate, storeName, null));
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
                        var sslCert = await SSLCertificateServices.GetSSLCertificate(this.WebServerConfig.SSLCertificateStoreName, this.WebServerConfig.SSLCertificateSubjectName);
                        var sslCertConfig = await SSLCertificateServices.GetSSLCertificate(this.WebServerConfig.UtilityConfig.SSLCertificateStoreName, this.WebServerConfig.UtilityConfig.SSLCertificateSubjectName);
                        await req.RespondWithJson(new Tuple<WebServerConfig, SSLCertVM, SSLCertVM>(this.WebServerConfig, new SSLCertVM(sslCert, this.WebServerConfig.SSLCertificateStoreName, null), new SSLCertVM(sslCertConfig, this.WebServerConfig.UtilityConfig.SSLCertificateStoreName, null))).ConfigureAwait(false);
                    }, "GET"));
                this.ConfigServer.WebAPIs.Add(new WebAPIAction(
                    "/config/save", async (req) =>
                    {
                        if (!CheckAuthentication(req)) { await req.RespondWithText("You are unauthorized to access this page!", HttpStatusCode.Unauthorized); return; }
                        WebServerConfig newWebServerConfig = JsonConvert.DeserializeObject<WebServerConfig>(req.RequestBody, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                        bool restartServer = (this.WebServerConfig.EnableHTTP != newWebServerConfig.EnableHTTP) || (this.WebServerConfig.EnableHTTPS != newWebServerConfig.EnableHTTPS) || (this.WebServerConfig.HTTPPort != newWebServerConfig.HTTPPort)
                        || (this.WebServerConfig.HTTPSPort != newWebServerConfig.HTTPSPort) || (this.WebServerConfig.LogDirectory != newWebServerConfig.LogDirectory) || (this.WebServerConfig.SSLCertificateStoreName != newWebServerConfig.SSLCertificateStoreName)
                        || (this.WebServerConfig.SSLCertificateSubjectName != newWebServerConfig.SSLCertificateSubjectName) || (this.WebServerConfig.WWWDirectory != newWebServerConfig.WWWDirectory);
                        this.HttpServer.Config = newWebServerConfig;
                        if (restartServer)
                        {
                            this.HttpServer.Stop();
                            await this.HttpServer.StartAsync().ConfigureAwait(false);
                        }
                        this.WebServerConfig = newWebServerConfig;
                        this.WebServerConfig.SaveConfigFile("webserver.json");
                        await req.RespondEmpty().ConfigureAwait(false);
                    }, "PUT"));
                this.ConfigServer.WebAPIs.Add(new WebAPIAction(
                    "/config/saveutilitysettings", async (req) =>
                    {
                        if (!CheckAuthentication(req)) { await req.RespondWithText("You are unauthorized to access this page!", HttpStatusCode.Unauthorized); return; }
                        UtilityConfig newUtilityConfig = JsonConvert.DeserializeObject<UtilityConfig>(req.RequestBody, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                        bool restartConfigServer = (newUtilityConfig.ConfigPasswordHash != this.WebServerConfig.UtilityConfig.ConfigPasswordHash || newUtilityConfig.ConfigUsername != this.WebServerConfig.UtilityConfig.ConfigUsername
                        || newUtilityConfig.EnableHTTPS != this.WebServerConfig.UtilityConfig.EnableHTTPS || newUtilityConfig.HTTPPort != this.WebServerConfig.UtilityConfig.HTTPPort || newUtilityConfig.HTTPSPort != this.WebServerConfig.UtilityConfig.HTTPSPort
                        || newUtilityConfig.SSLCertificateStoreName != this.WebServerConfig.UtilityConfig.SSLCertificateStoreName || newUtilityConfig.SSLCertificateSubjectName != this.WebServerConfig.UtilityConfig.SSLCertificateSubjectName
                        || newUtilityConfig.AllowLocalhostConnectionsOnlyForHttp != this.WebServerConfig.UtilityConfig.AllowLocalhostConnectionsOnlyForHttp);
                        this.WebServerConfig.UtilityConfig = JsonConvert.DeserializeObject<UtilityConfig>(req.RequestBody, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                        this.WebServerConfig.SaveConfigFile("webserver.json");
                        await req.RespondWithJson(restartConfigServer).ConfigureAwait(false);
                        await Task.Delay(3000);
                        if (restartConfigServer)
                        {
                            this.ConfigServer.Stop();
                            this.ConfigServer.Config.AllowLocalhostConnectionsOnlyForHttp = this.WebServerConfig.UtilityConfig.AllowLocalhostConnectionsOnlyForHttp;
                            this.ConfigServer.Config.EnableHTTPS = this.WebServerConfig.UtilityConfig.EnableHTTPS;
                            this.ConfigServer.Config.HTTPPort = this.WebServerConfig.UtilityConfig.HTTPPort;
                            this.ConfigServer.Config.HTTPSPort = this.WebServerConfig.UtilityConfig.HTTPSPort;
                            this.ConfigServer.Config.SSLCertificateStoreName = this.WebServerConfig.UtilityConfig.SSLCertificateStoreName;
                            this.ConfigServer.Config.SSLCertificateSubjectName = this.WebServerConfig.UtilityConfig.SSLCertificateSubjectName;
                            await this.ConfigServer.StartAsync().ConfigureAwait(false);
                        }
                    }, "PUT"));
                this.ConfigServer.WebAPIs.Add(new WebAPIAction(
                    "/config/getlogs", async (req) =>
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
                var res = await this.ConfigServer.StartAsync().ConfigureAwait(false);


                return res;
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                return false;
            }
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
                    if (string.Equals(UN,this.WebServerConfig.UtilityConfig.ConfigUsername,StringComparison.InvariantCultureIgnoreCase) 
                        && (this.WebServerConfig.UtilityConfig.ConfigPasswordHash==null || PW.ToSha256()== this.WebServerConfig.UtilityConfig.ConfigPasswordHash)) return true;
                }
            }
            return false;
        }
    }
}
