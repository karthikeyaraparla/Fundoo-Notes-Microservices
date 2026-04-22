using Microsoft.AspNetCore.Builder;
using SharedLibrary.Middleware;

namespace SharedLibrary.Extensions;

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}