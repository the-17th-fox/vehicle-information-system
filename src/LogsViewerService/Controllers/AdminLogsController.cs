using AutoMapper;
using Common.ViewModels;
using LogsViewerService.Services;
using LogsViewerService.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LogsViewerService.Utilities;

namespace LogsViewerService.Controllers
{
    [Route("api/logs")]
    [ApiController]
    public class AdminLogsController : ControllerBase
    {
        private readonly ILogger<AdminLogsController> _logger;
        private readonly IMapper _mapper;
        private readonly ILogsViewerSvc _logsSvc;
        private string _userId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private string _userEmail => User.FindFirstValue(ClaimTypes.Email);

        public AdminLogsController(ILogger<AdminLogsController> logger, IMapper mapper, ILogsViewerSvc logsSvc)
        {
            _logger = logger;
            _mapper = mapper;
            _logsSvc = logsSvc;
        }

        // Temporally AllowAnon
        // todo: add prettier binding from the url for logsParams
        //[Authorize(Policy = AccountsPolicies.DefaultRights, AuthenticationSchemes = "Identity.Application,Bearer")]
        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllAsync([FromQuery] LogsParametersViewModel logsParams, [FromQuery] PageParametersViewModel pageParams)
        {
            _logger.LogInformation(LogEventType.LogsRetrievingAttempt, _userId, _userEmail);

            var result = await _logsSvc.GetAllAsync(logsParams, pageParams);

            var model = _mapper.Map<PageViewModel<LoggingRecordViewModel>>(result);

            _logger.LogInformation(LogEventType.LogsRetrieved, _userId, _userEmail);

            return Ok(model);
        }
    }
}
