using AccountsService.Constants.Auth;
using AccountsService.Constants.Logger;
using AccountsService.Services;
using AccountsService.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountsService.Controllers
{
    [Authorize(Policy = AccountsPolicies.ElevatedRights)]
    [Route("api/users")]
    [ApiController]
    public class AdminAccountsController : ControllerBase
    {
        private readonly ILogger<AdminAccountsController> _logger;
        private readonly IAccountsSvc _accountsSvc;
        public AdminAccountsController(IAccountsSvc accountsSvc, ILogger<AdminAccountsController> logger)
        {
            _accountsSvc = accountsSvc;
            _logger = logger;
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            _logger.LogInformation(LoggingForms.DeletionAttempt, id);

            await _accountsSvc.DeleteAsync(id);

            _logger.LogInformation(LoggingForms.UserDeleted, id);

            return Ok();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllUsersAsync(int pageNumber = 1, int pageSize = 20)
        {
            _logger.LogInformation(LoggingForms.TryingToGetUsers);

            var users = await _accountsSvc.GetUsersAsync();
            var page = new PageViewModel(users.Count, pageNumber, pageSize);
            users = users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var accountsVM = new AccountsViewModel()
            {
                PageViewModel = page,
                Users = users
            };

            _logger.LogInformation(LoggingForms.GotUsers);

            return Ok(accountsVM);
        }
    }
}
