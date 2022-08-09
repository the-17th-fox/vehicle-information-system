﻿using AccountsService.Models;
using AccountsService.Services.Pagination;
using AccountsService.Utilities;
using AccountsService.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AccountsService.Services
{
    public interface IAccountsSvc
    {
        public Task RegisterAsync(User user, string password, bool requirePassword = true);
        public Task<User> RestoreAsync(User newUser, User oldUser, string password);
        public Task DeleteAsync(Guid id);
        public Task ChangeRoleAsync(Guid userId, string role);
        public Task<PagedList<User>> GetAllAsync(PageParametersViewModel pageParams);
        public Task<string> LoginAsync(string email, string password, IOptions<JwtConfigurationModel> securityConfig);
        public Task<User> SaveExternalUserAsync(ExternalLoginInfo loginInfo);
    }
}
