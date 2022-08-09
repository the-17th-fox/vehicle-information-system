using AccountsService.Constants.Auth;
using AccountsService.Constants.Logger;
using AccountsService.Exceptions.CustomExceptions;
using AccountsService.Models;
using AccountsService.Services.Pagination;
using AccountsService.Utilities;
using AccountsService.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AccountsService.Services
{
    public class AccountsSvc : IAccountsSvc
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ILogger<AccountsSvc> _logger;
        public AccountsSvc(UserManager<User> userManager, ILogger<AccountsSvc> logger, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
        }

        private List<Claim> GetClaims(User user, IList<string> userRoles)
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

        private JwtSecurityToken CreateSecurityToken(IOptions<JwtConfigugartionModel> securityConfig, List<Claim> claims)
        {
            var symSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityConfig.Value.Key));

            return new(
                issuer: securityConfig.Value.Issuer,
                audience: securityConfig.Value.Audience,
                notBefore: DateTime.UtcNow,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(securityConfig.Value.LifetimeHours),
                signingCredentials: new SigningCredentials(symSecurityKey, SecurityAlgorithms.HmacSha256));
        }

        public async Task<string> LoginAsync(string email, string password, IOptions<JwtConfigugartionModel> securityConfig)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null || user.IsDeleted)
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

        public async Task<PagedList<User>> GetAllAsync(PageParametersViewModel pageParams)
        {
            var users = _userManager.Users.AsNoTracking();
            if(!users.Any())
            {
                _logger.LogInformation(LoggingForms.NoUsersFound);
                throw new NotFoundException("No users were found");
            }

            return await PagedList<User>.ToPagedListAsync(users, pageParams.PageNumber, pageParams.PageSize);
        }

        public async Task ChangeRoleAsync(Guid userId, string role)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
            {
                _logger.LogInformation(LoggingForms.UserNotFound, userId);
                throw new NotFoundException($"User with provided id [{userId}] was not found");
            }

            var findRole = await _roleManager.FindByNameAsync(role);   
            if(findRole is null)
            {
                _logger.LogInformation(LoggingForms.RoleNotFound, role);
                throw new NotFoundException($"Role {role} not found");
            }

            var inRole = await _userManager.IsInRoleAsync(user, findRole.Name);
            if(inRole)
            {
                _logger.LogInformation(LoggingForms.UserAlredyInRole, userId, findRole.Name);
                throw new Exception($"This user [{userId}] already in role {role}");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var removeRoles = await _userManager.RemoveFromRolesAsync(user, userRoles);
            var result = await _userManager.AddToRoleAsync(user, findRole.Name);
            if (!result.Succeeded)
            {
                var error = result.Errors.First<IdentityError>().Description;
                _logger.LogInformation(LoggingForms.FailedToAddToRole, userId, role, error);
                throw new Exception(error);
            }
        }
    }
}
