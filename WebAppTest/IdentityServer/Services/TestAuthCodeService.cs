using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Services
{
    public class TestAuthCodeService:IAuthCodeService
    {
        public bool Validate(string name, string authCode)
        {
            return true;
        }
    }
}
