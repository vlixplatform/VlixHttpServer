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

        VlixHttpServer vlixHttpServer;
        protected override void OnStart(string[] args)
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
                .WriteTo.File(LogDirectory + "\\HttpServer.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            vlixHttpServer = new VlixHttpServer(WWWDirectory, Convert.ToInt32(Port), EnableCache.ToBool());
            vlixHttpServer.OnError = (ex) => Log.Error(ex.ToString());
            vlixHttpServer.OnInfoLog = (log) => Log.Information(log);
            vlixHttpServer.OnWarningLog = (log) => Log.Warning(log);
            vlixHttpServer.Start();
        }

        protected override void OnStop()
        {
            vlixHttpServer?.Stop();
        }
    }
}
