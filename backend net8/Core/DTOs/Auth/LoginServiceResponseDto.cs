namespace backend_net8.Core.DTOs.Auth
{
    public class LoginServiceResponseDto
    {
        public string NewToken { get; set; }
        public UserInfoResult UserInfo { get; set; }
    }
}
