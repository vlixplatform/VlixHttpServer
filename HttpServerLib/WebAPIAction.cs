using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Vlix.HttpServer
{
    public class WebAPIAction
    {
        public WebAPIAction(string path, Func<WebAPIActionInput,Task> action, string httpMethod = "Get")
        {
            this.HttpMethod = httpMethod;
            this.Path = path;
            this.Action = action;
        }
        public string HttpMethod { get; set; } = "Get";
        public string Path { get; set; } = null;
        public Func<WebAPIActionInput,Task> Action { get; set; }        
    }

    public class WebAPIActionInput
    {
        public WebAPIActionInput() { }
        public WebAPIActionInput(string httpMethod, string query, string requestBody, WebAPIAction webAPIAction, HttpListenerContext context, Action<string> onErrorLog)
        {
            this.WebAPIAction = webAPIAction;
            this.HttpMethod = httpMethod;
            this.onErrorLog = onErrorLog;
            query = query.TrimStart('?');
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string[] q1 = query.Split('&');
            foreach (string q2 in q1)
            {
                string[] q3 = q2.Split('=');
                if (q3.Length==2) this.Query.Add(q3[0], q3[1]);                
            }
            foreach (var h in context.Request.Headers.AllKeys) Headers.Add(h, context.Request.Headers[h]);
            this.RequestBody = requestBody;
            this.HttpListenerContext = context;
        }
        Action<string> onErrorLog = null;
        public WebAPIAction WebAPIAction { get; set; } = null;
        public string HttpMethod { get; set; } = "Get";
        public Dictionary<string, string> Query { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.OK;
        public HttpListenerContext HttpListenerContext { get; set; } = null;
        private MemoryStream CreateStream(string responseBody, HttpStatusCode httpStatusCode)
        {
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(responseBody ?? ""));
            this.HttpListenerContext.Response.ContentLength64 = stream.Length;
            this.HttpListenerContext.Response.ContentEncoding = Encoding.UTF8;
            this.HttpListenerContext.Response.StatusCode = (int)httpStatusCode;
            return stream;        
        }
        public async Task RespondWithJson(object responseObject, HttpStatusCode httpStatusCode = HttpStatusCode.OK, JsonSerializerSettings jsonSerializerSettings = null)
        {
            if (jsonSerializerSettings == null) jsonSerializerSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented };
            string responseBody = JsonConvert.SerializeObject(responseObject, jsonSerializerSettings);
            this.HttpListenerContext.Response.ContentType = "application/json";
            var mStream = CreateStream(responseBody, httpStatusCode);
            if (mStream.Length == 0) this.HttpListenerContext.Response.Close();
            else await CreateStream(responseBody, httpStatusCode).CopyToAsync(this.HttpListenerContext.Response.OutputStream).ConfigureAwait(false);
        }
        public async Task RespondWithText(string responseBody, HttpStatusCode httpStatusCode= HttpStatusCode.OK)
        {
            this.HttpListenerContext.Response.ContentType = "text/html";
            var mStream = CreateStream(responseBody, httpStatusCode);
            if (mStream.Length == 0) this.HttpListenerContext.Response.Close();
            else await CreateStream(responseBody, httpStatusCode).CopyToAsync(this.HttpListenerContext.Response.OutputStream).ConfigureAwait(false);
        }
        public async Task Respond(string responseBody, string contentType, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            this.HttpListenerContext.Response.ContentType = contentType;
            var mStream = CreateStream(responseBody, httpStatusCode);
            if (mStream.Length == 0) this.HttpListenerContext.Response.Close();
            else await mStream.CopyToAsync(this.HttpListenerContext.Response.OutputStream).ConfigureAwait(false);           
        }

        public Task RespondEmpty(HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            this.HttpListenerContext.Response.StatusCode = (int)httpStatusCode;
            this.HttpListenerContext.Response.ContentLength64 = 0;
            this.HttpListenerContext.Response.Close();
            return Task.FromResult<object>(null);
        }
    }

}