using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;

namespace Nop.Plugin.Baramjk.Framework.Services.Networks
{
    public class HttpClientHelper
    {
        private static readonly JsonSerializerSettings _serializerOptions
            = new() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        private readonly HttpClient _client;

        public HttpClientHelper()
        {
            var cookieContainer = new CookieContainer();
            var clientHandler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            _client = new HttpClient(clientHandler);
        }

        public void AddHeader(string name, string value)
        {
            if (_client.DefaultRequestHeaders.Contains(name))
                return;
            
            _client.DefaultRequestHeaders.Add(name, value);
        }

        public void AddUserAgent(ProductInfoHeaderValue infoHeaderValue)
        {
            _client.DefaultRequestHeaders.UserAgent.Add(infoHeaderValue);
        }

        public async Task<HttpResponse> PostFormDataAsync(string url, params KeyValuePair<string, string>[] formData)
        {
            var content = new FormUrlEncodedContent(formData);

            var responseMessage = await _client.PostAsync(url, content);
            var response = CreateResponse(responseMessage);
            return response;
        }

        public async Task<HttpResponse<TResponse>> PostFormDataAsync<TResponse>(string url,
            params KeyValuePair<string, string>[] formData)
        {
            var content = new FormUrlEncodedContent(formData);

            var responseMessage = await _client.PostAsync(url, content);
            var response = await CreateResponseAsync<TResponse>(responseMessage);
            return response;
        }

        public async Task<HttpResponse> PostJsonAsync<TData>(string url, TData data)
        {
            var json = JsonConvert.SerializeObject(data, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var responseMessage = await _client.PostAsync(url, content);
            var response = CreateResponse(responseMessage);
            return response;
        }

        public async Task<HttpResponse<TResponse>> PostJsonAsync<TData, TResponse>(string url, TData data)
        {
            var json = JsonConvert.SerializeObject(data, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var responseMessage = await _client.PostAsync(url, content);
            var response = await CreateResponseAsync<TResponse>(responseMessage);
            return response;
        }

        public async Task<HttpResponse> PutJsonAsync<TData>(string url, TData data)
        {
            var json = JsonConvert.SerializeObject(data, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var responseMessage = await _client.PutAsync(url, content);
            var response = CreateResponse(responseMessage);
            return response;
        }

        public async Task<HttpResponse<TResponse>> PutJsonAsync<TData, TResponse>(string url, TData data)
        {
            var json = JsonConvert.SerializeObject(data, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var responseMessage = await _client.PutAsync(url, content);
            var response = await CreateResponseAsync<TResponse>(responseMessage);
            return response;
        }

        public async Task<HttpResponse> PatchAsync<TData>(string url, TData data)
        {
            var json = JsonConvert.SerializeObject(data, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var responseMessage = await _client.PatchAsync(url, content);
            var response = CreateResponse(responseMessage);
            return response;
        }

        public async Task<HttpResponse<TResponse>> PatchAsync<TData, TResponse>(string url, TData data)
        {
            var json = JsonConvert.SerializeObject(data, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var responseMessage = await _client.PutAsync(url, content);
            var response = await CreateResponseAsync<TResponse>(responseMessage);
            return response;
        }

        public async Task<HttpResponse<TResponse>> GetAsync<TResponse>(string url,
            params KeyValuePair<string, string>[] parameters)
        {
            url = CreateUrlWithQueryString(url, parameters);
            var responseMessage = await _client.GetAsync(url);
            var response = await CreateResponseAsync<TResponse>(responseMessage);
            return response;
        }

        public async Task<HttpResponse> GetAsync(string url, params KeyValuePair<string, string>[] parameters)
        {
            url = CreateUrlWithQueryString(url, parameters);
            var responseMessage = await _client.GetAsync(url);
            var response = CreateResponse(responseMessage);
            return response;
        }

        private static string CreateUrlWithQueryString(string url, KeyValuePair<string, string>[] parameters)
        {
            var qb = new QueryBuilder();
            if (parameters != null && parameters.Length > 0)
            {
                foreach (var item in parameters)
                    qb.Add(item.Key, item.Value);

                if (url.EndsWith("\\") == false)
                    url += "\\";

                url += qb.ToString();
            }

            return url;
        }

        private static HttpResponse CreateResponse(HttpResponseMessage result)
        {
            if (result == null)
                return HttpResponse.NoContent;

            var networkResponse = new HttpResponse
            {
                StatusCode = result.StatusCode,
                ReasonPhrase = result.ReasonPhrase
            };

            return networkResponse;
        }

        private static async Task<HttpResponse<TResponse>> CreateResponseAsync<TResponse>(HttpResponseMessage result)
        {
            if (result == null)
                return HttpResponse<TResponse>.NoContent;

            var resultContent = await result.Content.ReadAsStringAsync();
            var response = DeserializeObject<TResponse>(resultContent);
            var networkResponse = new HttpResponse<TResponse>
            {
                Body = response,
                StatusCode = result.StatusCode,
                ReasonPhrase = result.ReasonPhrase
            };

            return networkResponse;
        }

        private static TResponse DeserializeObject<TResponse>(string result)
        {
            try
            {
                if (string.IsNullOrEmpty(result))
                    return default;

                var data = JsonConvert.DeserializeObject<TResponse>(result);
                return data;
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}