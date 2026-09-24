using Serilog.Context;
using Serilog;
using System.Diagnostics;
using System.Text.Json;
using GateOfEgypt.API.Dtos;

namespace GateOfEgypt.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;
        private readonly bool _showDetails;

        public ExceptionMiddleware(RequestDelegate next, IWebHostEnvironment env, IConfiguration configuration)
        {
            _next = next;
            _env = env;
            // Set "ErrorHandling:ShowDetails": true temporarily on the host to see the exception in the response.
            _showDetails = env.IsDevelopment() || configuration.GetValue<bool>("ErrorHandling:ShowDetails");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew(); // بدء قياس الوقت

            try
            {
                await _next(context);

                if (context.Response.StatusCode == 403)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"Message\": \"Access Denied: You do not have the required permissions to access this resource.\"}");
                }
                else if (context.Response.StatusCode == 401)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"Message\": \"Unauthorized: Please log in to access this resource.\"}");
                }
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    LogError(context, ex, stopwatch.Elapsed.TotalMilliseconds);
                    throw;
                }

                context.Response.StatusCode = StatusCodes.Status500InternalServerError; // التأكد من Status Code 500
                LogError(context, ex, stopwatch.Elapsed.TotalMilliseconds);

                context.Response.ContentType = "application/json";
                var response = _showDetails
                              ? new ErrorResponse { Message = "Internal Server Error", Details = ex.ToString() }
                              : new ErrorResponse { Message = "An unexpected error occurred. Please try again later.", Details = null };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            finally
            {
                stopwatch.Stop(); // إيقاف قياس الوقت
            }
        }

        private void LogError(HttpContext context, Exception ex, double elapsedMilliseconds)
        {
            using (LogContext.PushProperty("RequestPath", context.Request.Path))
            using (LogContext.PushProperty("RequestMethod", context.Request.Method))
            using (LogContext.PushProperty("StatusCode", context.Response.StatusCode))
            using (LogContext.PushProperty("Elapsed", elapsedMilliseconds))
            using (LogContext.PushProperty("User", context.User?.Identity?.Name ?? "Anonymous"))
            using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
            using (LogContext.PushProperty("SpanId", Activity.Current?.SpanId.ToString() ?? "N/A"))
            {
                Log.Error(ex, "An error occurred while processing the request.");
            }
        }
    }
}
