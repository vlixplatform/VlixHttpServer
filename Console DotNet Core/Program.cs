using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Vlix.HttpServer
{
    class Program
    {
        static async Task Main(string[] args)
        {            
            HttpServerConfig config = new HttpServerConfig() { SSLCertificateSubjectName = "CN=azrin.vlix.me" };
            config.Rules.Add(new HttpToHttpsRedirectRule("azrin.vlix.me"));
            config.Rules.Add(new SimpleReverseProxyRule("azrin.vlix.me", "/rproxy/*",5000));
            //config.Rules.Add(new SimpleHostNameRedirectRule("*.vlixplatform.com", "vlixplatform.com"));
            
            //config.Rules.Add(new SimplePathDenyRule("/forbidden.html"));

            config.SaveConfigFile();
            config.LoadConfigFile();
            string[] wWWWFiles = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Sample"));
            foreach (var wWWFile in wWWWFiles) File.Copy(wWWFile, Path.Combine(config.WWWDirectory, Path.GetFileName(wWWFile)),true);
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().WriteTo.File(Path.Combine(config.LogDirectory, "HTTPServer.log"), rollingInterval: RollingInterval.Day).CreateLogger();


            HttpServer internalServer = new HttpServer(config.WWWDirectory, 5000);
            internalServer.OnErrorLog = (log) => Log.Error("internal > " + log);
            internalServer.OnInfoLog = (log) => Log.Information("internal > " + log);
            internalServer.OnWarningLog = (log) => Log.Warning("internal > " + log);
            await internalServer.StartAsync();

            HttpServer vlixHttpServer = new HttpServer((new CancellationTokenSource()).Token, config);
            vlixHttpServer.OnErrorLog = (log) => Log.Error(log);
            vlixHttpServer.OnInfoLog = (log) => Log.Information(log);
            vlixHttpServer.OnWarningLog = (log) => Log.Warning(log);
            await vlixHttpServer.StartAsync();
            Console.ReadKey();
        }
    }

    
}
