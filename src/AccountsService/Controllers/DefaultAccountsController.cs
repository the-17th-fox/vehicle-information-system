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
        public DefaultAccountsController(IAccountsSvc accountsSvc, IMapper mapper, ILogger<DefaultAccountsController> logger)
        {
            _accountsSvc = accountsSvc;
            _mapper = mapper;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegistrationViewModel viewModel)
        {
            var user = _mapper.Map<User>(viewModel);
            await _accountsSvc.RegisterAsync(user, viewModel.Password);

            // todo: Replace with with more wide-meaning logging form
            //_logger.LogInformation(LoggingHelper.LogUserActions(LoggingForms.Registred, user));

            return Ok();
        }
    }
}
