using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace  Vlix.HttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpServerConfig config = new HttpServerConfig();
            config.LoadConfigFile();

            string[] myFiles = Directory.GetFiles(config.WWWDirectory);
            if (myFiles.FirstOrDefault(f => Path.GetFileName(f) == "index.html") == null) File.Copy(Path.Combine("Sample", "index.html"), Path.Combine(config.WWWDirectory, "index.html"));
            if (myFiles.FirstOrDefault(f => Path.GetFileName(f) == "test.html") == null) File.Copy(Path.Combine("Sample", "test.html"), Path.Combine(config.WWWDirectory, "test.html"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(config.LogDirectory, "HTTPServer.log"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            HttpServer vlixHttpServer = new HttpServer((new CancellationTokenSource()).Token, config);
            vlixHttpServer.OnErrorLog = (log) => Log.Error(log);
            vlixHttpServer.OnInfoLog = (log) => Log.Information(log);
            vlixHttpServer.OnWarningLog = (log) => Log.Warning(log);
            vlixHttpServer.Start();
        }
    }
}
