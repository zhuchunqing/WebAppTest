using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using WebAppTest.Models;

namespace WebAppTest.Controllers
{
    [Route("api/[controller]")]
    
    public class ValuesController : Controller
    {
        public UsersContext _userContext;
        public ValuesController(UsersContext userContext)
        {
            _userContext = userContext;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Json(await _userContext.Users.SingleOrDefaultAsync(w => w.Name == "xiaoma"));
        }
        /// <summary>
        /// 有用户信息则返回true 没有就新建信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [Route("check-or-create")]
        [HttpPost]
        public async Task<IActionResult> CheckOrCreate(string name)
        {
           var user= _userContext.Users.SingleOrDefault(w => w.Name == name);
            if (user==null)
            {
                user = new AppUser
                {
                    Name = name,
                };
                _userContext.Users.Add(user);
                await _userContext.SaveChangesAsync();
            }
            return Ok(user.Id);
        }
    }
}
