using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace Vlix
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }

    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        VlixHttpServer vlixHttpServer = null;
        CancellationTokenSource cancellationTokenSource;
        protected override void OnStart(string[] args)
        {
            string HttpPort = ConfigurationManager.AppSettings.Get("HttpPort");
            string HttpsPort = ConfigurationManager.AppSettings.Get("HttpsPort");
            string EnableCache = ConfigurationManager.AppSettings.Get("EnableCache");
            string WWWDirectory = ConfigurationManager.AppSettings.Get("WWWDirectory");
            string LogDirectory = ConfigurationManager.AppSettings.Get("LogDirectory");
            string OnlyCacheItemsLessThenMB = ConfigurationManager.AppSettings.Get("OnlyCacheItemsLessThenMB");
            string MaximumCacheSizeInMB = ConfigurationManager.AppSettings.Get("MaximumCacheSizeInMB");
            WWWDirectory = WWWDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            LogDirectory = LogDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .WriteTo.File(LogDirectory + "\\HttpServer.log", rollingInterval: RollingInterval.Day)
                   .CreateLogger();
            try
            {
                Directory.CreateDirectory(WWWDirectory);
                string[] myFiles = Directory.GetFiles(WWWDirectory);
                var AppDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                if (myFiles.FirstOrDefault(f => Path.GetFileName(f) == "index.html") == null) File.Copy(AppDir + "\\Sample\\index.html", WWWDirectory + "\\index.html");
                if (myFiles.FirstOrDefault(f => Path.GetFileName(f) == "test.html") == null) File.Copy(AppDir + "\\Sample\\test.html", WWWDirectory + "\\test.html");
                cancellationTokenSource = new CancellationTokenSource();
                vlixHttpServer = new VlixHttpServer(cancellationTokenSource.Token, WWWDirectory, Convert.ToInt32(HttpPort), Convert.ToInt32(HttpsPort), EnableCache.ToBool(), Convert.ToInt32(OnlyCacheItemsLessThenMB), Convert.ToInt32(MaximumCacheSizeInMB));
                vlixHttpServer.OnError = (ex) => Log.Error(ex.ToString());
                vlixHttpServer.OnInfoLog = (log) => Log.Information(log);
                vlixHttpServer.OnWarningLog = (log) => Log.Warning(log);
                vlixHttpServer.Start();
            }
            catch (Exception ex)
            {
                var AppDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                Log.Error(ex.ToString() + "\r\n\r\nApp Directory = " + AppDir);
            }
        }


        protected override void OnStop()
        {
            cancellationTokenSource.Cancel();
            vlixHttpServer?.Stop();
        }
    }
}
