using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Services
{
    public interface IUserService
    {
        /// <summary>
        /// 检查手机号
        /// </summary>
        /// <param name="name"></param>
        Task<int> CheckOrCreate(string name);
    }
}
