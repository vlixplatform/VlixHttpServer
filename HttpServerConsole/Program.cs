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
            
            string Port = ConfigurationManager.AppSettings.Get("Port");
            string EnableCache = ConfigurationManager.AppSettings.Get("EnableCache");
            string WWWDirectory = ConfigurationManager.AppSettings.Get("WWWDirectory");
            string LogDirectory = ConfigurationManager.AppSettings.Get("LogDirectory");
            WWWDirectory = WWWDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            LogDirectory = LogDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            Directory.CreateDirectory(WWWDirectory);            
            //string directory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Vlix\\HttpServer"; //This translates to => C:\ProgramData\Vlix\HttpServer
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(LogDirectory + "\\HttpServer.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            VlixHttpServer vlixHttpServer = new VlixHttpServer(WWWDirectory, Convert.ToInt32(Port), EnableCache.ToBool());
            vlixHttpServer.OnError = (ex) => Log.Error(ex.ToString());
            vlixHttpServer.OnInfoLog = (log) => Log.Information(log);
            vlixHttpServer.OnWarningLog = (log) => Log.Warning(log);
            vlixHttpServer.Start();
        }
    }
}
