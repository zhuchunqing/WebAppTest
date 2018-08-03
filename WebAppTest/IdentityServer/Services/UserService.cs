using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.Extensions.Options;
using Resilience;

namespace IdentityServer.Services
{
    public class UserService:IUserService
    {
        //public string _userServiceUrl = "http://localhost:85";
        public string _userServiceUrl;
        private HttpClient _httpClient;
        public UserService(HttpClient httpClient,IDnsQuery dnsQuery,IOptions<Dto.ServiceDisvoveryOptions> serviceDiscoveryOptions)
        {
            _httpClient = httpClient;
            var address = dnsQuery.ResolveService("service.consul", serviceDiscoveryOptions.Value.UserServiceName);
            var addressList = address.First().AddressList;
            var host = addressList.Any() ? addressList.First().ToString() : address.First().HostName;
            var port = address.First().Port;
            _userServiceUrl = $"http://{host}:{port}";
        }

        public async Task<int> CheckOrCreate(string name)
        {
            var form = new Dictionary<string, string>
            {
                {
                    "name", name
                }
            };
            var content = new FormUrlEncodedContent(form);
            try
            {
                var response = await _httpClient.PostAsync(_userServiceUrl + "/api/Values/check-or-create", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var userId = await response.Content.ReadAsStringAsync();
                    int.TryParse(userId, out int intUserId);
                    return intUserId;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            } 
            return 0;

        }
    }
}
