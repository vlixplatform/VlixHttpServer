using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Vlix.HttpServer
{
    public enum Scheme { http, https }
    public enum ActionType { AlternativeWWWDirectory, Redirect, Deny, ReverseProxy }

    
    public abstract class ReponseAction
    {
        public virtual string ShortName { get; }
        public virtual Task<ProcessRuleResult> ProcessAsync(string callerIP,Scheme scheme, string host, int port, string path, string pathAndQuery, NameValueCollection query, NameValueCollection headers, StaticFileProcessor parent)
        {
            return null;
        }
    }


    //public class ResponseAction
    //{
    //    [JsonConverter(typeof(StringEnumConverter))]
    //    public ActionType ActionType { get; set; } = ActionType.Redirect;
        
    //}
    public class DenyAction : ReponseAction
    {
        public override string ShortName { get { return "Deny"; } }
        public override Task<ProcessRuleResult> ProcessAsync(string callerIP, Scheme scheme, string host, int port, string path, string pathAndQuery, NameValueCollection query, NameValueCollection headers,StaticFileProcessor parent) 
        { 
            return Task.FromResult(new ProcessRuleResult() { IsSuccess = true, ActionType = ActionType.Deny,Message = "Request denied!" });
        }
    }


    public class RedirectAction : ReponseAction
    {
        public override string ShortName { get { return "Redirect"; } }
        public bool SetScheme { get; set; } = false;
        [JsonConverter(typeof(StringEnumConverter))]
        public Scheme? Scheme { get; set; } = null;
        public bool SetHostName { get; set; } = false;
        public string HostName { get; set; } = null;
        public bool SetPort { get; set; } = false;
        public int? Port { get; set; } = null;
        public bool SetPath { get; set; } = false;
        public string Path { get; set; } = null;

        public override Task<ProcessRuleResult> ProcessAsync(string callerIP, Scheme scheme, string host, int port, string path, string pathAndQuery, NameValueCollection query, NameValueCollection headers,StaticFileProcessor parent)
        {
            string rScheme; if (this.SetScheme) rScheme = (this.Scheme ?? Scheme).ToString(); else rScheme = scheme.ToString();
            string rHost; if (this.SetHostName) rHost = (this.HostName ?? host).ToString(); else rHost = host;
            string rPath; if (this.SetPath) rPath = (this.Path ?? path).ToString(); else rPath = path;
            string rPort; if (this.SetPort) rPort = (this.Port ?? port).ToString(); else rPort = port.ToString();
            string redirectURL = rScheme + "://" + rHost + ":" + rPort + rPath;
            return Task.FromResult(new ProcessRuleResult() { IsSuccess=true, ActionType=ActionType.Redirect, RedirectURL = redirectURL, Message = "Redirected to '" + redirectURL + "'" }); ;
        }
    }

    public class ReverseProxyAction : ReponseAction
    {
        public override string ShortName { get { return "RProxy"; } }
        public bool SetScheme { get; set; } = false;
        [JsonConverter(typeof(StringEnumConverter))]
        public Scheme? Scheme { get; set; } = null;
        public bool SetHostName { get; set; } = false;
        public string HostName { get; set; } = null;
        public bool SetPort { get; set; } = false;
        public int? Port { get; set; } = null;
        public bool SetPath { get; set; } = false;
        public bool UsePathVariable { get; set; } = false;
        public string PathAndQuery { get; set; } = null;
        public override async Task<ProcessRuleResult> ProcessAsync(string callerIP, Scheme scheme, string host, int port, string path, string pathAndQuery, NameValueCollection query, NameValueCollection headers,StaticFileProcessor parent)
        {
            string portStr = port.ToString();
            string pScheme; if (this.SetScheme) pScheme = (this.Scheme ?? Scheme).ToString(); else pScheme = scheme.ToString();
            string pHost; if (this.SetHostName) pHost = (this.HostName ?? host).ToString(); else pHost = host;
            string pPath = null;
            if (this.SetPath)
            {
                if (this.UsePathVariable)
                {
                    // {$Path|^.sdsdd$}
                    string newPath = this.PathAndQuery.Replace("%PATH%", this.PathAndQuery);
                    Regex regex = new Regex(@"\%[^\%]*\%", RegexOptions.IgnoreCase);
                    MatchCollection matches = regex.Matches(this.PathAndQuery);
                    foreach (Match match in matches)
                    {
                        string innerRegex = match.Value.Trim('%');
                        var regex2 = new Regex(innerRegex, RegexOptions.IgnoreCase);
                        string innerRegexResult = regex2.Match(pathAndQuery).Value;
                        newPath = newPath.Replace(match.Value, innerRegexResult);
                    }
                    pPath = newPath;
                }
                else pPath = path;
            }
            else pPath = path;
            string pPort; if (this.SetPort) pPort = (this.Port ?? port).ToString(); else pPort = portStr;
            string rProxyURL = pScheme + "://" + pHost + ":" + pPort + pPath;


            //AddOriginal Headers
            HttpClient httpClient = ((HttpServer)parent).HttpClient;
            var items = headers.AllKeys.SelectMany(headers.GetValues, (k, v) => new { key = k, value = v });
            string originalHost = null; 
            bool AddXFFor = true; bool AddXFProto = true; bool AddXFHost = true; bool AddXFPort = true;
            foreach (var item in items)
            {
                if (string.Equals(item.key, "Host")) { originalHost = item.value; continue; }// AddHost = false;
                if (string.Equals(item.key, "X-Forwarded-For")) AddXFFor = false;
                if (string.Equals(item.key, "X-Forwarded-Proto")) AddXFProto = false;
                if (string.Equals(item.key, "X-Forwarded-Host")) AddXFHost = false;
                if (string.Equals(item.key, "X-Forwarded-Port")) AddXFPort = false;
                httpClient.DefaultRequestHeaders.Add(item.key, item.value);
            }
            if (originalHost == null) originalHost = host;

            //Add Proxy Headers
            if (AddXFFor) httpClient.DefaultRequestHeaders.Add("X-Forwarded-For", callerIP);
            if (AddXFProto) httpClient.DefaultRequestHeaders.Add("X-Forwarded-Proto", pScheme);
            if (AddXFHost) httpClient.DefaultRequestHeaders.Add("X-Forwarded-Host", originalHost);
            if (AddXFPort) httpClient.DefaultRequestHeaders.Add("X-Forwarded-Port", portStr);


            HttpResponseMessage resp = await httpClient.GetAsync(rProxyURL); 
            Stream rProxyResult = await resp.Content.ReadAsStreamAsync();
            string contentType = null;
            if (resp.IsSuccessStatusCode)
                if (resp.Content.Headers.Contains("Content-Type")) contentType = resp.Content.Headers.GetValues("Content-Type").FirstOrDefault();
            if (resp.IsSuccessStatusCode)
            {
                return new ProcessRuleResult()
                {
                    IsSuccess = true,
                    ActionType = ActionType.ReverseProxy,
                    HttpStreamResult = new HTTPStreamResult(false, null, resp.StatusCode, contentType, rProxyResult, null, false),
                    Message = rProxyURL
                };
            }
            else
            {
                return new ProcessRuleResult() 
                {
                    IsSuccess = false, ActionType = ActionType.ReverseProxy, SendErrorResponsePage_HttpStatusCode = resp.StatusCode, 
                    LogLevel = LogLevel.Warning, Message = rProxyURL + " > " + resp.ReasonPhrase 
                };
            }
        }
    }    
    public class AlternativeWWWDirectoryAction : ReponseAction
    {
        public override string ShortName { get { return "AltWWWDir"; } }
        public string AlternativeWWWDirectory { get; set; } = null;
        public override async Task<ProcessRuleResult> ProcessAsync(string callerIP, Scheme scheme, string host, int port, string path, string pathAndQuery, NameValueCollection query, NameValueCollection headers,StaticFileProcessor parent)
        {
            HTTPStreamResult httpStreamResult = await parent.ProcessRequestAsync(callerIP, path, this.AlternativeWWWDirectory).ConfigureAwait(false);
            if ((int)httpStreamResult.HttpStatusCode >= 200 && (int)httpStreamResult.HttpStatusCode < 300)
            {
                return new ProcessRuleResult() { IsSuccess = true, ActionType = ActionType.AlternativeWWWDirectory, HttpStreamResult = httpStreamResult, Message = "Served '" + httpStreamResult.FileToRead + "'" };
            }
            else
            {
                return new ProcessRuleResult()
                {
                    IsSuccess = false,
                    ActionType = ActionType.AlternativeWWWDirectory,
                    SendErrorResponsePage_HttpStatusCode = httpStreamResult.HttpStatusCode,
                    LogLevel = LogLevel.Warning,
                    Message = httpStreamResult.ErrorMsg
                };            
            }
        }
    }


    public class ProcessRuleResult
    {
        public bool IsSuccess { get; set; } = true;
        public LogLevel LogLevel { get; set; } = LogLevel.Info;        
        public string Message { get; set; } = null;
        public HttpStatusCode SendErrorResponsePage_HttpStatusCode { get; set; } = HttpStatusCode.BadRequest;
        public bool ContinueNextRule { get; set; } = false;
        public ActionType ActionType { get; set; } = ActionType.AlternativeWWWDirectory;
        public HTTPStreamResult HttpStreamResult { get; set; } = null;
        public string RedirectURL { get; set; } = null;
    }

    public enum LogLevel { Info, Warning, Error}
}