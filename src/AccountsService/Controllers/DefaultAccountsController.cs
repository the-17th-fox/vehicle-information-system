using AccountsService.Constants.Auth;
using AccountsService.Constants.Logger;
using AccountsService.Exceptions.CustomExceptions;
using AccountsService.Models;
using AccountsService.Services;
using AccountsService.Utilities;
using AccountsService.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        private readonly IOptions<JwtConfigurationModel> _jwtConfig;
        private string _userId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private string _userEmail => User.FindFirstValue(ClaimTypes.Email);

        public DefaultAccountsController(
            IAccountsSvc accountsSvc, 
            IMapper mapper, 
            ILogger<DefaultAccountsController> logger,
            IOptions<JwtConfigurationModel> jwtConfig)
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

        [Authorize(Policy = AccountsPolicies.DefaultRights, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("[action]")]
        public async Task<IActionResult> DeleteAsync()
        {
            _logger.LogInformation(LoggingForms.DeletionAttempt, _userId);

            await _accountsSvc.DeleteAsync(Guid.Parse(_userId));

            _logger.LogInformation(LoggingForms.UserDeleted, _userId);

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task LoginGoogleAsync()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleResponse))
            };

            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties);
        }

        [Authorize(AuthenticationSchemes = GoogleDefaults.AuthenticationScheme)]
        [HttpGet("[action]")]
        public async Task<IActionResult> LogoutGoogleAsync()
        {
            await HttpContext.SignOutAsync("Identity.External");
            await HttpContext.SignOutAsync("Identity.Application");

            _logger.LogInformation(LoggingForms.GoogleLogout, _userId, _userEmail);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IActionResult> GoogleResponse()
        {
            AuthenticateResult authResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authResult.Succeeded)
            {
                throw new UnauthorizedException(authResult.Failure!.Message);
            }
            var claimsPrincipal = authResult.Principal;
            var id = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);

            _logger.LogInformation(LoggingForms.GoogleAuthPassed, id, email);

            var claimsIdentity = new ClaimsIdentity(GoogleDefaults.AuthenticationScheme);
            claimsIdentity = _accountsSvc.GetGoogleUserClaims(claimsPrincipal, claimsIdentity);

            await HttpContext.SignInAsync("Identity.Application", new ClaimsPrincipal(claimsIdentity));
            _logger.LogInformation(LoggingForms.GoogleLoggedIn, id, email);

            return Ok();
        }
    }
}
