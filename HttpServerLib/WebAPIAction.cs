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
        public WebAPIAction(string path, Action<WebAPIActionInput> action, string httpMethod = "Get")
        {
            this.HttpMethod = httpMethod;
            this.Path = path;
            this.Action = action;
        }
        public string HttpMethod { get; set; } = "Get";
        public string Path { get; set; } = null;
        public Action<WebAPIActionInput> Action { get; set; }        
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
                if (q3.Length==2) this.Input.Add(q3[0], q3[1]);                
            }
            this.RequestBody = requestBody;
            this.HttpListenerContext = context;
        }
        Action<string> onErrorLog = null;
        public WebAPIAction WebAPIAction { get; set; } = null;
        public string HttpMethod { get; set; } = "Get";
        public Dictionary<string, string> Input { get; set; } = new Dictionary<string, string>();
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
            try
            {
                if (jsonSerializerSettings == null) jsonSerializerSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented };
                string responseBody = JsonConvert.SerializeObject(responseObject, jsonSerializerSettings);
                this.HttpListenerContext.Response.ContentType = "application/json";
                await CreateStream(responseBody, httpStatusCode).CopyToAsync(this.HttpListenerContext.Response.OutputStream).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                onErrorLog?.Invoke(ex.ToString());
                await RespondWithText(ex.ToString(), HttpStatusCode.InternalServerError);
            }
        }
        public async Task RespondWithText(string responseBody, HttpStatusCode httpStatusCode= HttpStatusCode.OK)
        {
            try
            {
                this.HttpListenerContext.Response.ContentType = "text/html";
                await CreateStream(responseBody, httpStatusCode).CopyToAsync(this.HttpListenerContext.Response.OutputStream).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                onErrorLog?.Invoke(ex.ToString());
                await Respond(ex.ToString(), "text/html",HttpStatusCode.InternalServerError);
            }
        }
        public async Task Respond(string responseBody, string contentType, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            try
            { 
                this.HttpListenerContext.Response.ContentType = contentType;
                await CreateStream(responseBody, httpStatusCode).CopyToAsync(this.HttpListenerContext.Response.OutputStream).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                onErrorLog?.Invoke(ex.ToString());
                await RespondWithText(ex.ToString(), HttpStatusCode.InternalServerError);
            }
        }
    }

}