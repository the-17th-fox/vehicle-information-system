using AccountsService.Services;
using AccountsService.Utilities;
using AccountsService.ViewModels;
using AutoMapper;
using Common.Constants.Auth;
using Common.Models.AccountsService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly SignInManager<User> _signInManager;
        private string _userId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private string _userEmail => User.FindFirstValue(ClaimTypes.Email);

        public DefaultAccountsController(
            IAccountsSvc accountsSvc, 
            IMapper mapper, 
            ILogger<DefaultAccountsController> logger,
            IOptions<JwtConfigurationModel> jwtConfig,
            SignInManager<User> signInManager)
        {
            _accountsSvc = accountsSvc;
            _mapper = mapper;
            _logger = logger;
            _jwtConfig = jwtConfig;
            _signInManager = signInManager;
        }        

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegistrationViewModel viewModel)
        {
            _logger.LogInformation(LogEventType.RegistrationAttempt, viewModel.UserName, viewModel.Email);

            var user = _mapper.Map<User>(viewModel);
            await _accountsSvc.RegisterAsync(user, viewModel.Password);

            _logger.LogInformation(LogEventType.Registered, user.UserName, user.Email);

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginViewModel viewModel)
        {
            _logger.LogInformation(LogEventType.LoginAttempt, viewModel.Email);

            var token = await _accountsSvc.LoginAsync(viewModel.Email, viewModel.Password, _jwtConfig);

            _logger.LogInformation(LogEventType.LoggedIn, viewModel.Email);

            return Ok(token);
        }

        [Authorize(Policy = AccountsPolicies.DefaultRights, AuthenticationSchemes = "Identity.Application,Bearer")]
        [HttpDelete("[action]")]
        public async Task<IActionResult> DeleteAsync()
        {
            _logger.LogInformation(LogEventType.DeletionAttempt, _userId);

            await _accountsSvc.DeleteAsync(Guid.Parse(_userId));

            if (User?.Identity?.AuthenticationType != "Bearer")
                await LogoutGoogleAsync();

            _logger.LogInformation(LogEventType.UserDeleted, _userId);

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task LoginGoogleAsync()
        {
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, Url.Action(nameof(GoogleResponse)));

            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties);
        }

        [Authorize(AuthenticationSchemes = "Identity.Application")]
        [HttpGet("[action]")]
        public async Task<IActionResult> LogoutGoogleAsync()
        {
            await _signInManager.SignOutAsync();

            _logger.LogInformation(LogEventType.GoogleLogout, _userId, _userEmail);            

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IActionResult> GoogleResponse()
        {
            var loginInfo = await _signInManager.GetExternalLoginInfoAsync();

            var googleId = loginInfo.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email);

            _logger.LogInformation(LogEventType.GoogleAuthPassed, googleId, email);

            var signInResult = await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, isPersistent: false);

            // If the user has never been logged in -> creates a record in the users table
            
            if(!signInResult.Succeeded)
            {
                var user = await _accountsSvc.SaveExternalUserAsync(loginInfo);

                await _signInManager.SignInAsync(user, isPersistent: false);
            }
            
            _logger.LogInformation(LogEventType.GoogleLoggedIn, _userId, email);

            return Ok();
        }

    }
}
