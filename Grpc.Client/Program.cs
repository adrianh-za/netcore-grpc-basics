using Basics;
using Google.Protobuf.Reflection;
using Grpc.Client;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Grpc.Reflection.V1Alpha;
using Grpc.Shared;

const string baseUrl = "https://localhost:7191";

//Reflect the Grpc server
using var channelReflection = GrpcChannel.ForAddress(baseUrl);
var clientReflection = new ServerReflection.ServerReflectionClient(channelReflection);
var reflectionRequest = new ServerReflectionRequest { ListServices = "" };
using var reflectionCall = clientReflection.ServerReflectionInfo();
await reflectionCall.RequestStream.WriteAsync(reflectionRequest);
await reflectionCall.RequestStream.CompleteAsync();

//Get the reflected services
var reflectedServices = new Dictionary<ServiceResponse, List<string>>();
while (await reflectionCall.ResponseStream.MoveNext())
{
    foreach(var service in reflectionCall.ResponseStream.Current.ListServicesResponse.Service)
    {
        reflectedServices.Add(service, []); //Methods to be added below
    }
}

//Get the methods of the services retreived
foreach (var service in reflectedServices)
{
    var methodsReflectionRequest = new ServerReflectionRequest { FileContainingSymbol = service.Key.Name};
    using var methodReflectionCall = clientReflection.ServerReflectionInfo();
    await methodReflectionCall.RequestStream.WriteAsync(methodsReflectionRequest);
    await methodReflectionCall.RequestStream.CompleteAsync();

    while (await methodReflectionCall.ResponseStream.MoveNext())
    {
        var descriptorResponse = methodReflectionCall.ResponseStream.Current.FileDescriptorResponse;
        var fileDescriptors = FileDescriptor.BuildFromByteStrings(descriptorResponse.FileDescriptorProto.Reverse());
        var serviceDescriptor = fileDescriptors
            .SelectMany(x => x.Services)
            .FirstOrDefault(x => x.FullName == service.Key.Name);
        if (serviceDescriptor is not null)
            service.Value.AddRange(serviceDescriptor.Methods.Select(x => x.Name));
    }
}

//Print the reflected services and methods
Console.WriteLine("************************************");
Console.WriteLine("Reflected services:");
foreach (var service in reflectedServices)
{
    Console.WriteLine($"Service: {service.Key.Name}");
    foreach (var method in service.Value)
    {
        Console.WriteLine($"  * Method: {method}");
    }
}
Console.WriteLine();



//Token generation for the Grpc call
var credentials = CallCredentials.FromInterceptor((context, metadata) =>
{
    var token = JwtHelper.GenerateJwtToken("Adrian the Developer");
    metadata.Add("Authorization", $"Bearer {token}");
    return Task.CompletedTask;
});

//Create the retry policy  (Swap out the hedging policy above with this one to test)
var retryPolicy = new MethodConfig()
{
    Names = { MethodName.Default },
    RetryPolicy = new RetryPolicy()
    {
        MaxAttempts = 5,
        BackoffMultiplier = 1.5,
        InitialBackoff = TimeSpan.FromSeconds(1),
        MaxBackoff = TimeSpan.FromSeconds(2),
        RetryableStatusCodes = { StatusCode.Internal }
    }
};

//Create the hedging policy (Swap out the retry policy above with this one to test)
var hedgePolicy = new MethodConfig()
{
    Names = { MethodName.Default },
    HedgingPolicy = new HedgingPolicy()
    {
        MaxAttempts = 5,
        NonFatalStatusCodes = { StatusCode.Internal },
        HedgingDelay = TimeSpan.FromMilliseconds(500)
    }
};

//Create the options
var channelOptions = new GrpcChannelOptions()
{
    ServiceConfig = new ServiceConfig { MethodConfigs = { retryPolicy } },
    Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
};

//Create the channel
using var channel = GrpcChannel.ForAddress(baseUrl, channelOptions);

//Health check client
var healthClient = new Health.HealthClient(channel);
var healthResponse = await healthClient.CheckAsync(new HealthCheckRequest());
Console.WriteLine("************************************");
Console.WriteLine($"Health check response: {healthResponse.Status}");
Console.WriteLine();

//First service client
//var firstServiceDefinitionClient = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);
var callInvoker = channel.Intercept(new ClientLoggerInterceptor()); //Can also be done in the options in services/di
var firstServiceDefinitionClient = new FirstServiceDefinition.FirstServiceDefinitionClient(callInvoker);

Console.WriteLine("************************************");
Console.WriteLine("Call services:");
//Do the calls
const string name = "Adrian the Developer";
Console.WriteLine(Callers.CallUnary(name, firstServiceDefinitionClient));
Console.WriteLine(await Callers.CallClientStream(name, firstServiceDefinitionClient));
Console.WriteLine(await Callers.CallServerStream(name, firstServiceDefinitionClient));
Console.WriteLine(await Callers.CallBiDirectionalStream(name, firstServiceDefinitionClient));   //Has authorization
Console.WriteLine(Callers.CallUnarySetHeaders(name, firstServiceDefinitionClient));
Console.WriteLine(await Callers.CallUnaryGetTrailers(name, firstServiceDefinitionClient));
Console.WriteLine(Callers.CallUnaryZipped(name, firstServiceDefinitionClient));

//Deadline
try
{
    Callers.CallUnaryDeadline(name, firstServiceDefinitionClient); //Will fail with deadline exceeded
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

//Cancel
try
{
    await Callers.CallServerStreamCancel(name, firstServiceDefinitionClient); //Will fail with cancel
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

//Cancel - custom exception
try
{
    await Callers.CallServerStreamException(name, firstServiceDefinitionClient); //Will fail with cancel
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

//Retry - custom header
try
{
    Callers.CallUnarySetRetry(name, firstServiceDefinitionClient); //Will fail after retries as defined in policy
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}