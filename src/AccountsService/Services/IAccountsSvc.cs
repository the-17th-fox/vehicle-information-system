using AccountsService.Models;
using AccountsService.Services.Pagination;
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
        public Task<PagedList<User>> GetAllAsync(PageParametersViewModel pageParams);
        public Task<string> LoginAsync(string email, string password, IOptions<JwtConfigurationModel> securityConfig);
        public ClaimsIdentity GetGoogleUserClaims(ClaimsPrincipal claimsPrincipal, ClaimsIdentity claimsIdentity);
    }
}
