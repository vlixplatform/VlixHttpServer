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
        Task<ProcessResult> ProcessAsync(string callerIP,Scheme scheme, string host, int port, string path, StaticFileProcessor parent);
    }


    //public class ResponseAction
    //{
    //    [JsonConverter(typeof(StringEnumConverter))]
    //    public ActionType ActionType { get; set; } = ActionType.Redirect;
        
    //}

    public class DenyAction : IReponseAction
    {
        public Task<ProcessResult> ProcessAsync(string callerIP, Scheme scheme, string host, int port, string path, StaticFileProcessor parent) 
        { 
            return Task.FromResult(new ProcessResult() { ActionType = ActionType.Deny,Message = "Request denied!" });
        }
    }


    public class RedirectAction : IReponseAction
    {

        [JsonConverter(typeof(StringEnumConverter))]
        public bool RedirectScheme { get; set; } = false;
        public Scheme? Scheme { get; set; } = null;
        public bool RedirectHostName { get; set; } = false;
        public string HostName { get; set; } = null;
        public bool RedirectPort { get; set; } = false;
        public int? Port { get; set; } = null;
        public bool RedirectPath { get; set; } = false;
        public string Path { get; set; } = null;

        public Task<ProcessResult> ProcessAsync(string callerIP, Scheme scheme, string host, int port, string path, StaticFileProcessor parent)
        {
            string rScheme; if (this.RedirectScheme) rScheme = (this.Scheme ?? Scheme).ToString(); else rScheme = scheme.ToString();
            string rHost; if (this.RedirectHostName) rHost = (this.HostName ?? host).ToString(); else rHost = host;
            string rPath; if (this.RedirectPath) rPath = (this.Path ?? path).ToString(); else rPath = path;
            string rPort; if (this.RedirectPort) rPort = (this.Port ?? port).ToString(); else rPort = port.ToString();
            string redirectURL = rScheme + "://" + rHost + ":" + rPort + rPath;
            return Task.FromResult(new ProcessResult() { ActionType=ActionType.Redirect, RedirectURL = redirectURL, Message = "Redirected to '" + redirectURL + "'" }); ;
        }
        //public Task<ProcessResult> ProcessAsync(string callerIP, Scheme scheme, string host, int port, string path, StaticFileProcessor parent)
        //{
        //    string rScheme; if (this.RedirectScheme) rScheme = (this.Scheme ?? Scheme).ToString(); else rScheme = scheme.ToString();
        //    string rHost; if (this.RedirectHostName) rHost = (this.HostName ?? host).ToString(); else rHost = host;
        //    string rPath; if (this.RedirectPath) rPath = (this.Path ?? path).ToString(); else rPath = path;
        //    string rPort; if (this.RedirectPort) rPort = (this.Port ?? port).ToString(); else rPort = port.ToString();            
        //    string redirectURL = rScheme + "://" + rHost + ":" + rPort + rPath;
        //    context.Response.Redirect(redirectURL);
        //    return Task.FromResult(new ProcessResult() { Message="Redirected to '" + redirectURL + "'" });;
        //}

    }

    public class ReverseProxyAction : IReponseAction
    {
        public Task<ProcessResult> ProcessAsync(string callerIP, Scheme scheme, string host, int port, string path, StaticFileProcessor parent)
        {
            throw new NotImplementedException();
            //return Task.FromResult(new ProcessResult() { LogLevel = LogLevel.Warning, Message = "Denied!" });
        }
    }    
    public class AlternativeWWWDirectoryAction : IReponseAction
    {
        public string AlternativeWWWDirectory { get; set; } = null;
        public async Task<ProcessResult> ProcessAsync(string callerIP, Scheme scheme, string host, int port, string path, StaticFileProcessor parent)
        {
            HTTPStreamResult httpStreamResult = await parent.ProcessRequestAsync(callerIP, path, this.AlternativeWWWDirectory).ConfigureAwait(false);
            if (httpStreamResult.HttpStatusCode == HttpStatusCode.OK)
            {
                return new ProcessResult() { ActionType = ActionType.AlternativeWWWDirectory, AlternativeWWWDirectoryHttpStreamResult = httpStreamResult, Message = "Served '" + httpStreamResult.FileToRead + "'" };
            }
            else
            {
                if (httpStreamResult.HttpStatusCode == HttpStatusCode.NotFound) 
                    return new ProcessResult() { ActionType=ActionType.AlternativeWWWDirectory, SendErrorResponsePage = true, SendErrorResponsePage_HttpStatusCode = HttpStatusCode.NotFound, LogLevel = LogLevel.Warning, Message = httpStreamResult.FileToRead + "' does not exist" };
                return new ProcessResult() { ActionType = ActionType.AlternativeWWWDirectory, SendErrorResponsePage = true, SendErrorResponsePage_HttpStatusCode = httpStreamResult.HttpStatusCode, LogLevel = LogLevel.Error, Message = httpStreamResult.ErrorMsg };
            }
        }
        //public async Task<ProcessResult> ProcessAsync(string callerIP, Scheme scheme, string host, int port, string path, HttpListenerContext context, HttpServer parent)
        //{
        //    HTTPStreamResult httpStreamResult = await parent.ProcessRequestAsync(callerIP, path, this.AlternativeWWWDirectory).ConfigureAwait(false);
        //    parent.OnHTTPStreamResult?.Invoke(httpStreamResult);
        //    if (httpStreamResult.HttpStatusCode == HttpStatusCode.OK)
        //    {
        //        await parent.SendToOutputAsync(httpStreamResult, context);
        //        return new ProcessResult() { Message = "Served '" + httpStreamResult.FileToRead + "'" };
        //    }
        //    else
        //    {
        //        if (httpStreamResult.HttpStatusCode == HttpStatusCode.NotFound)
        //        {
        //            return new ProcessResult() { SendErrorResponsePage = true, SendErrorResponsePage_HttpStatusCode = HttpStatusCode.NotFound, LogLevel = LogLevel.Warning, Message = httpStreamResult.FileToRead + "' does not exist" };
        //        }
        //        return new ProcessResult() { SendErrorResponsePage = true, SendErrorResponsePage_HttpStatusCode = httpStreamResult.HttpStatusCode, LogLevel = LogLevel.Error, Message = httpStreamResult.ErrorMsg };
        //    }
        //}
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

        public ActionType ActionType { get; set; } = ActionType.AlternativeWWWDirectory;
        public HTTPStreamResult AlternativeWWWDirectoryHttpStreamResult { get; set; } = null;
        public string RedirectURL { get; set; } = null;

    }

    public enum LogLevel { Info, Warning, Error}
}