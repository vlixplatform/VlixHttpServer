using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Vlix;


namespace Vlix.HttpServer
{
    partial class Program
    {
        static async Task Main(string[] args)
        {

            //string pathTest = "http://clove/?id={Id}&name={Name}";
            //Uri uri = new Uri(pathTest);
            //var nvc = HttpUtility.ParseQueryString(HttpUtility.UrlDecode(uri.Query));
            //foreach (var inputNV in nvc.AllKeys) 
            //{
            //    var dTypeStr = nvc[inputNV];
            //    if (string.Equals(dTypeStr,"{string}",StringComparison.InvariantCultureIgnoreCase))
            //    {

            //    }
            //}

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

            WebServer webServer = new WebServer();
            await webServer.StartAsync();
            _ = Task.Run(async () =>
            {
                int n = 0;
                while (true)
                {
                    n++;
                    webServer.HttpServer.AddLog("This is Log " + n);
                    await Task.Delay(100);
                }
            });

            Console.ReadKey();
        }
    }


}
