using AccountsService.Constants.Auth;
using AccountsService.Constants.Logger;
using AccountsService.Exceptions.CustomExceptions;
using AccountsService.Models;
using AccountsService.Utilities;
using Microsoft.AspNetCore.Identity;

namespace AccountsService.Services
{
    public class AccountsSvc : IAccountsSvc
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AccountsSvc> _logger;
        public AccountsSvc(UserManager<User> userManager, ILogger<AccountsSvc> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task RegisterAsync(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if(!result.Succeeded)
            {
                throw new RegistrationFailedException($"{result.Errors.First().Description}");
            }
            _logger.LogInformation(LoggingForms.Registred, user.UserName, user.Email);


            result = await _userManager.AddToRoleAsync(user, AccountsRoles.DefaultUser);
            if(!result.Succeeded)
            {
                throw new RegistrationFailedException($"{result.Errors.First().Description}");
            }
            _logger.LogInformation(LoggingForms.AddedToRole, user.UserName, user.Email, AccountsRoles.DefaultUser);
        }
    }
}
