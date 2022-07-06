using AccountsService.Models;
using AccountsService.Utilities;
using AccountsService.ViewModels;

namespace AccountsService.Services
{
    public interface IAccountsSvc
    {
        public Task RegisterAsync(User user, string password);
    }
}
