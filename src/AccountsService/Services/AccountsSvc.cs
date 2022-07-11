using AccountsService.Constants.Auth;
using AccountsService.Constants.Logger;
using AccountsService.Exceptions.CustomExceptions;
using AccountsService.Models;
using AccountsService.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

        public List<Claim> GetClaims(User user, IList<string> userRoles)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        public JwtSecurityToken CreateSecurityToken(IOptions<JwtConfigugartionModel> securityConfig, List<Claim> claims)
        {
            return new(
                issuer: securityConfig.Value.Issuer,
                audience: securityConfig.Value.Audience,
                notBefore: DateTime.UtcNow,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(securityConfig.Value.LifetimeHours));
        }

        public async Task<string> LoginAsync(string email, string password, IOptions<JwtConfigugartionModel> securityConfig)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                _logger.LogInformation(LoggingForms.UserNotFound, email);
                throw new NotFoundException($"User with provided email {email} was not found") ;
            }

            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                _logger.LogInformation(LoggingForms.InvalidCredentials, email);
                throw new UnauthorizedException();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var userClaims = GetClaims(user, userRoles);
            var token = CreateSecurityToken(securityConfig, userClaims);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task RegisterAsync(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if(!result.Succeeded)
            {
                _logger.LogInformation(LoggingForms.FailedToRegister, user.UserName, user.Email, result.Errors.First<IdentityError>().Description);
                throw new InvalidParamsException(result.Errors.First<IdentityError>().Description);
            }

            result = await _userManager.AddToRoleAsync(user, AccountsRoles.DefaultUser);
            if(!result.Succeeded)
            {   
                throw new Exception(result.Errors.First<IdentityError>().Description);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null)
            {
                _logger.LogInformation(LoggingForms.UserNotFound, user.Email);
                throw new NotFoundException($"User with provided id [{id}] was not found");
            }

            if(user.IsDeleted == true)
            {
                _logger.LogInformation(LoggingForms.AlreadyDeleted, id);
                throw new Exception($"User with provided id [{id}] is already deleted");
            }

            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var error = result.Errors.First<IdentityError>().Description;
                _logger.LogInformation(LoggingForms.FailedToDelete, id, error);
                throw new Exception(error);
            }
        }
    }
}
