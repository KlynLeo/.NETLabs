using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
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

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string message = exception.Message;

            switch (exception)
            {
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound; // 404
                    break;

                case ArgumentNullException:
                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest; // 400
                    break;

                case InvalidOperationException:
                    statusCode = HttpStatusCode.Conflict; // 409
                    break;

                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized; // 401
                    break;

                case NotImplementedException:
                    statusCode = HttpStatusCode.NotImplemented; // 501
                    break;

            }

            var response = new
            {
                statusCode = (int)statusCode,
                message
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            string json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }
}
