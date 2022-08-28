using Common.CustomExceptions;
using System.Net;
using System.Text.Json;
using LogsViewerService.Utilities;

namespace LogsViewerService.Exceptions
{
    internal class GlobalExceptionsHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionsHandler> _logger;
        public GlobalExceptionsHandler(RequestDelegate next, ILogger<GlobalExceptionsHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                switch(exception)
                {
                    case NotFoundException:
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;

                    case InvalidParamsException:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;

                    case UnauthorizedException:
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        break;

                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        _logger.LogError(LogEventType.ExceptionForm, exception.GetType().Name, response.StatusCode, exception.Message);
                        break;
                }

                var exceptionResponse = new
                {
                    exceptionType = exception.GetType().Name,
                    statusCode = response.StatusCode,
                    message = exception.Message,                    
                };

                var result = JsonSerializer.Serialize(exceptionResponse);
                await response.WriteAsync(result);
            }
        }

    }
}
