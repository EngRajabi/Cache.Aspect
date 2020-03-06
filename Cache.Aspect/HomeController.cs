using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Cache.Aspect
{
    public class HomeController : ControllerBase
    {
        private readonly ITestService _testService;

        public HomeController(ITestService testService)
        {
            _testService = testService;
        }

        [HttpPost]
        public IActionResult GetName([FromBody]Param1 param)
        {
            return Ok(_testService.GetName(param));
        }
    }
}
