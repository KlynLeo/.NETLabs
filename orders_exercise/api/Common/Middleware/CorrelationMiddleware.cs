using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationHeader = "X-Correlation-ID";

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationHeader, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString(); 
        }
        context.Items[CorrelationHeader] = correlationId;
        context.Response.Headers[CorrelationHeader] = correlationId;
        await _next(context);
    }
}
