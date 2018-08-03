using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Services;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace IdentityServer.Authentication
{
    public class SmsAuthCodeValidate:IExtensionGrantValidator
    {
        private readonly IAuthCodeService _authCodeService;
        private readonly IUserService _userService;
        public string GrantType => "sms_auth_code";

        public SmsAuthCodeValidate(IAuthCodeService authCodeService,IUserService userService)
        {
            _userService = userService;
            _authCodeService = authCodeService;
        }

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var name = context.Request.Raw["name"];
            var code = context.Request.Raw["auth_code"];
            var errorValidationResult=new GrantValidationResult(TokenRequestErrors.InvalidGrant);
            if (string.IsNullOrWhiteSpace(name)|| string.IsNullOrWhiteSpace(code))
            {
                context.Result = errorValidationResult;
            }
            if(!_authCodeService.Validate(name, code))
            {
                context.Result = errorValidationResult;
                return ;
            }
            var userid = await _userService.CheckOrCreate(name);
            if (userid<=0)
            {
                context.Result = errorValidationResult;
                return;
            }
            context.Result=new GrantValidationResult(userid.ToString(),GrantType);
        }
    }
}
