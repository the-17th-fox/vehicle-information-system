﻿using AccountsService.Constants.Auth;
using AccountsService.Constants.Logger;
using AccountsService.Services;
using AccountsService.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountsService.Controllers
{
    [Authorize(Policy = AccountsPolicies.ElevatedRights, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
            _logger.LogInformation(LoggingForms.DeletionAttempt, id);

            await _accountsSvc.DeleteAsync(id);

            _logger.LogInformation(LoggingForms.UserDeleted, id);

            return Ok();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllUsersAsync([FromQuery] PageParametersViewModel pageParams)
        {
            _logger.LogInformation(LoggingForms.TryingToGetUsers);

            var accounts = await _accountsSvc.GetAllAsync(pageParams);
            var accountsVM = _mapper.Map<PageViewModel<UserViewModel>>(accounts);

            _logger.LogInformation(LoggingForms.GotUsers);

            return Ok(accountsVM);
        }
    }
}
