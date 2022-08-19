using Common.Utilities.Pagination;
using AccountsService.Utilities;
using Common.Models.AccountsService;
using Common.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AccountsService.Services
{
    public interface IAccountsSvc
    {
        public Task RegisterAsync(User user, string password, bool requirePassword = true);
        public Task<User> RestoreAsync(User newUser, User oldUser, string password);
        public Task DeleteAsync(Guid id);
        public Task ChangeRoleAsync(Guid userId, string role);
        public Task<PagedList<User>> GetAllAsync(PageParametersViewModel pageParams);
        public Task<string> LoginAsync(string email, string password, IOptions<JwtConfigurationModel> securityConfig);
        public Task<User> SaveExternalUserAsync(ExternalLoginInfo loginInfo);
        public Task<PagedList<LoggingRecord>> GetAllLogsAsync(LogsParametersViewModel logsParams, PageParametersViewModel pageParams);
    }
}
