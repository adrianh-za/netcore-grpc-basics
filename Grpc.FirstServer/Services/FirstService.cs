using System.Diagnostics;
using System.Globalization;
using Basics;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Grpc.FirstServer.Services;

//NOTE: Only BiDirectionalStream has authorization

public class FirstService:  FirstServiceDefinition.FirstServiceDefinitionBase, IFirstService
{
    public override async Task<Response> Unary(Request request, ServerCallContext context)
    {
        //This is so we can test retry policy
        var internalFail = context.RequestHeaders.Any(c => c.Key == "internal-fail");
        if (internalFail)
        {
            var retryCount = context.RequestHeaders.SingleOrDefault(c => c.Key == "grpc-previous-rpc-attempts")?.Value ?? "0";
            throw new RpcException(new Status(StatusCode.Internal, $"An internal error to test retry occured.  Current request try: {int.Parse(retryCount) + 1}."));
        }

        //Process the request
        var sw = new Stopwatch();
        sw.Start();
        var response = new Response
        {
            Message = $"Hello {request.Content} from Unary with {context.RequestHeaders.Count} headers."
        };
        sw.Stop();

        //Just showing how to add a trailer
        context.ResponseTrailers.Add("Timestamp", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
        context.ResponseTrailers.Add("Duration", sw.ElapsedMilliseconds.ToString());

        //To disable compression
        //context.WriteOptions = new WriteOptions(WriteFlags.NoCompress);

        await Task.Delay(500);  //Used for testing deadlines
        return response;
    }

    public override async Task<Response> ClientStream(IAsyncStreamReader<Request> requestStream, ServerCallContext context)
    {
        var content = string.Empty;

        //Preferred option
        await foreach (var request in requestStream.ReadAllAsync())
        {
            content += request.Content;
        }

        //Other option
        // while (await requestStream.MoveNext())
        // {
        //     content += requestStream.Current.Content;
        // }

        await Task.Delay(500);  //Used for testing deadlines
        return new Response
        {
            Message = $"Hello {content} from ClientStream with {context.RequestHeaders.Count} headers."
        };
    }

    public override async Task ServerStream(Request request, IServerStreamWriter<Response> responseStream, ServerCallContext context)
    {
        await responseStream.WriteAsync(new Response { Message = "Hello " });
        foreach (var t in request.Content)
        {
            await responseStream.WriteAsync(new Response { Message = t.ToString() });
        }

        await Task.Delay(500);  //Used for testing deadlines
        await responseStream.WriteAsync(new Response { Message = $" from ServerStream with {context.RequestHeaders.Count} headers." });
    }

    [Authorize]
    public override async Task BiDirectionalStream(IAsyncStreamReader<Request> requestStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
    {
        var content = string.Empty;
        await foreach (var request in requestStream.ReadAllAsync())
        {
            content += request.Content;
        }

        await Task.Delay(500);  //Used for testing deadlines

        await responseStream.WriteAsync(new Response { Message = "Hello " });
        foreach (var t in content)
        {
            await responseStream.WriteAsync(new Response { Message = t.ToString() });
        }
        await responseStream.WriteAsync(new Response { Message = $" from BiDirectionalStream with {context.RequestHeaders.Count} headers." });
    }
}