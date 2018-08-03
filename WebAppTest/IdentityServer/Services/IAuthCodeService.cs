using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Services
{
    public interface IAuthCodeService
    {
        /// <summary>
        /// 根据手机号验证验证码
        /// </summary>
        /// <param name="name">姓名</param>
        /// <param name="authCode">手机号</param>
        /// <returns></returns>
        bool Validate(string name, string authCode);
    }
}
