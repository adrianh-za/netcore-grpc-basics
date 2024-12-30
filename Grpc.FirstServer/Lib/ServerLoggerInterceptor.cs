using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Grpc.FirstServer.Lib;

public class ServerLoggerInterceptor(ILogger<ServerLoggerInterceptor> logger) : Interceptor
{
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        logger.LogInformation("SERVER INTERCEPT: intercepted the call of type {method} from {peer}", context.Method, context.Peer);
        return continuation(request, context);
    }
}