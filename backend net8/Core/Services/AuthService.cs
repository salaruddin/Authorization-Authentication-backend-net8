using backend_net8.Core.Constants;
using backend_net8.Core.DTOs.Auth;
using backend_net8.Core.DTOs.General;
using backend_net8.Core.Entities;
using backend_net8.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend_net8.Core.Services
{
    public class AuthService : IAuthService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogService logService;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogService logService, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            this.logService = logService;
            _configuration = configuration;
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

        public Task<GeneralServiceResponseDto> LoginAsync(LoginDto loginDto)
        {
            throw new NotImplementedException();
        }

        public Task<GeneralServiceResponseDto> MeAsync(MeDto meDto)
        {
            throw new NotImplementedException();
        }

        public Task<GeneralServiceResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            throw new NotImplementedException();
        }



        public Task<GeneralServiceResponseDto> UpdateRoleAsync(ClaimsPrincipal User, UpdateRoleDto updateRoleDto)
        {
            throw new NotImplementedException();
        }
    }
}
