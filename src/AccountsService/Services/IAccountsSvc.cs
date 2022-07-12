using AccountsService.Models;
using AccountsService.Utilities;
using AccountsService.ViewModels;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AccountsService.Services
{
    public interface IAccountsSvc
    {
        public Task RegisterAsync(User user, string password);
        public Task DeleteAsync(Guid id);
        public Task<string> LoginAsync(string email, string password, IOptions<JwtConfigugartionModel> securityConfig);
        protected List<Claim> GetClaims(User user, IList<string> userRoles);
        protected JwtSecurityToken CreateSecurityToken(IOptions<JwtConfigugartionModel> securityConfig, List<Claim> claims);
    }
}
