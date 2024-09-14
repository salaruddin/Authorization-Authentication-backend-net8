using backend_net8.Core.Constants;
using backend_net8.Core.DTOs.Auth;
using backend_net8.Core.DTOs.General;
using backend_net8.Core.Entities;
using backend_net8.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend_net8.Core.Services
{
    public class AuthService : IAuthService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogService _logService;
        private readonly IConfiguration _configuration;


        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogService logService, IConfiguration configuration, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logService = logService;
            _configuration = configuration;
            _signInManager = signInManager;
        }

        public async Task<GeneralServiceResponseDto> SeedRolesAsync()
        {

            var isOwnerRoleExist = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN);
            // i simply check only one role (owner) if this exist others are existing
            if (isOwnerRoleExist)
            {
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = true,
                    StatusCode = 200,
                    Message = "Roles are already seeded"
                };
            }

            try
            {
                await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.OWNER));
                await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.ADMIN));
                await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.MANAGER));
                await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.USER));

                return new GeneralServiceResponseDto()
                {
                    IsSucceed = true,
                    StatusCode = 201,
                    Message = "Roles seeding done successfully"
                };
            }
            catch (Exception ex)
            {
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }


        }

        public async Task<GeneralServiceResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (registerDto == null)
            {
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 404,
                    Message = "Invaild User input"
                };
            }

            ApplicationUser user = await _userManager.FindByNameAsync(registerDto.UserName);
            if (user is null)
            {
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 409,
                    Message = "User is already exists"
                };
            }

            try
            {
                var newUser = new ApplicationUser()
                {
                    UserName = registerDto.UserName,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Email = registerDto.Email,
                    Address = registerDto.Address,
                };
                var result = await _userManager.CreateAsync(newUser, registerDto.Password);

                if (!result.Succeeded)
                {
                    return new GeneralServiceResponseDto()
                    {
                        IsSucceed = false,
                        StatusCode = 400,
                        Message = result.Errors.ToString()
                    };
                }
                await _userManager.AddToRoleAsync(newUser, StaticUserRoles.USER);
                await _logService.SaveNewLog(newUser.UserName, "User is registered");
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = true,
                    StatusCode = 201,
                    Message = " User created successfully"
                };

            }
            catch (Exception ex)
            {
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }


        public async Task<LoginServiceResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user == null) return null;

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordCorrect) return null;

            //return token and userinfo
            var newToken = await GenerateJWTTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var userInfo = GenerateUserInfoObject(user, roles);

            await _logService.SaveNewLog(user.UserName, "New Login");

            return new LoginServiceResponseDto()
            {
                NewToken = newToken,
                UserInfo = userInfo
            };

        }

        private async Task<string> GenerateJWTTokenAsync(ApplicationUser user)
        {
            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim("FirstName",user.FirstName),
                new Claim("LastName",user.LastName)
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var singingCredentials = new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256);

            var tokenObject = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                signingCredentials: singingCredentials,
                claims: authClaims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddDays(1)
                );
            string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);
            return token;
        }

        private UserInfoResult GenerateUserInfoObject(ApplicationUser user, IEnumerable<string> roles)
        {
            return new UserInfoResult()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                Roles = roles
            };
        }

        public async Task<UserInfoResult> GetUserDetailsByUserName(string userName)
        {
            throw new NotImplementedException();
            var user = _userManager.Users.First(x => x.UserName == userName);
            if (user != null)
            {
                return new UserInfoResult()
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt,
                    Roles = user.Roles,
                };
            }

        }

        public Task<GeneralServiceResponseDto> GetUsersListAsync()
        {
            throw new NotImplementedException();
        }



        public Task<GeneralServiceResponseDto> MeAsync(MeDto meDto)
        {
            throw new NotImplementedException();
        }





        public Task<GeneralServiceResponseDto> UpdateRoleAsync(ClaimsPrincipal User, UpdateRoleDto updateRoleDto)
        {
            throw new NotImplementedException();
        }
    }
}
