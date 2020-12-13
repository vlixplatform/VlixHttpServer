using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Vlix.HttpServer
{
    public class OPHttpClient:HttpClient
    {
        public async Task<HttpResponseMessage> RequestAsync(HttpMethod httpMethod, string uRL, string username, string password, StringContent body = null)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, uRL);
            requestMessage.Headers.Add("Authorization", "Basic " + (username + ":" + password).ToBase64());
            if (body != null) requestMessage.Content = body;
            return await this.SendAsync(requestMessage);
        }

        public async Task<HttpStatusCode> RequestStatusOnlyAsync(HttpMethod httpMethod, string uRL, string username, string password, StringContent body = null)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, uRL);
            requestMessage.Headers.Add("Authorization", "Basic " + (username + ":" + password).ToBase64());
            if (body != null) requestMessage.Content = body;
            var res = await this.SendAsync(requestMessage);            
            return res.StatusCode;
        }
        public async Task<string> RequestStringAsync(HttpMethod httpMethod, string uRL, string username, string password, StringContent body = null)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, uRL);
            requestMessage.Headers.Add("Authorization", "Basic " + (username + ":" + password).ToBase64());
            if (body != null) requestMessage.Content = body;
            var res = await this.SendAsync(requestMessage);
            string bodyStr = await res.Content.ReadAsStringAsync();
            return bodyStr;
        }
        public async Task<T> RequestJsonAsync<T>(HttpMethod httpMethod, string uRL, string username, string password, StringContent body = null)
            {
            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, uRL);
            requestMessage.Headers.Add("Authorization", "Basic " + (username + ":" + password).ToBase64());
            if (body != null) requestMessage.Content = body;
            var res = await this.SendAsync(requestMessage);
            string bodyStr = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(bodyStr);
        }
    }
}
