using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Resilience;

namespace IdentityServer.Infrastructure
{
    public class ResilienceClientFactory
    {
        private readonly Func<string, IEnumerable<Policy>> _policyCreator;
        private readonly ConcurrentDictionary<string, Policy> _policyWrappers;
        private ILogger<ResilienceHttpClient> _logger;
        private IHttpContextAccessor _httpContextAccessor;
        /// <summary>
        /// 重试次数
        /// </summary>
        private int _retryCount;
        /// <summary>
        /// 运行的异常次数
        /// </summary>
        private int _exceptionCountAllowedBeforeBreaking;

        public ResilienceClientFactory(ILogger<ResilienceHttpClient> logger, IHttpContextAccessor httpContextAccessor,int retryCount,int exceptionCountAllowedBeforeBreaking)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public ResilienceHttpClient GetResilienceHttpClient()=> new ResilienceHttpClient(origin=> CreatePolicy(origin),_logger,_httpContextAccessor);

        private Policy[] CreatePolicy(string origin)
        {
            return new Policy[]
            {
                Policy.Handle<HttpRequestException>().WaitAndRetry(_retryCount,retryAttempt=>TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        var msg = $"第{retryCount}次重试"+
                        $"of{context.PolicyKey}"+
                        $"at {context.ExecutionKey}"+
                        $"due to:{exception}";
                       _logger.LogWarning(msg);
                       _logger.LogDebug(msg);

                    }),
                Policy.Handle<HttpRequestException>().CircuitBreakerAsync(
                    _exceptionCountAllowedBeforeBreaking,
                    TimeSpan.FromMinutes(1),
                    (exception, duration) =>
                    {
                        _logger.LogTrace("熔弹器打开关闭");
                    }, () =>
                    {
                        _logger.LogTrace("Circuit breaker opend");
                    }
                )
            };
        }
    }
}
