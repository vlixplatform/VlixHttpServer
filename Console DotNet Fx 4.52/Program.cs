using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace  Vlix.HttpServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            WebServer webServer = new WebServer();
            await webServer.StartAsync();

            string[] wWWFiles = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Sample"));
            foreach (var wWWFile in wWWFiles) File.Copy(wWWFile, Path.Combine(webServer.WebServerConfig.WWWDirectoryParsed(), Path.GetFileName(wWWFile)), true);
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().WriteTo.File(Path.Combine(webServer.WebServerConfig.LogDirectoryParsed(), "HTTPServer.log"), rollingInterval: RollingInterval.Day).CreateLogger();


            _ = Task.Run(async () =>
            {
                int n = 0;
                while (true)
                {
                    n++;
                    webServer.HttpServer.AddLog("This is Log " + n);
                    await Task.Delay(1000);
                }
            });

            Console.ReadKey();
        }
    }
}
