using AccountsService.Constants.Logger;
using AccountsService.Exceptions.CustomExceptions;
using AccountsService.Utilities;
using System.Net;
using System.Text.Json;

namespace AccountsService.Exceptions
{
    public class GlobalExceptionsHandler
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
                        _logger.LogTrace(LoggingForms.ExceptionForm, exception.GetType().Name, response.StatusCode, exception.Message);
                        break;

                    case InvalidParamsException:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        _logger.LogTrace(LoggingForms.ExceptionForm, exception.GetType().Name, response.StatusCode, exception.Message);
                        break;

                    case UnauthorizedException:
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        _logger.LogTrace(LoggingForms.ExceptionForm, exception.GetType().Name, response.StatusCode, exception.Message);
                        break;

                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        _logger.LogWarning(LoggingForms.ExceptionForm, exception.GetType().Name, response.StatusCode, exception.Message);
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
