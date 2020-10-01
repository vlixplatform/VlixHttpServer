using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vlix
{
    class Program
    {
        static void Main(string[] args)
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Vlix\\HttpServer"; //This translates to => C:\ProgramData\Vlix\HttpServer
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(directory + "\\VlixHttpServer.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            VlixHttpServer vlixHttpServer = new VlixHttpServer(directory + "\\www", 8080, true);
            vlixHttpServer.OnError = (ex) => Log.Error(ex.ToString());
            vlixHttpServer.OnInfoLog = (log) => Log.Information(log);
            vlixHttpServer.OnWarningLog = (log) => Log.Warning(log);
            vlixHttpServer.Start();
        }
    }
}
