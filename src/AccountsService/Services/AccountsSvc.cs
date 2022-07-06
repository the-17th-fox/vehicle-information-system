using AccountsService.Constants.Auth;
using AccountsService.Models;
using Microsoft.AspNetCore.Identity;
using System.Reflection; // Isn't necessary after all exceptions will be implemented

namespace AccountsService.Services
{
    public class AccountsSvc : IAccountsSvc
    {
        private readonly UserManager<User> _userManager;
        public AccountsSvc(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task RegisterAsync(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if(!result.Succeeded)
            {
                // Should be replaced with custom exception (error handling middleware)
                // Errors logging should be deligated to the error handling middleware
                throw new NotImplementedException($"{MethodBase.GetCurrentMethod()!.Name} | {result.Errors.First().Description}");
            }

            result = await _userManager.AddToRoleAsync(user, AccountsRoles.DefaultUser);
            if(!result.Succeeded)
            {
                // Should be replaced with custom exception (error handling middleware)
                // Errors logging should be deligated to the error handling middleware
                throw new NotImplementedException($"{MethodBase.GetCurrentMethod()!.Name} | {result.Errors.First().Description}");
            }
        }
    }
}
