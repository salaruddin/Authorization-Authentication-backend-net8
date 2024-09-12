using backend_net8.Core.DTOs.Log;

namespace backend_net8.Core.Interfaces
{
    public interface ILogService
    {
        Task SaveNewLog(string UserName,string Description);
        Task<IEnumerable<GetLogDto>> GetLogsAsync();
        Task<IEnumerable<GetLogDto>> GetMyLogsAsync();
    }
}
