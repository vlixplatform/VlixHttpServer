using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Vlix.HttpServer
{
    public enum Scheme { http, https }
    public enum ActionType { AlternativeWWWDirectory, Redirect, Deny, ReverseProxy }

    public interface IReponseAction
    {
        [JsonIgnore]
        string ActionName { get; }
        Task<ProcessResult> ProcessAsync(string callerIP,Scheme scheme, string host, int port, string path, HttpListenerContext context, HttpServer parent);
    }


    //public class ResponseAction
    //{
    //    [JsonConverter(typeof(StringEnumConverter))]
    //    public ActionType ActionType { get; set; } = ActionType.Redirect;
        
    //}

    public class DenyAction : IReponseAction
    {
        public string ActionName { get { return "Deny"; } }
        public Task<ProcessResult> ProcessAsync(string callerIP, Scheme scheme, string host, int port, string path, HttpListenerContext context, HttpServer parent) { return Task.FromResult(new ProcessResult() { Message="Request denied!" });}
    }

    public class RedirectAction : IReponseAction
    {
        public string ActionName { get { return "Redirect"; } }

        [JsonConverter(typeof(StringEnumConverter))]
        public Scheme? Scheme { get; set; } = null;
        public string HostName { get; set; } = null;
        public int? Port { get; set; } = null;
        public string Path { get; set; } = null;

        public Task<ProcessResult> ProcessAsync(string callerIP, Scheme scheme, string host, int port, string path, HttpListenerContext context, HttpServer parent)
        {
            string redirectScheme = (this.Scheme ?? scheme).ToString();
            string redirectHost = this.HostName ?? host;
            int redirectPort = this.Port ?? port;
            string redirectPath = this.Path ?? path;
            string redirectURL = redirectScheme + "://" + redirectHost + ":" + redirectPort + redirectPath;
            context.Response.Redirect(redirectURL);
            return Task.FromResult(new ProcessResult() { Message="Redirected to '" + redirectURL + "'" });;
        }
    }

    public class ReverseProxyAction : IReponseAction
    {
        public string ActionName { get { return "Alternative WWW Directory"; } }
        public string AlternativeWWWDirectory { get; set; } = null;

        public Task<ProcessResult> ProcessAsync(string callerIP, Scheme scheme, string host, int port, string path, HttpListenerContext context, HttpServer parent)
        {
            throw new NotImplementedException();
            //return Task.FromResult(new ProcessResult() { LogLevel = LogLevel.Warning, Message = "Denied!" });
        }
    }    
    public class AlternativeWWWDirectoryAction : IReponseAction
    {
        public string ActionName { get { return "Reverse Proxy"; } }
        public string AlternativeWWWDirectory { get; set; } = null;

        public async Task<ProcessResult> ProcessAsync(string callerIP, Scheme scheme, string host, int port, string path, HttpListenerContext context, HttpServer parent)
        {
            HTTPStreamResult httpStreamResult = await parent.ProcessRequestAsync(callerIP, path, this.AlternativeWWWDirectory).ConfigureAwait(false);
            parent.OnHTTPStreamResult?.Invoke(httpStreamResult);
            if (httpStreamResult.HttpStatusCode == HttpStatusCode.OK)
            {
                await parent.SendToOutputAsync(httpStreamResult, context);
                return new ProcessResult() { Message = "Served '" + httpStreamResult.FileToRead + "'" };
            }
            else
            {
                if (httpStreamResult.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    return new ProcessResult() { SendErrorResponsePage = true, SendErrorResponsePage_HttpStatusCode = HttpStatusCode.NotFound, LogLevel = LogLevel.Warning, Message = httpStreamResult.FileToRead + "' does not exist" };
                }
                return new ProcessResult() { SendErrorResponsePage = true, SendErrorResponsePage_HttpStatusCode = httpStreamResult.HttpStatusCode, LogLevel = LogLevel.Error, Message = httpStreamResult.ErrorMsg };                
            }

        }
    }


    public class ProcessResult
    {
        public bool Log { get; set; } = true;
        [JsonConverter(typeof(StringEnumConverter))]
        public LogLevel LogLevel { get; set; } = LogLevel.Info;        
        public string Message { get; set; } = null;
        public bool SendErrorResponsePage { get; set; } = false;
        public HttpStatusCode SendErrorResponsePage_HttpStatusCode { get; set; } = HttpStatusCode.BadRequest;
        public bool ContinueNextRule { get; set; } = false;
    }

    public enum LogLevel { Info, Warning, Error}
}