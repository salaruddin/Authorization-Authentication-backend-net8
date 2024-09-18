using backend_net8.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_net8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        [Route("public-action")]
        public IActionResult PublicAction()
        {
            return Ok("action without authorization");
        }

        [HttpGet]
        [Route("user-access")]
        [Authorize(Roles =StaticUserRoles.USER)]
        public IActionResult UserAccess()
        {
            return Ok("user authorization");
        }

        [HttpGet]
        [Route("manager-access")]
        [Authorize(Roles = StaticUserRoles.MANAGER)]
        public IActionResult ManagerAccess()
        {
            return Ok("Manager authorization");
        }

        [HttpGet]
        [Route("admin-access")]
        [Authorize(Roles = StaticUserRoles.ADMIN)]
        public IActionResult AdminAccess()
        {
            return Ok("Admin authorization");
        }

        [HttpGet]
        [Route("owner-access")]
        [Authorize(Roles = StaticUserRoles.ADMIN)]
        public IActionResult OwnerAccess()
        {
            return Ok("Owner authorization");
        }
    }
}
