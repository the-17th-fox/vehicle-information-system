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

        // Temporaly has been moved from the AdminController in case of testing
        // Will be moved back in the end
        // todo: add prettier binding from the url for logsParams
        //[Authorize(Policy = AccountsPolicies.DefaultRights, AuthenticationSchemes = "Identity.Application,Bearer")]
        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllLogsAsync([FromQuery] LogsParametersViewModel logsParams, [FromQuery] PageParametersViewModel pageParams)
        {
            _logger.LogInformation(LoggingForms.LogsRetrievingAttempt, _userId, _userEmail);

            var result = await _accountsSvc.GetAllLogsAsync(logsParams, pageParams);

            _logger.LogInformation(LoggingForms.LogsRetrieved, _userId, _userEmail);

            return Ok(result);
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

        [Authorize(Policy = AccountsPolicies.DefaultRights, AuthenticationSchemes = "Identity.Application,Bearer")]
        [HttpDelete("[action]")]
        public async Task<IActionResult> DeleteAsync()
        {
            _logger.LogInformation(LoggingForms.DeletionAttempt, _userId);

            await _accountsSvc.DeleteAsync(Guid.Parse(_userId));

            if (User?.Identity?.AuthenticationType != "Bearer")
                await LogoutGoogleAsync();

            _logger.LogInformation(LoggingForms.UserDeleted, _userId);

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

            _logger.LogInformation(LoggingForms.GoogleLogout, _userId, _userEmail);            

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IActionResult> GoogleResponse()
        {
            var loginInfo = await _signInManager.GetExternalLoginInfoAsync();

            var googleId = loginInfo.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email);

            _logger.LogInformation(LoggingForms.GoogleAuthPassed, googleId, email);

            var signInResult = await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, isPersistent: false);

            // If the user has never been logged in -> create a record in the users table
            
            if(!signInResult.Succeeded)
            {
                var user = await _accountsSvc.SaveExternalUserAsync(loginInfo);

                await _signInManager.SignInAsync(user, isPersistent: false);
            }
            
            _logger.LogInformation(LoggingForms.GoogleLoggedIn, _userId, email);

            return Ok();
        }

    }
}
