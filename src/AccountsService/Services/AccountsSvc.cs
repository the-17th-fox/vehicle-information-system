using AccountsService.Constants.Auth;
using AccountsService.Constants.Logger;
using AccountsService.Exceptions.CustomExceptions;
using AccountsService.Infrastructure.Context;
using AccountsService.Models;
using AccountsService.Services.Pagination;
using AccountsService.Utilities;
using AccountsService.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Bson;
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
        private readonly AccountsServiceContext _context;
        private readonly IOptions<LogsContextConfiguration> _mongoConfig;
        public AccountsSvc(
            UserManager<User> userManager, 
            ILogger<AccountsSvc> logger, 
            AccountsServiceContext context, 
            RoleManager<IdentityRole<Guid>> roleManager, 
            IOptions<LogsContextConfiguration> mongoConfig)
        {
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
            _context = context;
            _mongoConfig = mongoConfig;
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

        private JwtSecurityToken CreateSecurityToken(IOptions<JwtConfigurationModel> securityConfig, List<Claim> claims)
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

        public async Task<string> LoginAsync(string email, string password, IOptions<JwtConfigurationModel> securityConfig)
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

        public async Task RegisterAsync(User user, string password, bool requirePassword = true)
        {
            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser is not null && !existingUser.IsDeleted)
            {
                _logger.LogInformation(LoggingForms.FailedToRegister, user.UserName, user.Email, (LoggingForms.UserAlreadyExists, user.Email));
                throw new InvalidParamsException("There is already a user with this email");
            }

            // For new external users and users who restore their accounts
            if(existingUser is not null && existingUser.IsDeleted)
            {
                await RestoreAsync(user, existingUser, password);
                return;
            }

            var result = requirePassword 
                ? await _userManager.CreateAsync(user, password) 
                : await _userManager.CreateAsync(user);

            if (!result.Succeeded)
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

        public async Task<User> RestoreAsync(User newUser, User oldUser, string password)
        {
            using(var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    oldUser.IsDeleted = false;
                    oldUser.UserName = newUser.UserName;
                    await _userManager.UpdateSecurityStampAsync(oldUser);
                    await _userManager.RemovePasswordAsync(oldUser);

                    // If the user came from default registration form he's unable to miss the password field
                    // otherwise, if the user came from external login (registration) he doesn't need any pass
                    if (password is not null)
                        await _userManager.AddPasswordAsync(oldUser, password);

                    await _userManager.UpdateAsync(oldUser);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                }
            }

            _logger.LogInformation(LoggingForms.Restored, oldUser.Email);
            
            return oldUser;
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null)
            {
                _logger.LogInformation(LoggingForms.UserNotFound, id.ToString());
                throw new NotFoundException($"User with provided id [{id}] was not found");
            }

            if(user.IsDeleted == true)
            {
                _logger.LogInformation(LoggingForms.AlreadyDeleted, id);
                throw new Exception($"User with provided id [{id}] is already deleted");
            }

            IList<UserLoginInfo> loginInfos;
            if ((loginInfos = await _userManager.GetLoginsAsync(user)).Any())
            {
                foreach (var item in loginInfos)
                    await _userManager.RemoveLoginAsync(user, item.LoginProvider, item.ProviderKey);
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
            if (findRole is null)
            {
                _logger.LogInformation(LoggingForms.RoleNotFound, role);
                throw new NotFoundException($"Role {role} not found");
            }

            var inRole = await _userManager.IsInRoleAsync(user, findRole.Name);
            if (inRole)
            {
                _logger.LogInformation(LoggingForms.UserAlreadyInRole, userId, findRole.Name);
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

        public async Task<User> SaveExternalUserAsync(ExternalLoginInfo loginInfo)
        {
            var email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email);

            var existingUser = await _userManager.FindByEmailAsync(email);
            if(existingUser is not null && !existingUser.IsDeleted)
            {
                _logger.LogInformation(LoggingForms.UserAlreadyExists, email);
                throw new InvalidParamsException("There is already a user with this email");
            }

            var user = new User
            {
                UserName = email,
                Email = email
            };

            if(existingUser is null)
                await RegisterAsync(user, null!, requirePassword: false);

            if (existingUser is not null && existingUser.IsDeleted)
                user = await RestoreAsync(user, existingUser, null!);

            await _userManager.AddLoginAsync(user, loginInfo);

            return user;
        }

        public async Task<PagedList<LoggingRecord>> GetAllLogsAsync(LogsParametersViewModel logsParams, PageParametersViewModel pageParams)
        {
            if (string.IsNullOrWhiteSpace(_mongoConfig.Value.ConnectionString))
            {
                var argument = nameof(_mongoConfig.Value.ConnectionString);
                _logger.LogError(LoggingForms.ParameterMissed, argument);
                throw new InvalidParamsException(LoggingForms.ParameterMissed.Replace("{argument}", argument));
            }

            if (string.IsNullOrWhiteSpace(_mongoConfig.Value.CollectionName))
            {
                var argument = nameof(_mongoConfig.Value.CollectionName);
                _logger.LogError(LoggingForms.ParameterMissed, argument);
                throw new InvalidParamsException(LoggingForms.ParameterMissed.Replace("{argument}", argument));
            }

            var context = new MongoDbContext<LoggingRecord>(_mongoConfig.Value.ConnectionString, _mongoConfig.Value.CollectionName);

            _logger.LogInformation(LoggingForms.DbConnectionEstablished, context.DatabaseName);

            var logsColl = context.GetCollection();

            var fromDate = new DateTime(
                year: logsParams.FromYear, 
                month: logsParams.FromMonth, 
                day: logsParams.FromDay);

            var logs = logsColl
                //.Find(l => Enum.Parse<LogLevel>(l.LogLevel) >= logsParams.LowestLoggingLevel)
                .Find(l => true);
                //.SortByDescending(l => l.UtcTimestamp);

            return await PagedList<LoggingRecord>.ToPagedListAsync(logs, pageParams.PageNumber, pageParams.PageSize);
        }
    }
}
