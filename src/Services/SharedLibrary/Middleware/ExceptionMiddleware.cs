using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SharedLibrary.CustomExceptions;
using SharedLibrary.Models;

namespace SharedLibrary.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private static async Task HandleException(HttpContext context, Exception ex)
    {
        var response = new ErrorResponse();

        switch (ex)
        {
            case BadRequestException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = ex.Message;
                break;

            case NotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = ex.Message;
                break;

            case UnauthorizedException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = ex.Message;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "Internal Server Error";
                response.Details = ex.Message;
                break;
        }

        response.StatusCode = context.Response.StatusCode;

        context.Response.ContentType = "application/json";

        var result = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(result);
    }
}