using backend_net8.Core.DbContext;
using backend_net8.Core.DTOs.General;
using backend_net8.Core.DTOs.Message;
using backend_net8.Core.Entities;
using backend_net8.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend_net8.Core.Services
{
    public class MessageService : IMessageService
    {
        ApplicationDbContext context;
        ILogService logService;
        UserManager<ApplicationUser> userManager;

        public MessageService(ApplicationDbContext context, ILogService logService, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.logService = logService;
            this.userManager = userManager;
        }

        public async Task<GeneralServiceResponseDto> CreateMessageAsync(ClaimsPrincipal User, CreateMessageDto createMessageDto)
        {
            if (User.Identity.Name == createMessageDto.ReceiverUserName)
            {
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 400,
                    Message = "Sender and Receiver can't be same"
                };
            }

            var isReceivedUserNameValid = userManager.Users.Any(u => u.UserName == createMessageDto.ReceiverUserName);
            if (!isReceivedUserNameValid)
            {
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 400,
                    Message = "Receiver username is not valid"
                };
            }

            Message newMessage = new Message()
            {
                Sender = User.Identity.Name,
                Receiver = createMessageDto.ReceiverUserName,
                Text = createMessageDto.Text
            };

            await context.Messages.AddAsync(newMessage);
            await context.SaveChangesAsync();

            logService.SaveNewLog(User.Identity.Name, "Send Message");

            return new GeneralServiceResponseDto()
            {
                IsSucceed = true,
                StatusCode = 201,
                Message = "Message Saved Successfully"
            };
        }

        public async Task<IEnumerable<GetMessageDto>> GetMessagesAsync()
        {
            var messages = await context.Messages
                .Select(m => new GetMessageDto()
                {
                    SenderUserName = m.Sender,
                    ReceiverUserName = m.Receiver,
                    Text = m.Text,
                    CreatedAt = m.CreatedAt,
                    Id = m.Id
                })
                .ToListAsync();
            return messages;
        }



        public async Task<IEnumerable<GetMessageDto>> GetMyMessagesAsync(ClaimsPrincipal User)
        {
            var mymessages = await context.Messages
                .Where(m=> m.Sender==User.Identity.Name || m.Receiver==User.Identity.Name)
                .Select(m => new GetMessageDto() { SenderUserName = m.Sender, ReceiverUserName = m.Receiver, Text = m.Text, CreatedAt = m.CreatedAt, Id = m.Id })
                .ToListAsync();
            return mymessages;
        }
    }
}
