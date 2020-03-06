using Cache.Aspect.Service;
using Microsoft.AspNetCore.Mvc;

namespace Cache.Aspect.Controller
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
