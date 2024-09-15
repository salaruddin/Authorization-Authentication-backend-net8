using Microsoft.IdentityModel.Tokens;

namespace backend_net8.Core.DTOs.Auth
{
    public class UpdateRoleDto
    {
        public string UserName { get; set; }
        public RoleType NewRole { get; set; }
    }
    public enum RoleType
    {
        OWNER,
        ADMIN,
        MANAGER,
        USER
    }
}
