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
            var res = await this.SendAsync(requestMessage);
            return res;
        }

        public async Task<HttpStatusCode> RequestStatusOnlyAsync(HttpMethod httpMethod, string uRL, string username, string password, StringContent body = null)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, uRL);
            requestMessage.Headers.Add("Authorization", "Basic " + (username + ":" + password).ToBase64());
            if (body != null) requestMessage.Content = body;
            var res = await this.SendAsync(requestMessage);
            if (!res.IsSuccessStatusCode) throw new HttpRequestException("Request returned error (" + res.StatusCode + ") " + res.ReasonPhrase);
            return res.StatusCode;
        }
        public async Task<HttpStatusCode> RequestStatusOnlyAsync(HttpMethod httpMethod, string uRL, string username, string password, object jsonObject)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, uRL);
            requestMessage.Headers.Add("Authorization", "Basic " + (username + ":" + password).ToBase64());
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(jsonObject, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto }), Encoding.UTF8, "application/json");
            var res = await this.SendAsync(requestMessage);
            if (!res.IsSuccessStatusCode) throw new HttpRequestException("Request returned error (" + res.StatusCode + ") " + res.ReasonPhrase);
            return res.StatusCode;
        }
        public async Task<string> RequestStringAsync(HttpMethod httpMethod, string uRL, string username, string password, StringContent body = null)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, uRL);
            requestMessage.Headers.Add("Authorization", "Basic " + (username + ":" + password).ToBase64());
            if (body != null) requestMessage.Content = body;
            var res = await this.SendAsync(requestMessage);
            if (!res.IsSuccessStatusCode) throw new HttpRequestException("Request returned error (" + res.StatusCode + ") " + res.ReasonPhrase);
            string bodyStr = await res.Content.ReadAsStringAsync();
            return bodyStr;
        }
        public async Task<T> RequestJsonAsync<T>(HttpMethod httpMethod, string uRL, string username, string password, StringContent body = null)
            {
            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, uRL);
            requestMessage.Headers.Add("Authorization", "Basic " + (username + ":" + password).ToBase64());
            if (body != null) requestMessage.Content = body;
            var res = await this.SendAsync(requestMessage);
            if (!res.IsSuccessStatusCode) throw new HttpRequestException("Request returned error (" + res.StatusCode + ") " + res.ReasonPhrase);
            string bodyStr = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(bodyStr, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
        }

        public async Task<T> RequestJsonAsync<T>(HttpMethod httpMethod, string uRL, string username, string password, object jsonObject)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, uRL);
            requestMessage.Headers.Add("Authorization", "Basic " + (username + ":" + password).ToBase64());
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(jsonObject, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto }), Encoding.UTF8, "application/json");
            var res = await this.SendAsync(requestMessage);
            if (!res.IsSuccessStatusCode) throw new HttpRequestException("Request returned error (" + res.StatusCode + ") " + res.ReasonPhrase);
            string bodyStr = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(bodyStr, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
        }
    }
}
