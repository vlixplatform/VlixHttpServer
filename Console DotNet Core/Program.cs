using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
namespace  Vlix.HttpServer
{
    class Program
    {
        static void Main(string[] args)
        {

            HttpServerConfig config = new HttpServerConfig();

            string appDirectory = ConfigurationManager.AppSettings.Get("AppDirectory");
            appDirectory = appDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));


            Directory.CreateDirectory(appDirectory);
            string configFilePath = Path.Combine(appDirectory, "httpserver.json");
            if (File.Exists(configFilePath))
            {
                string configJSONStr = File.ReadAllText(configFilePath);
                config = JsonConvert.DeserializeObject<HttpServerConfig>(configJSONStr);
            }
            else
            {
                string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, jsonString);
            }
            config.LogDirectory = config.LogDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            config.WWWDirectory = config.WWWDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));



            string[] myFiles = Directory.GetFiles(config.WWWDirectory);
            if (myFiles.FirstOrDefault(f => Path.GetFileName(f) == "index.html") == null) File.Copy(Path.Combine("Sample", "index.html"), Path.Combine(config.WWWDirectory, "index.html"));
            if (myFiles.FirstOrDefault(f => Path.GetFileName(f) == "test.html") == null) File.Copy(Path.Combine("Sample", "test.html"), Path.Combine(config.WWWDirectory, "test.html"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(config.LogDirectory, "HTTPServer.log"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            VlixHttpServer vlixHttpServer = new VlixHttpServer((new CancellationTokenSource()).Token, config);
            vlixHttpServer.OnErrorLog = (log) => Log.Error(log);
            vlixHttpServer.OnInfoLog = (log) => Log.Information(log);
            vlixHttpServer.OnWarningLog = (log) => Log.Warning(log);
            vlixHttpServer.Start();
        }
    }
}
