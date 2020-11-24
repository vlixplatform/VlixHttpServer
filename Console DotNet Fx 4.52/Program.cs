using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vlix
{
    class Program
    {
        static void Main(string[] args)
        {
            string HTTPPort = ConfigurationManager.AppSettings.Get("HTTPPort");
            string HTTPSPort = ConfigurationManager.AppSettings.Get("HTTPSPort");
            string EnableCache = ConfigurationManager.AppSettings.Get("EnableCache");
            string WWWDirectory = ConfigurationManager.AppSettings.Get("WWWDirectory");
            string LogDirectory = ConfigurationManager.AppSettings.Get("LogDirectory");
            string OnlyCacheItemsLessThenMB = ConfigurationManager.AppSettings.Get("OnlyCacheItemsLessThenMB");
            string MaximumCacheSizeInMB = ConfigurationManager.AppSettings.Get("MaximumCacheSizeInMB");
            WWWDirectory = WWWDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            LogDirectory = LogDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            string AllowLocalhostConnectionsOnly = ConfigurationManager.AppSettings.Get("AllowLocalhostConnectionsOnly");


            Directory.CreateDirectory(WWWDirectory);
            string[] myFiles = Directory.GetFiles(WWWDirectory);
            if (myFiles.FirstOrDefault(f => Path.GetFileName(f) == "index.html") == null) File.Copy("Sample\\index.html", WWWDirectory + "\\index.html");
            if (myFiles.FirstOrDefault(f => Path.GetFileName(f) == "test.html") == null) File.Copy("Sample\\test.html", WWWDirectory + "\\test.html");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(LogDirectory + "\\HTTPServer.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            VlixHttpServer vlixHttpServer = new VlixHttpServer((new CancellationTokenSource()).Token, WWWDirectory, HTTPPort.ToInt(80), HTTPSPort.ToInt(443), EnableCache.ToBool(),
                OnlyCacheItemsLessThenMB.ToInt(10), MaximumCacheSizeInMB.ToInt(250), AllowLocalhostConnectionsOnly.ToBool());
            vlixHttpServer.OnErrorLog = (log) => Log.Error(log);
            vlixHttpServer.OnInfoLog = (log) => Log.Information(log);
            vlixHttpServer.OnWarningLog = (log) => Log.Warning(log);
            vlixHttpServer.Start();
        }
    }
}
