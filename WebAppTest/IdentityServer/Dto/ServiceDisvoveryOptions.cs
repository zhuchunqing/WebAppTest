﻿namespace IdentityServer.Dto
{
    public class ServiceDisvoveryOptions
    {
        public string UserServiceName { get; set; }

        public ConsulOptions Consul { get; set; }
    }
}
