using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Phaeton.gRPC.Interceptors;

public class ClientLoggerInterceptor : Interceptor
{
    private readonly ILogger _logger;

    public ClientLoggerInterceptor(ILogger logger)
        => _logger = logger;

    public override AsyncUnaryCall<K> AsyncUnaryCall<T, K>(
        T req,
        ClientInterceptorContext<T, K> ctx,
        AsyncUnaryCallContinuation<T, K> continuation
    )
    {
        _logger.LogInformation($"Starting call. Type: {ctx.Method.Type}. Method: {ctx.Method.Name}.");
        
        return continuation(
            req,
            ctx
        );
    }
}