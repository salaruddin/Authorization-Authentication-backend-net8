using backend_net8.Core.Constants;
using backend_net8.Core.DTOs.Auth;
using backend_net8.Core.DTOs.General;
using backend_net8.Core.Entities;
using backend_net8.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            if (user is not null)
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

        public async Task<GeneralServiceResponseDto> UpdateRoleAsync(ClaimsPrincipal User, UpdateRoleDto updateRoleDto)
        {
            var user = await _userManager.FindByNameAsync(updateRoleDto.UserName);

            if (user is null)
            {
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 404,
                    Message = "User not found"
                };
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            //if User is admin, it allows to change the users having USER,MANAGER roles to USER or MANAGER role. it can't change other than user and manger roles.
            if (User.IsInRole(StaticUserRoles.ADMIN))
            {
                if (updateRoleDto.NewRole == RoleType.USER || updateRoleDto.NewRole == RoleType.MANAGER)
                {
                    if (user.Roles.Any(r => r.Equals(StaticUserRoles.ADMIN) || r.Equals(StaticUserRoles.OWNER)))
                    {
                        return new GeneralServiceResponseDto()
                        {
                            IsSucceed = false,
                            StatusCode = 403,
                            Message = "You are not allowed to change role of this user"
                        };
                    }
                    else
                    {
                        await _userManager.RemoveFromRolesAsync(user, userRoles);
                        await _userManager.AddToRoleAsync(user, updateRoleDto.NewRole.ToString());
                        await _logService.SaveNewLog(user.UserName, "User roles updated");

                        return new GeneralServiceResponseDto()
                        {
                            IsSucceed = true,
                            StatusCode = 200,
                            Message = "User role updated successfully"
                        };
                    }
                }

                //if role that is being changed, is not user or manager
                else
                {
                    return new GeneralServiceResponseDto()
                    {
                        IsSucceed = false,
                        StatusCode = 403,
                        Message = "You are not allowed to change role of this user"
                    };
                }

            }
            else
            {
                if (user.Roles.Any(r => r.Equals(StaticUserRoles.OWNER)))
                {
                    return new GeneralServiceResponseDto()
                    {
                        IsSucceed = false,
                        StatusCode = 403,
                        Message = "You are not allowed to change role of this user"
                    };
                }

                await _userManager.RemoveFromRolesAsync(user, userRoles);
                await _userManager.AddToRoleAsync(user, updateRoleDto.NewRole.ToString());
                await _logService.SaveNewLog(user.UserName, "Role updated successfully");
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = true,
                    StatusCode = 200,
                    Message = "Role Updated"
                };
            }



        }

        public async Task<LoginServiceResponseDto?> MeAsync(MeDto meDto)
        {
            ClaimsPrincipal handler = new JwtSecurityTokenHandler().ValidateToken(meDto.Token, new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["JWT:ValidIssuer"],
                ValidAudience = _configuration["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]))
            }, out SecurityToken securityToken);

            string decodedUserName = handler.Claims.First(q => q.Type == ClaimTypes.Name).Value;
            if (decodedUserName == null)
                return null;

            var user = await _userManager.FindByNameAsync(decodedUserName);
            if (user == null)
                return null;

            var newToken = await GenerateJWTTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var userInfo = GenerateUserInfoObject(user, roles);

            await _logService.SaveNewLog(user.UserName, "New Token Generated");

            return new LoginServiceResponseDto()
            {
                NewToken = newToken,
                UserInfo = userInfo,
            };



        }

        public async Task<IEnumerable<UserInfoResult>> GetUsersListAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            List<UserInfoResult> result = new List<UserInfoResult>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userInfo = GenerateUserInfoObject(user, roles);
                result.Add(userInfo);
            }
            return result;
        }

        public async Task<UserInfoResult> GetUserDetailsByUserNameAsync(string userName)
        {

            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            var userInfo =  GenerateUserInfoObject(user, roles);
            return userInfo;
            
            
        }

        public async Task<IEnumerable<string>> GetUsernamesListAsync(string userName)
        {
            var usernames = await _userManager.Users
                 .Select(x => x.UserName)
                 .ToListAsync();
            return usernames;
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

    }
}
