using System.IO;
using System.Net;

namespace  Vlix.HttpServer
{
    public class HTTPStreamResult
    {
        public HTTPStreamResult(bool obtainedFromCache, string fileToRead, HttpStatusCode httpStatusCode, string contentType, Stream stream, string errorMsg, bool FaviconError)
        {
            this.ObtainedFromCache = obtainedFromCache;
            this.FileToRead = fileToRead;
            this.HttpStatusCode = httpStatusCode;
            this.ContentType = contentType;
            this.Stream = stream;
            this.ErrorMsg = errorMsg;
            this.FaviconError = FaviconError;
        }
        public string FileToRead { get; set; } = null;
        public Stream Stream { get; set; } = null;
        public string ContentType { get; set; } = null;
        public string ErrorMsg { get; set; } = null;
        public bool FaviconError { get; set; } = false;
        public bool ObtainedFromCache { get; set; } = false;
        public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.OK;
    }
}
