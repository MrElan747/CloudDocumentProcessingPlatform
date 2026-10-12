using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace SmartDoc.API.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync( HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var traceId = httpContext.TraceIdentifier;

            var (statusCode, title, detail, logLevel) = MapException(exception);

            _logger.Log( logLevel, exception, "Request failed. TraceId: {TraceId}", traceId);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["traceId"] = traceId;

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);

            return true;
        }

        private static (
            int StatusCode,
            string Title,
            string Detail,
            LogLevel LogLevel) MapException(Exception exception)
        {
            return exception switch
            {
                BadRequestException ex =>
                    (
                        StatusCodes.Status400BadRequest,
                        "Bad Request",
                        ex.Message,
                        LogLevel.Warning
                    ),

                NotFoundException ex =>
                    (
                        StatusCodes.Status404NotFound,
                        "Not Found",
                        ex.Message,
                        LogLevel.Information
                    ),

                ConflictException ex =>
                    (
                        StatusCodes.Status409Conflict,
                        "Conflict",
                        ex.Message,
                        LogLevel.Warning
                    ),

                DbUpdateException ex when IsUniqueConstraintViolation(ex) =>
                    (
                        StatusCodes.Status409Conflict,
                        "Conflict",
                        "The requested operation conflicts with existing data.",
                        LogLevel.Warning
                    ),

                DbUpdateException =>
                    (
                        StatusCodes.Status500InternalServerError,
                        "Database Error",
                        "A database error occurred while processing your request.",
                        LogLevel.Error
                    ),

                SqlException =>
                    (
                        StatusCodes.Status500InternalServerError,
                        "Database Error",
                        "A database error occurred while processing your request.",
                        LogLevel.Error
                    ),

                UnauthorizedAccessException =>
                    (
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized",
                        "You are not authorized to perform this operation.",
                        LogLevel.Warning
                    ),

                OperationCanceledException =>
                    (
                        StatusCodes.Status499ClientClosedRequest,
                        "Request Cancelled",
                        "The request was cancelled.",
                        LogLevel.Information
                    ),

                _ =>
                    (
                        StatusCodes.Status500InternalServerError,
                        "Internal Server Error",
                        "An unexpected error occurred.",
                        LogLevel.Error
                    )
            };
        }

        private static bool IsUniqueConstraintViolation(
            DbUpdateException exception)
        {
            if (exception.InnerException is SqlException sqlException)
            {
                return sqlException.Number is 2601 or 2627;
            }

            return false;
        }
    }
}
