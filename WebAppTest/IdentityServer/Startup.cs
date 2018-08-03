using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DnsClient;
using IdentityServer.Dto;
using IdentityServer.Infrastructure;
using IdentityServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resilience;

namespace IdentityServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityServer()
                .AddExtensionGrantValidator<Authentication.SmsAuthCodeValidate>()
                .AddDeveloperSigningCredential()
                .AddInMemoryClients(Config.GetClients())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources());

            services.Configure<Dto.ServiceDisvoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            services.AddSingleton<IDnsQuery>(p =>
            {
                var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDisvoveryOptions>>().Value;
                return new LookupClient(serviceConfiguration.Consul.DnsEndpoint.ToIPEndPoint());
            });
            ////注册全局单例ResilienceClientFactory
            //services.AddSingleton(typeof(ResilienceClientFactory), sp =>
            //{
            //    var logger = sp.GetRequiredService<ILogger<ResilienceHttpClient>>();
            //    var httpcontextAccesser = sp.GetRequiredService<IHttpContextAccessor>();
            //    var retryCount = 5;
            //    var execptionCountAllowBeforeBreaking = 5;
            //    return new ResilienceClientFactory(logger, httpcontextAccesser, retryCount,
            //        execptionCountAllowBeforeBreaking);
            //});

            //注册全局httpClinet
            services.AddSingleton<HttpClient>(new HttpClient());

            ////注册全局httpClinet
            //services.AddSingleton<IHttpClient>(sp =>
            //{
            //    return sp.GetRequiredService<ResilienceClientFactory>().GetResilienceHttpClient();
            //});         
            services.AddScoped<IAuthCodeService, TestAuthCodeService>().AddScoped<IUserService, UserService>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseIdentityServer();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
