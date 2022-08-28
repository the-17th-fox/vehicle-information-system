﻿using AccountsService.Services;
using AccountsService.Utilities;
using AccountsService.ViewModels;
using AutoMapper;
using Common.Constants.Auth;
using Common.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountsService.Controllers
{
    [Authorize(Policy = AccountsPolicies.ElevatedRights, AuthenticationSchemes = "Identity.Application,Bearer")]
    [Route("api/users")]
    [ApiController]
    public class AdminAccountsController : ControllerBase
    {
        private readonly ILogger<AdminAccountsController> _logger;
        private readonly IAccountsSvc _accountsSvc;
        private readonly IMapper _mapper;
        public AdminAccountsController(IAccountsSvc accountsSvc, ILogger<AdminAccountsController> logger, IMapper mapper)
        {
            _accountsSvc = accountsSvc;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            _logger.LogInformation(LogEventType.DeletionAttempt, id);

            await _accountsSvc.DeleteAsync(id);

            _logger.LogInformation(LogEventType.UserDeleted, id);

            return Ok();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllUsersAsync([FromQuery] PageParametersViewModel pageParams)
        {
            _logger.LogInformation(LogEventType.TryingToGetUsers);

            var accounts = await _accountsSvc.GetAllAsync(pageParams);
            var accountsVM = _mapper.Map<PageViewModel<UserViewModel>>(accounts);

            _logger.LogInformation(LogEventType.GotUsers);

            return Ok(accountsVM);
        }

        [HttpPatch("[action]")]
        public async Task<IActionResult> ChangeRoleAsync(Guid userId, string role)
        {
            await _accountsSvc.ChangeRoleAsync(userId, role);

            _logger.LogInformation(LogEventType.AddedToRole, userId, role);

            return Ok();
        }
    }
}
