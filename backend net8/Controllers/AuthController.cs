using backend_net8.Core.Constants;
using backend_net8.Core.DTOs.Auth;
using backend_net8.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_net8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;
        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        // seed roles to DB
        [HttpPost]
        [Route("seed-roles")]
        public async Task<IActionResult> SeedRoles()
        {
            var seedResult = await authService.SeedRolesAsync();
            return StatusCode(seedResult.StatusCode, seedResult.Message);
        }

        // register
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var registerResult = await authService.RegisterAsync(registerDto);
            return StatusCode(registerResult.StatusCode, registerResult.Message);
        }

        //login
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var loginResult = await authService.LoginAsync(loginDto);

            if (loginResult is null)
                return Unauthorized("Your credentials are invalid. Please contact to an Admin");

            return Ok(loginResult);
        }

        [HttpPost]
        [Route("update-role")]
        [Authorize(Roles = StaticUserRoles.OwnerAdmin)]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleDto updateRoleDto)
        {
            var updateRoleResult = await authService.UpdateRoleAsync(User, updateRoleDto);

            if (updateRoleResult.IsSucceed)
                return Ok(updateRoleResult);
            else
                return StatusCode(updateRoleResult.StatusCode, updateRoleResult.Message);
        }

        //getting data of a user from it's JWT
        [HttpPost]
        [Route("me")]
        public async Task<ActionResult<LoginServiceResponseDto>> Me([FromBody] MeDto meDto)
        {
            try
            {
                var me = await authService.MeAsync(meDto);
                if (me is not null)
                {
                    return Ok(me);
                }
                return Unauthorized("Invalid Token");
            }
            catch (Exception)
            {

                return Unauthorized("Invalid Token");
            }
        }

        //list of all users with details
        [HttpGet]
        [Route("users")]
        public async Task<ActionResult<UserInfoResult>> GetUsersList()
        {
            var users = await authService.GetUsersListAsync();
            return Ok(users);
        }

        //get user by username
        [HttpGet]
        [Route("users/{userName}")]
        public async Task<ActionResult<UserInfoResult>> GetUserDetailsByUserName([FromRoute] string userName)
        {
            var user = await authService.GetUserDetailsByUserNameAsync(userName);

            if(user is not null)
                return Ok(user);
            return NotFound("Username not found");
        }

        //get list of all usernames for send message
        [HttpGet]
        [Route("usernames")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserNameList()
        {
            var usernames = await authService.GetUsernamesListAsync();
            return Ok(usernames);
        }

    }
}
