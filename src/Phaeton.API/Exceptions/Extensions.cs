using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Phaeton.API.Exceptions.Interfaces;
using Phaeton.API.Exceptions.Mappers;
using Phaeton.API.Exceptions.Middlewares;

namespace Phaeton.API.Exceptions;

public static class Extensions
{
    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
        => services
            .AddSingleton<ErrorHandlerMiddleware>()
            .AddSingleton<IExceptionToResponseMapper, ExceptionToResponseMapper>();

    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ErrorHandlerMiddleware>();
}