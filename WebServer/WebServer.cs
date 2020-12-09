using Serilog;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Vlix.HttpServer;
namespace Vlix.WebServer
{
    public class WebServer
    {
        public WebServer()
        {

        }
        public HttpServer.HttpServer HttpServer { get; set; } = null;
        public HttpServerConfig HttpServerConfig { get; set; } = null;
        public static ConcurrentQueue<LogStruct> LogsCache = new ConcurrentQueue<LogStruct>();
        public async Task Start()
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    if (LogsCache.Count > 100) for (int n = 0; n < LogsCache.Count - 100; n++) LogsCache.TryDequeue(out _);
                    await Task.Delay(15000).ConfigureAwait(false);
                }
            });
            if (HttpServerConfig == null) { this.HttpServerConfig = new HttpServerConfig(); this.HttpServerConfig.LoadConfigFile(); }
            this.HttpServer = new HttpServer.HttpServer((new CancellationTokenSource()).Token, this.HttpServerConfig);
            this.HttpServer.OnErrorLog = (log) => { Log.Error(log); LogsCache.Enqueue(new LogStruct(log, LogLevel.Error)); };
            this.HttpServer.OnInfoLog = (log) => { Log.Information(log); LogsCache.Enqueue(new LogStruct(log, LogLevel.Info)); };
            this.HttpServer.OnWarningLog = (log) => { Log.Warning(log); LogsCache.Enqueue(new LogStruct(log, LogLevel.Warning)); };
            await this.HttpServer.StartAsync().ConfigureAwait(false);
        }
    }


}
