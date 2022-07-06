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
        public DefaultAccountsController(IAccountsSvc accountsSvc, IMapper mapper)
        {
            _accountsSvc = accountsSvc;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegistrationViewModel viewModel)
        {
            var user = _mapper.Map<User>(viewModel);
            await _accountsSvc.RegisterAsync(user, viewModel.Password);

            return Ok();
        }
    }
}
