using AccountsService.Constants.Auth;
using AccountsService.Models;
using AccountsService.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection; // Isn't necessary after all exceptions will be implemented
using System.Security.Claims;

namespace AccountsService.Services
{
    public class AccountsSvc : IAccountsSvc
    {
        private readonly UserManager<User> _userManager;
        public AccountsSvc(UserManager<User> userManager)
        {
            _userManager = userManager;
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
            // todo: add logging to this method
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                throw new NotImplementedException();
            }

            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                throw new NotImplementedException();
            }

            var userClaims = GetClaims(user, await _userManager.GetRolesAsync(user));
            var token = CreateSecurityToken(securityConfig, userClaims);

            return new JwtSecurityTokenHandler().WriteToken(token);
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
