using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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
        public HttpServerConfig HttpServerConfig { get; set; } = null;
        public static ConcurrentQueue<LogStruct> LogsCache = new ConcurrentQueue<LogStruct>();
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
            if (this.HttpServerConfig == null) { this.HttpServerConfig = new HttpServerConfig(); this.HttpServerConfig.LoadConfigFile(); }
            this.HttpServer = new HttpServer.HttpServer((new CancellationTokenSource()).Token, this.HttpServerConfig);
            this.HttpServer.OnErrorLog = (log) => { Log.Error(log); LogsCache.Enqueue(new LogStruct(log, LogLevel.Error)); };
            this.HttpServer.OnInfoLog = (log) => { Log.Information(log); LogsCache.Enqueue(new LogStruct(log, LogLevel.Info)); };
            this.HttpServer.OnWarningLog = (log) => { Log.Warning(log); LogsCache.Enqueue(new LogStruct(log, LogLevel.Warning)); };
            this.HttpServer.WebAPIs = new List<WebAPIAction>();
            this.HttpServer.WebAPIs.Add(new WebAPIAction(
                "/config/load", async (req) =>
                {
                    if (!CheckAuthentication(req)) { await req.RespondWithText("You are unauthorized to access this page!", HttpStatusCode.Unauthorized); return; }
                    await req.RespondWithJson(this.HttpServerConfig).ConfigureAwait(false);
                },"GET"));
            this.HttpServer.WebAPIs.Add(new WebAPIAction(
                "/config/save", async (req) => 
                {
                    if (!CheckAuthentication(req)) { await req.RespondWithText("You are unauthorized to access this page!", HttpStatusCode.Unauthorized); return; }
                    HttpServerConfig httpServerConfig = JsonConvert.DeserializeObject<HttpServerConfig>(req.RequestBody);
                    await req.RespondEmpty().ConfigureAwait(false);
                },"PUT"));
            this.HttpServer.WebAPIs.Add(new WebAPIAction(
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
            this.HttpServer.StartAsync().ConfigureAwait(false);
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
                    if (string.Equals(UN,this.HttpServerConfig.ConfigUsername,StringComparison.InvariantCultureIgnoreCase) && (this.HttpServerConfig.ConfigPasswordHash==null || PW.ToSha256()==this.HttpServerConfig.ConfigPasswordHash)) return true;
                }
            }
            return false;
        }
    }
    public static partial class Extensions
    {
        public static string ToStringFromBase64(this string encodedString)
        {
            byte[] data = Convert.FromBase64String(encodedString);
            string decodedString = Encoding.UTF8.GetString(data);
            return decodedString;
        }
        public static string ToSha256(this string value)
        {
            var message = Encoding.ASCII.GetBytes(value);
            SHA256Managed hashString = new SHA256Managed();
            string hex = "";
            var hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue) hex += String.Format("{0:x2}", x);
            return hex;
        }

        public static string ToBase64(this string text)
        {
            return ToBase64(text, Encoding.UTF8);
        }

        public static string ToBase64(this string text, Encoding encoding)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            byte[] textAsBytes = encoding.GetBytes(text);
            return Convert.ToBase64String(textAsBytes);
        }
    }
}
