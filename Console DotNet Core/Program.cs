using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Vlix
{
    class Program
    {
        static void Main(string[] args)
        {

            

            string HttpPort = ConfigurationManager.AppSettings.Get("HttpPort");
            string HttpsPort = ConfigurationManager.AppSettings.Get("HttpsPort");
            string EnableCache = ConfigurationManager.AppSettings.Get("EnableCache");
            string WWWDirectory = ConfigurationManager.AppSettings.Get("WWWDirectory");
            string LogDirectory = ConfigurationManager.AppSettings.Get("LogDirectory");
            WWWDirectory = WWWDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            LogDirectory = LogDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));


            Directory.CreateDirectory(WWWDirectory);
            string[] myFiles = Directory.GetFiles(WWWDirectory);
            if (myFiles.FirstOrDefault(f => Path.GetFileName(f) == "index.html") == null) File.Copy("Sample\\index.html", WWWDirectory + "\\index.html");
            if (myFiles.FirstOrDefault(f => Path.GetFileName(f) == "test.html") == null) File.Copy("Sample\\test.html", WWWDirectory + "\\test.html");
            


            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(LogDirectory + "\\HttpServer.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            VlixHttpServer vlixHttpServer = new VlixHttpServer(WWWDirectory, Convert.ToInt32(HttpPort), Convert.ToInt32(HttpsPort), EnableCache.ToBool());
            vlixHttpServer.OnError = (ex) => Log.Error(ex.ToString());
            vlixHttpServer.OnInfoLog = (log) => Log.Information(log);
            vlixHttpServer.OnWarningLog = (log) => Log.Warning(log);
            vlixHttpServer.Start();
        }
    }
}
