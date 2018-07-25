using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;

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
    }
}
