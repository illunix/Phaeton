using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Phaeton.gRPC.Interceptors;

public class ServerLoggerInterceptor : Interceptor
{
    private readonly ILogger _logger;

    public ServerLoggerInterceptor(ILogger<ServerLoggerInterceptor> logger)
        => _logger = logger;

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest req,
        ServerCallContext ctx,
        UnaryServerMethod<TRequest, TResponse> continuation
    )
    {
        _logger.LogInformation($"Starting receiving call. Type: {MethodType.Unary}. Method: {ctx.Method}.");
        
        try
        {
            return await continuation(
                req, 
                ctx
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                $"Error thrown by {ctx.Method}."
            );
            throw;
        }
    }
}