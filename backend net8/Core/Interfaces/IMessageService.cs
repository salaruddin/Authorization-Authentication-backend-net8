using backend_net8.Core.DTOs.General;
using backend_net8.Core.DTOs.Message;
using System.Security.Claims;

namespace backend_net8.Core.Interfaces
{
    public interface IMessageService
    {
        Task<GeneralServiceResponseDto> CreateMessageAsync(ClaimsPrincipal User, CreateMessageDto createMessageDto);
        Task<GetMessageDto> GetMessagesAsync();
        Task<GetMessageDto> GetMyMessagesAsync(ClaimsPrincipal User);
    }
}
