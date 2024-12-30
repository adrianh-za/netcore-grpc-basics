using Grpc.Core.Interceptors;

namespace Grpc.Client;

public class ClientLoggerInterceptor: Interceptor
{
    public override TResponse BlockingUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        Console.WriteLine($"  CLIENT INTERCEPT: the call of type {context.Method.FullName} to {context.Host} intercepted");
        return continuation(request, context);
    }
}