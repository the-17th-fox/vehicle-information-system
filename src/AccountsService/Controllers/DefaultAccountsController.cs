using AccountsService.Constants.Auth;
using AccountsService.Constants.Logger;
using AccountsService.Models;
using AccountsService.Services;
using AccountsService.Utilities;
using AccountsService.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AccountsService.Controllers
{
    /// <summary>
    /// This controller is defined to handle Default Users actions
    /// </summary>
    /// 
    [Authorize]
    [Route("api/account")]
    [ApiController]
    public class DefaultAccountsController : ControllerBase
    {
        private readonly IAccountsSvc _accountsSvc;
        private readonly IMapper _mapper;
        private readonly ILogger<DefaultAccountsController> _logger;
        private readonly IOptions<JwtConfigugartionModel> _jwtConfig;
        private Guid _userId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        public DefaultAccountsController(
            IAccountsSvc accountsSvc, 
            IMapper mapper, 
            ILogger<DefaultAccountsController> logger,
            IOptions<JwtConfigugartionModel> jwtConfig)
        {
            _accountsSvc = accountsSvc;
            _mapper = mapper;
            _logger = logger;
            _jwtConfig = jwtConfig;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegistrationViewModel viewModel)
        {
            _logger.LogInformation(LoggingForms.RegistrationAttempt, viewModel.UserName, viewModel.Email);

            var user = _mapper.Map<User>(viewModel);
            await _accountsSvc.RegisterAsync(user, viewModel.Password);

            _logger.LogInformation(LoggingForms.Registred, user.UserName, user.Email);

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginViewModel viewModel)
        {
            _logger.LogInformation(LoggingForms.LoginAttempt, viewModel.Email);

            var token = await _accountsSvc.LoginAsync(viewModel.Email, viewModel.Password, _jwtConfig);

            _logger.LogInformation(LoggingForms.LoggedIn, viewModel.Email);

            return Ok(token);
        }

        [Authorize(Policy = AccountsPolicies.DefaultRights)]
        [HttpDelete("[action]")]
        public async Task<IActionResult> DeleteAsync()
        {
            _logger.LogInformation(LoggingForms.DeletionAttempt, _userId);

            await _accountsSvc.DeleteAsync(_userId);

            _logger.LogInformation(LoggingForms.UserDeleted, _userId);

            return Ok();
        }
    }
}
