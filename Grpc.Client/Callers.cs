using System.Text;
using Basics;
using Grpc.Core;

namespace Grpc.Client;

public static class Callers
{
    public static string CallUnary(string content, FirstServiceDefinition.FirstServiceDefinitionClient client)
    {
        var responseUnary = client.Unary(new Request() { Content = content });
        return responseUnary.Message;
    }

    public static string CallUnaryZipped(string content, FirstServiceDefinition.FirstServiceDefinitionClient client)
    {
        //Not needed as on by default, but just to show it actually is added to HTTP request
        //The http header is required though for compresion to work.  And the server must support it.
        var metadata = new Metadata
        {
            { "grpc-accept-encoding", "gzip" },
        };
        var responseUnary = client.Unary(new Request() { Content = content }, headers: metadata);
        return responseUnary.Message;
    }

    public static string CallUnaryDeadline(string content, FirstServiceDefinition.FirstServiceDefinitionClient client)
    {
        var responseUnary = client.Unary(
            new Request() { Content = content },
            deadline: DateTime.UtcNow.AddMilliseconds(100)
        );
        return responseUnary.Message;
    }

    public static string CallUnarySetHeaders(string content, FirstServiceDefinition.FirstServiceDefinitionClient client)
    {
        var metadata = new Metadata
        {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" },
        };

        var responseUnary = client.Unary(
            new Request() { Content = content },
            headers: metadata
        );
        return responseUnary.Message;
    }

    public static string CallUnarySetRetry(string content, FirstServiceDefinition.FirstServiceDefinitionClient client)
    {
        var metadata = new Metadata
        {
            { "internal-fail", "true" },
        };

        var responseUnary = client.Unary(
            new Request() { Content = content },
            headers: metadata
        );
        return responseUnary.Message;
    }

    public static async Task<string> CallUnaryGetTrailers(string content, FirstServiceDefinition.FirstServiceDefinitionClient client)
    {
        var responseUnary = client.UnaryAsync(
            new Request() { Content = content }
        );

        var message = await responseUnary.ResponseAsync;

        var trailersString = new StringBuilder();
        foreach (var entry in responseUnary.GetTrailers())
        {
            trailersString.AppendLine($"{entry.Key}: {entry.Value}");
        }

        return $"{message.Message}\nTrailers:\n{trailersString}";
    }

    public static async Task<string> CallClientStream(string content, FirstServiceDefinition.FirstServiceDefinitionClient client)
    {
        using var caller = client.ClientStream();

        //Post the data (streamed)
        foreach (var character in content)
        {
            await caller.RequestStream.WriteAsync(new Request() { Content = character.ToString() });
        }

        await caller.RequestStream.CompleteAsync();

        //Get the response
        var response = await caller.ResponseAsync;
        return response.Message;
    }

    public static async Task<string> CallServerStream(string content, FirstServiceDefinition.FirstServiceDefinitionClient client)
    {
        //Post the data
        using var caller = client.ServerStream(new Request() { Content = content });

        //Get the response (streamed)
        var builder = new StringBuilder();
        await foreach (var response in caller.ResponseStream.ReadAllAsync())
        {
            builder.Append(response.Message);
        }

        return builder.ToString();
    }

    public static async Task<string> CallServerStreamCancel(string content, FirstServiceDefinition.FirstServiceDefinitionClient client)
    {
        //Post the data
        using var caller = client.ServerStream(new Request() { Content = content });

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        //Get the response (streamed)
        var builder = new StringBuilder();
        await foreach (var response in caller.ResponseStream.ReadAllAsync(cancellationTokenSource.Token))
        {
            builder.Append(response.Message);
        }

        return builder.ToString();
    }

    public static async Task<string> CallServerStreamException(string content, FirstServiceDefinition.FirstServiceDefinitionClient client)
    {
        try
        {
            //Post the data
            using var caller = client.ServerStream(new Request() { Content = content });

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            //Get the response (streamed)
            var builder = new StringBuilder();
            await foreach (var response in caller.ResponseStream.ReadAllAsync(cancellationTokenSource.Token))
            {
                builder.Append(response.Message);
            }

            return builder.ToString();
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            throw new Exception("The call was cancelled, so this is a custom exception.", ex);
        }
    }

    public static async Task<string> CallBiDirectionalStream(string content, FirstServiceDefinition.FirstServiceDefinitionClient client)
    {
        using var caller = client.BiDirectionalStream();

        //Post the data (streamed)
        foreach (var character in content)
        {
            await caller.RequestStream.WriteAsync(new Request() { Content = character.ToString() });
        }

        await caller.RequestStream.CompleteAsync();

        //Get the response (streamed)
        var builder = new StringBuilder();
        await foreach (var response in caller.ResponseStream.ReadAllAsync())
        {
            builder.Append(response.Message);
        }

        return builder.ToString();
    }
}