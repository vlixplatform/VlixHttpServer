using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Vlix.HttpServerConfig;

namespace Vlix.HttpServer
{
    partial class Program
    {



        static async Task Main(string[] args)
        {
            //HttpServerConfig config = new HttpServerConfig() { SSLCertificateSubjectName = "CN=azrin.vlix.me" };
            //config.Rules.Add(new HttpToHttpsRedirectRule("azrin.vlix.me"));
            //config.Rules.Add(new SimpleReverseProxyRule("azrin.vlix.me", "/rproxy/*", 5000));
            //config.Rules.Add(new SimplePathDenyRule("/forbidden.html"));
            //config.SaveConfigFile();

            HttpServerConfig config = new HttpServerConfig();
            config.LoadConfigFile();
            string[] wWWFiles = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Sample"));
            foreach (var wWWFile in wWWFiles) File.Copy(wWWFile, Path.Combine(config.WWWDirectory, Path.GetFileName(wWWFile)), true);
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().WriteTo.File(Path.Combine(config.LogDirectory, "HTTPServer.log"), rollingInterval: RollingInterval.Day).CreateLogger();





            //HttpServer internalServer = new HttpServer(config.WWWDirectory, 5000);
            //internalServer.OnErrorLog = (log) => Log.Error("internal > " + log);
            //internalServer.OnInfoLog = (log) => Log.Information("internal > " + log);
            //internalServer.OnWarningLog = (log) => Log.Warning("internal > " + log);
            //await internalServer.StartAsync();


            await (new WebServer()).Start();
            Console.ReadKey();
        }
    }

    public class WebServer
    { 
        public HttpServerConfig HttpServerConfig { get; set; } = null;
        public static ConcurrentQueue<LogStruct> LogsCache = new ConcurrentQueue<LogStruct>();
        public async Task Start()
        {
            _= Task.Run(async () =>
            {
                while (true)
                {
                    if (LogsCache.Count > 100) for (int n = 0; n < LogsCache.Count - 100; n++) LogsCache.TryDequeue(out _);
                    await Task.Delay(15000);
                }
            });
            if (HttpServerConfig == null) { this.HttpServerConfig = new HttpServerConfig(); this.HttpServerConfig.LoadConfigFile(); }
            HttpServer vlixHttpServer = new HttpServer((new CancellationTokenSource()).Token, this.HttpServerConfig);
            vlixHttpServer.OnErrorLog = (log) => { Log.Error(log); LogsCache.Enqueue(new LogStruct(log, LogLevel.Error)); };
            vlixHttpServer.OnInfoLog = (log) => { Log.Information(log); LogsCache.Enqueue(new LogStruct(log, LogLevel.Info)); };
            vlixHttpServer.OnWarningLog = (log) => { Log.Warning(log); LogsCache.Enqueue(new LogStruct(log, LogLevel.Warning)); };
            await vlixHttpServer.StartAsync();                
        }
    }


}
