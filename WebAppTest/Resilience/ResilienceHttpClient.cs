using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Wrap;

namespace Resilience
{
    public class ResilienceHttpClient:IHttpClient
    {

        private readonly HttpClient _httpClient;
        private readonly Func<string, IEnumerable<Policy>> _poliyCreator;
        private readonly ConcurrentDictionary<string, PolicyWrap> _policyWrappers;
        private readonly ILogger<ResilienceHttpClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ResilienceHttpClient(Func<string, IEnumerable<Policy>> poliyCreator ,ILogger<ResilienceHttpClient> logger,IHttpContextAccessor httpContextAccessor)
        {
            _httpClient=new HttpClient();
            _policyWrappers = new ConcurrentDictionary<string, PolicyWrap>();
            _poliyCreator = poliyCreator;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string url, T item, string authorizationToken,
            string requestId = null,
            string authorizationMethod = "Bearer")
        {
            var requestMessage = CreateHttpRequestMessage(HttpMethod.Post, url, item);

            return await DoPostAsync(HttpMethod.Post, url, requestMessage, authorizationToken, requestId, authorizationMethod);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> from,
            string authorizationToken, string requestId = null,
            string authorizationMethod = "Bearer")
        {
            var requestMessage= CreateHttpRequestMessage(HttpMethod.Post, url, from);
            return await DoPostAsync(HttpMethod.Post, url, requestMessage, authorizationToken, requestId, authorizationMethod);
        }


        private Task<HttpResponseMessage> DoPostAsync(HttpMethod method,string url, HttpRequestMessage requestMessage, string authorizationToken,
            string requestId = null,
            string authorizationMethod = "Bearer")
        {
            if (method != HttpMethod.Post && method != HttpMethod.Put)
            {
                throw new ArgumentException("error",nameof(method));
            }
            var origin = GetOriginFromUrl(url);
            return HttpInvoker(origin, async () =>
            {
                //var requestMessage = new HttpRequestMessage(method, url);

                //requestMessage.Content = new StringContent(JsonConvert.SerializeObject(item),Encoding.UTF8,"application/json");
                SetAuthorizationHeader(requestMessage);
                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization=new AuthenticationHeaderValue(authorizationMethod,authorizationToken);
                }
                if (requestId != null)
                {
                    requestMessage.Headers.Add("x-requestid",requestId);
                }
                var response = await _httpClient.SendAsync(requestMessage);
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }
                return response;
            });
        }

        private string GetOrigin()
        {
            return "";
        }

        private async Task<T> HttpInvoker<T>(string origin, Func<Task<T>> action)
        {
            var normalizedOrigin = NormalizeOrigin(origin);
            if (!_policyWrappers.TryGetValue(normalizedOrigin, out PolicyWrap policyWrap))
            {
                policyWrap = Policy.WrapAsync(_poliyCreator(normalizedOrigin).ToArray());
                _policyWrappers.TryAdd(normalizedOrigin, policyWrap);
            }
            return await policyWrap.ExecuteAsync(action, new Context(normalizedOrigin));
        }

        public HttpRequestMessage CreateHttpRequestMessage<T>(HttpMethod method,string url,T item)
        {
            return new HttpRequestMessage(method, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json")
            };
        }

        public HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string url, string item,Dictionary<string,string> form)
        {
            //var requestMessage = new HttpRequestMessage(method, url);
            //requestMessage.Content=new FormUrlEncodedContent(form);
            //return requestMessage;
            return new HttpRequestMessage(method, url)
            {
                Content = new FormUrlEncodedContent(form)
            };
        }

        private static string NormalizeOrigin(string origin)
        {
            return origin?.Trim()?.ToLower();
        }

        private static string GetOriginFromUrl(string uri)
        {
            var url=new Uri(uri);
            var origin = $"{url.Scheme}://{url.DnsSafeHost}:{url.Port}";
            return origin;
        }

        private void SetAuthorizationHeader(HttpRequestMessage requestMessage)
        {
            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                requestMessage.Headers.Add("Authorization",new List<string>(){authorizationHeader});
            }
        }

    }
}
