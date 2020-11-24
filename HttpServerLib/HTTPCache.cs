using System;
using System.IO;

namespace Vlix
{
    public class HTTPCache
    {
        public HTTPCache(string fileToRead, MemoryStream memoryStream, DateTime lastModifiedTimeUTC, double contentLengthInKB)
        {
            this.FileToRead = fileToRead;
            this.MemoryStream = memoryStream;
            this.LastModifiedTimeUTC = lastModifiedTimeUTC;
            this.LastAccessTimeUTC = DateTime.UtcNow;
            this.RequestCount = 1;
            this.ContentLengthInKB = contentLengthInKB;
        }
        public string FileToRead { get; internal set; }
        public DateTime LastModifiedTimeUTC { get; set; }
        public DateTime LastAccessTimeUTC { get; set; }
        public int RequestCount { get; set; } = 0;
        public MemoryStream MemoryStream { get; set; }
        public double ContentLengthInKB { get; set; }

    }
}
