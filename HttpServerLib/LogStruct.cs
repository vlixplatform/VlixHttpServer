using System;
using Vlix.HttpServer;

namespace Vlix.HttpServerConfig
{
    public struct LogStruct
    {
        public LogStruct(string logEntry, LogLevel logLevel )
        {
            this.LogEntry = logEntry;
            this.LogLevel = logLevel;
            this.TimeStampInUTC = DateTime.UtcNow;
        }
        public string LogEntry { get; set; }
        public LogLevel LogLevel { get; set; }
        public DateTime TimeStampInUTC { get; set; }            
    }
}
