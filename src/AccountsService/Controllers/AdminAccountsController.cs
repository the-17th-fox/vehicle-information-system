using AccountsService.Constants.Auth;
using AccountsService.Constants.Logger;
using AccountsService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountsService.Controllers
{
    [Authorize(Roles = AccountsRoles.Administrator)]
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
    }
}
