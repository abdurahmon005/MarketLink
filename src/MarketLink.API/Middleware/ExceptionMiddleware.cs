using MarketLink.API.Common;
using MarketLink.API.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text.Json;

namespace MarketLink.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                NotFoundException           => (HttpStatusCode.NotFound,            exception.Message),
                ForbiddenException          => (HttpStatusCode.Forbidden,           exception.Message),
                BadRequestException         => (HttpStatusCode.BadRequest,          exception.Message),
                ConflictException           => (HttpStatusCode.Conflict,            exception.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized,        exception.Message),
                SecurityTokenException      => (HttpStatusCode.Unauthorized,        "Token yaroqsiz yoki muddati o'tgan"),
                ArgumentException           => (HttpStatusCode.BadRequest,          exception.Message),
                _                           => (HttpStatusCode.InternalServerError, "Ichki server xatosi yuz berdi")
            };

            if (statusCode == HttpStatusCode.InternalServerError)
                _logger.LogError(exception, "Unhandled exception at {Path}", context.Request.Path);

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = message
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = (int)statusCode;

            await context.Response.WriteAsync(JsonSerializer.Serialize(response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}
