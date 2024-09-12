using backend_net8.Core.DTOs.Auth;
using backend_net8.Core.DTOs.General;
using System.Security.Claims;

namespace backend_net8.Core.Interfaces
{
    public interface IAuthService
    {
        Task<GeneralServiceResponseDto> SeedRolesAsync();
        Task<GeneralServiceResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<GeneralServiceResponseDto> LoginAsync(LoginDto loginDto);
        Task<GeneralServiceResponseDto> UpdateRoleAsync(ClaimsPrincipal User, UpdateRoleDto updateRoleDto);
        Task<GeneralServiceResponseDto> MeAsync(MeDto meDto);
        Task<GeneralServiceResponseDto> GetUsersListAsync();
        Task<UserInfoResult> GetUserDetailsByUserName(string userName);


        
    }
}
