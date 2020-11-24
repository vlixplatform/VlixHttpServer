using System.IO;
using System.Net;

namespace Vlix
{
    public class HTTPStreamResult
    {
        public HTTPStreamResult(bool obtainedFromCache, string fileToRead, HttpStatusCode httpStatusCode, string contentType, MemoryStream memoryStream, string errorMsg, bool FaviconError)
        {
            this.ObtainedFromCache = obtainedFromCache;
            this.FileToRead = fileToRead;
            this.HttpStatusCode = httpStatusCode;
            this.ContentType = contentType;
            this.MemoryStream = memoryStream;
            this.ErrorMsg = errorMsg;
            this.FaviconError = FaviconError;
        }
        public string FileToRead { get; set; } = null;
        public MemoryStream MemoryStream { get; set; } = null;
        public string ContentType { get; set; } = null;
        public string ErrorMsg { get; set; } = null;
        public bool FaviconError { get; set; } = false;
        public bool ObtainedFromCache { get; set; } = false;
        public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.OK;
    }
}
