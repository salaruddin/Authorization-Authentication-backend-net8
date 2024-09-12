using backend_net8.Core.DTOs.Log;
using backend_net8.Core.Entities;
using backend_net8.Core.Interfaces;
using backend_net8.Core.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend_net8.Core.Services
{
    public class LogService : ILogService
    {
        ApplicationDbContext dbContext;
        public LogService(ApplicationDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task SaveNewLog(string UserName, string Description)
        {
            var newLog = new Log() { UserName = UserName, Description = Description };
            await dbContext.Logs.AddAsync(newLog);
            await dbContext.SaveChangesAsync();
        }
        public async Task<IEnumerable<GetLogDto>> GetLogsAsync()
        {
            var logs = await dbContext.Logs
                .Select(l => new GetLogDto
                {
                    CreatedAt = l.CreatedAt,
                    Description = l.Description,
                    UserName = l.UserName
                })
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
            return logs;
        }

        public async Task<IEnumerable<GetLogDto>> GetMyLogsAsync(ClaimsPrincipal User)
        {
            var mylogs = await dbContext.Logs
                .Where(l => l.UserName == User.Identity.Name)
                .Select(l => new GetLogDto
                {
                    CreatedAt = l.CreatedAt,
                    Description = l.Description,
                    UserName = l.UserName
                })
                .OrderByDescending(l => l.CreatedAt) 
                .ToListAsync();
            return mylogs;
        }


    }
}
